using System;
using System.Collections.Generic;
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
        /// The channel's team ID.
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// The channel ID. The notification endpoint will receive all reactions related to the channel.
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// TODO: @Malte
        /// </summary>
        public string PublicKey { get; set; }
    }
}
