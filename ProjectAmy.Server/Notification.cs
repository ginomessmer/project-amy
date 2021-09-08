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

namespace ProjectAmy.Server
{
    public class Notification
    {
       
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
                return HandleNotificationReceived(changeNotifications, log);
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

        private IActionResult HandleNotificationReceived(ChangeNotificationCollection changeNotifications, ILogger logger)
        {

            logger.LogInformation("handle notification:");
            logger.LogInformation(JsonConvert.SerializeObject(changeNotifications));
            return new OkObjectResult("");
            
        }


    }
}
