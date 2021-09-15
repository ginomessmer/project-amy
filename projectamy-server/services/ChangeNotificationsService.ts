import { Logger } from '@azure/functions';
import { DefaultAzureCredential } from '@azure/identity';
import { CryptographyClient, RsaDecryptParameters} from '@azure/keyvault-keys';
import {ChangeNotificationCollection, ChangeNotification,ChatMessage, ResourceData, ChatMessageReaction} from '@microsoft/microsoft-graph-types';
import {createHmac, BinaryLike, KeyObject, createDecipheriv, CipherKey} from 'crypto';
import { IReaction } from '../models/IReaction';


function isChatMessage(object: any): object is ChatMessage {
    return 'body' in object && 'messageType' in object ;
}


export class ChangeNotificationsService {

    private static readonly MESSAGE_ENTITY_ODATA_CONTEXT = "/messages/$entity";
    private cryptographyClient: CryptographyClient;

    constructor(){
        this.cryptographyClient = new CryptographyClient(process.env.AZURE_KEY_VAULT_DECRYPTION_KEY_URL ,new DefaultAzureCredential() );
    
    }

    public async handleNotificationReceivedAsync(changeNotificationCollection: ChangeNotificationCollection): Promise<IReaction[]> {
        await this.decryptEncryptedChangeNotifications(changeNotificationCollection);
        const reactions: IReaction[] = [];
        for (const changeNotification of changeNotificationCollection.value) {
            if(changeNotification.resourceData){
                const resourceData: ResourceData = changeNotification.resourceData;
                if(isChatMessage(resourceData)){
                    const message: ChatMessage = resourceData as ChatMessage;
                    const isMessageWithReaction = message.messageType === 'message' && message.reactions && message.reactions.length > 0;
                    if(isMessageWithReaction){
                        const isReactionReasonForUpdate = message.lastModifiedDateTime && message.reactions.some(reaction => reaction.createdDateTime === message.lastModifiedDateTime);
                        if(isReactionReasonForUpdate){
                            message.reactions.filter(reaction => reaction.createdDateTime === message.lastModifiedDateTime).forEach(reaction => {
                                const reactionHasUser = reaction.user.user && reaction.user.user.displayName;
                                reactions.push({
                                    reactionType: reaction.reactionType,
                                    name: reactionHasUser ?reaction.user.user.displayName: null
                                });
                            });
                        }
                    }

                }
            }
        }

        return reactions;

        
    }

    private async decryptEncryptedChangeNotifications(changeNotificationCollection: ChangeNotificationCollection) {
        for (const changeNotification of changeNotificationCollection.value) {
            if (changeNotification.encryptedContent) {
                const dataKey = changeNotification.encryptedContent.dataKey;

                if (dataKey !== null && dataKey !== undefined) {
                    const ciphertext = Buffer.from(dataKey, 'base64');
                    const decryptParameters: RsaDecryptParameters = {
                        algorithm: 'RSA-OAEP',
                        ciphertext: ciphertext
                    };
                    const decryptedKey = await this.cryptographyClient.decrypt(decryptParameters);
                    if (this.dataSignatureEquals(decryptedKey.result, changeNotification.encryptedContent.dataSignature, changeNotification.encryptedContent.data)) {
                        const decryptedResourceData = this.decryptResourceDataContent(decryptedKey.result, changeNotification.encryptedContent.data);
                        changeNotification.resourceData = JSON.parse(decryptedResourceData);
                    } else {
                        throw new Error('Data signature does not match');
                    }
                }
            }

        }
    }

    private dataSignatureEquals(key: BinaryLike | KeyObject, expectedSignature: string, base64encodedPayload) {
        /* const decryptedSymetricKey = []; //Buffer provided by previous step
const  = 'base64 encodded value from the dataSignature property';
const hmac = crypto.createHmac('sha256', decryptedSymetricKey);
hmac.write(base64encodedPayload, 'base64');
if(base64encodedSignature === hmac.digest('base64'))
{
    // Continue with decryption of the encryptedPayload.
}
else
{
    // Do not attempt to decrypt encryptedPayload. Assume notification payload has been tampered with and investigate.
}
*/

        const hmac = createHmac('sha256', key);
        hmac.write(base64encodedPayload, 'base64');
        return expectedSignature === hmac.digest('base64');
    }

    private decryptResourceDataContent(decryptedSymetricKey, base64encodedPayload: string){
        const iv = Buffer.alloc(16, 0);
        decryptedSymetricKey.copy(iv, 0, 0, 16);
        const decipher = createDecipheriv('aes-256-cbc', decryptedSymetricKey, iv);
        let decryptedPayload = decipher.update(base64encodedPayload, 'base64', 'utf8');
        decryptedPayload += decipher.final('utf8');
        return decryptedPayload;
    }
}
