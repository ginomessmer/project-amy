using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using System.IO;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using System.Security.Cryptography;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace ProjectAmy.Server
{
    public class Notification
    {
        private CryptographyClient _cryptoClient;

        public Notification(CryptographyClient cryptoClient)
        {
            _cryptoClient = cryptoClient;
        }

        [FunctionName(nameof(Notification))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
           
            var validationToken = req.Query["validationToken"];
            if (validationToken.Any())
            {
                return ValidateNewSubscription(validationToken);
            } else
            {

                var changeNotifications = await ParseNotificationAsync(req);
                log.LogInformation(JsonConvert.SerializeObject(changeNotifications));
                return await HandleNotificationReceivedAsync(changeNotifications, log);
            }
            
        }

        private IActionResult ValidateNewSubscription(String validationToken)
        {
            var decodedToken = HttpUtility.UrlDecode(validationToken);
            return new OkObjectResult(decodedToken)
            {
                ContentTypes = new MediaTypeCollection
                    {
                        "text/plain"
                    }
            };
        }

        private async Task<ChangeNotificationCollection> ParseNotificationAsync(HttpRequest request)
        {
            /*var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));*/
            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            return JsonConvert.DeserializeObject<ChangeNotificationCollection>(requestBody);
        }

        private async Task<IActionResult> HandleNotificationReceivedAsync(ChangeNotificationCollection changeNotifications, ILogger logger)
        {
            foreach(ChangeNotification changeNotification in changeNotifications.Value)
            {
                   var dataKey = changeNotification.EncryptedContent.DataKey;
                if (dataKey != null)
                {
                    var dataKeyBytes = Convert.FromBase64String(dataKey.ToString());
                    logger.LogInformation("dataKeyBytes");
                    /*DecryptParameters decryptParameters = DecryptParameters.RsaOaepParameters(dataKeyBytes);
                    DecryptResult decryptedKey = await _cryptoClient.DecryptAsync(decryptParameters);*/
                    DecryptResult decryptedKey = await _cryptoClient.DecryptAsync(EncryptionAlgorithm.RsaOaep, dataKeyBytes);


                    byte[] encryptedPayload = Convert.FromBase64String(changeNotification.EncryptedContent.Data);
                    byte[] expectedSignature = Convert.FromBase64String(changeNotification.EncryptedContent.DataSignature);
                    byte[] actualSignature;

                    using (HMACSHA256 hmac = new HMACSHA256(decryptedKey.Plaintext))
                    {
                        actualSignature = hmac.ComputeHash(encryptedPayload);
                        logger.LogInformation("actualSignature");

                    }
                    if (actualSignature.SequenceEqual(expectedSignature))
                    {
                        // Continue with decryption of the encryptedPayload.

                        AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider();
                        aesProvider.Key = decryptedKey.Plaintext;
                        aesProvider.Padding = PaddingMode.PKCS7;
                        aesProvider.Mode = CipherMode.CBC;

                        // Obtain the intialization vector from the symmetric key itself.
                        int vectorSize = 16;
                        byte[] iv = new byte[vectorSize];
                        Array.Copy(decryptedKey.Plaintext, iv, vectorSize);
                        aesProvider.IV = iv;
                        logger.LogInformation("Obtained the intialization vector from the symmetric key itself");


                        string decryptedResourceData;
                        // Decrypt the resource data content.
                        using (var decryptor = aesProvider.CreateDecryptor())
                        {
                            logger.LogInformation("decryptor ready");

                            using (MemoryStream msDecrypt = new MemoryStream(encryptedPayload))
                            {
                                logger.LogInformation("memory stram ready");

                                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                                {
                                    logger.LogInformation("crypto stream ready");

                                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                                    {
                                        logger.LogInformation("stream reader ready");

                                        decryptedResourceData = srDecrypt.ReadToEnd();
                                        logger.LogInformation("handle notification:");
                                        logger.LogInformation(JsonConvert.SerializeObject(decryptedResourceData));
                                    }
                                }
                            }
                        }

                        // decryptedResourceData now contains a JSON string that represents the resource.
                    }
                    else
                    {
                        // Do not attempt to decrypt encryptedPayload. Assume notification payload has been tampered with and investigate.
                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    }


                    return new OkObjectResult("");

                }
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);


            }
            return new OkObjectResult("");
        }


    }
}
