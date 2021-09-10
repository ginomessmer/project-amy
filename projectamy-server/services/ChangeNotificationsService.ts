import { DefaultAzureCredential } from '@azure/identity';
import { CryptographyClient, RsaDecryptParameters} from '@azure/keyvault-keys';
import {ChangeNotificationCollection, ChangeNotification, ResourceData} from '@microsoft/microsoft-graph-types';
import {createHmac, BinaryLike, KeyObject, createDecipheriv, CipherKey} from 'crypto';

export class ChangeNotificationsService {

    private cryptographyClient: CryptographyClient;

    constructor(){
        this.cryptographyClient = new CryptographyClient(process.env.AZURE_KEY_VAULT_DECRYPTION_KEY_URL ,new DefaultAzureCredential() );
    
    }

    public async handleNotificationReceivedAsync(changeNotificationCollection: ChangeNotificationCollection) {
        await this.decryptEncryptedChangeNotifications(changeNotificationCollection);

        // TODO handle change notifications
        for (const changeNotification of changeNotificationCollection.value) {
            console.dir(changeNotification);

        }


        
    }

    private async decryptEncryptedChangeNotifications(changeNotificationCollection: ChangeNotificationCollection) {
        for (const changeNotification of changeNotificationCollection.value) {
            if (changeNotification.encryptedContent) {
                const dataKey = changeNotification.encryptedContent.dataKey;

                if (dataKey !== null && dataKey !== undefined) {
                    const ciphertext = Buffer.from(dataKey);
                    const decryptParameters: RsaDecryptParameters = {
                        algorithm: 'RSA-OAEP',
                        ciphertext: ciphertext
                    };
                    const decryptedKey = await this.cryptographyClient.decrypt(decryptParameters);
                    if (this.dataSignatureEquals(decryptedKey.result, changeNotification.encryptedContent.dataSignature)) {
                        const decryptedResourceData = this.decryptResourceDataContent(decryptedKey.result, changeNotification.encryptedContent.data);
                        changeNotification.resourceData = decryptedResourceData;
                    } else {
                        throw new Error('Data signature does not match');
                    }
                }
            }

        }
    }

    private dataSignatureEquals(key: BinaryLike | KeyObject, expectedSignature) {

       /* const decryptedSymetricKey = []; //Buffer provided by previous step
const base64encodedSignature = 'base64 encodded value from the dataSignature property';
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
        hmac.write(expectedSignature);
        return expectedSignature === hmac.digest('base64')
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
