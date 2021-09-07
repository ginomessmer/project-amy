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
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Graph;

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
            var plainNotifications = new Dictionary<string, ChangeNotification>();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            return await System.Text.Json.JsonSerializer.DeserializeAsync<ChangeNotificationCollection>(request.Body, options);
        }

        private IActionResult HandleNotificationReceived(ChangeNotificationCollection changeNotifications, ILogger logger)
        {

            logger.LogInformation("handle notification {notifications}", changeNotifications);
            return new OkObjectResult("");
            
        }


    }
}
