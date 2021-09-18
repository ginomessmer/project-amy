using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAmy.ClientWorker.Options
{
    public class AmyClientOptions
    {
        /// <summary>
        /// The endpoint for registering notifications. Should also include the code as a query parameter.
        /// </summary>
        public string FunctionsNotificationsEndpoint { get; set; }

        /// <summary>
        /// TODO: @Malte
        /// </summary>
        public string PublicKeyCertificatePath { get; set; } = Path.Combine(Environment.CurrentDirectory, "cert.pem");
    }
}
