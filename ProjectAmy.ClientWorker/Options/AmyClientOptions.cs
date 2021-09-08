using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAmy.ClientWorker.Options
{
    public class AmyClientOptions
    {
        public string FunctionsNotificationsEndpoint { get; set; }

        public string TeamId { get; set; }

        public string ChannelId { get; set; }

        public string PublicKey { get; set; }
    }
}
