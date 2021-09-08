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
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Graph;
using System.IO;
using Newtonsoft.Json;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using System.Text;
using System.Security.Cryptography;

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
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            return JsonConvert.DeserializeObject<ChangeNotificationCollection>(requestBody);
        }

        private async Task<IActionResult> HandleNotificationReceivedAsync(ChangeNotificationCollection changeNotifications, ILogger logger)
        {
            var dataKey = changeNotifications.AdditionalData["dataKey"];
            if(dataKey != null)
            {
                var dataKeyBytes = Convert.FromBase64String(dataKey.ToString());
                DecryptParameters decryptParameters = DecryptParameters.RsaOaepParameters(dataKeyBytes);
                DecryptResult decryptedKey = await _cryptoClient.DecryptAsync(decryptParameters);

                byte[] encryptedPayload = Convert.FromBase64String(changeNotifications.AdditionalData["data"].ToString());
                byte[] expectedSignature = Convert.FromBase64String(changeNotifications.AdditionalData["dataSignature"].ToString()) ;
                byte[] actualSignature;

                using (HMACSHA256 hmac = new HMACSHA256(decryptedKey.Plaintext))
                {
                    actualSignature = hmac.ComputeHash(encryptedPayload);
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


                    string decryptedResourceData;
                    // Decrypt the resource data content.
                    using (var decryptor = aesProvider.CreateDecryptor())
                    {
                        using (MemoryStream msDecrypt = new MemoryStream(encryptedPayload))
                        {
                            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                            {
                                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                                {
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


    }
}
