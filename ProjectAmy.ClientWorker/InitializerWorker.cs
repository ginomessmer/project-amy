using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace ProjectAmy.ClientWorker
{
    public class InitializerWorker : BackgroundService
    {
        private readonly ILogger<InitializerWorker> _logger;
        private readonly IPublicClientApplication _app;

        public InitializerWorker(ILogger<InitializerWorker> logger, IPublicClientApplication app)
        {
            _logger = logger;
            _app = app;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Sign in
            var accounts = await _app.GetAccountsAsync();
            if (!accounts.Any())
            {
                // Sign in
                var result = await _app.AcquireTokenInteractive(new[] {"ChannelMessage.Read.All"})
                    .ExecuteAsync(stoppingToken);

                _logger.LogInformation("Sign-in successful!");
            }

            // Check subscriptions

            // Register subscription if required
        }
    }
}
