using System;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ProjectAmy.Server
{
    public static class Notification
    {
        [FunctionName(nameof(Notification))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var validationToken = req.Query["validationToken"];
            if (validationToken.Any())
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
            
            return new OkObjectResult("");
        }
    }
}
