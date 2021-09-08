using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace ProjectAmy.ClientWorker
{
    public class InitializerWorker : BackgroundService
    {
        private readonly ILogger<InitializerWorker> _logger;
        private readonly IPublicClientApplication _app;
        private readonly GraphServiceClient _graphServiceClient;

        public InitializerWorker(ILogger<InitializerWorker> logger,
            IPublicClientApplication app, GraphServiceClient graphServiceClient)
        {
            _logger = logger;
            _app = app;
            _graphServiceClient = graphServiceClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Sign in
            var accounts = await _app.GetAccountsAsync();
            if (!accounts.Any())
            {
                // Sign in
                var result = await _app.AcquireTokenInteractive(Program.Scopes)
                    .ExecuteAsync(stoppingToken);

                _logger.LogInformation("Sign-in successful!");
            }

            // Check subscriptions
            try
            {
                var subscriptions = await _graphServiceClient.Subscriptions.Request()
                    //.Filter($"startsWith(notificationUrl,'{Program.FunctionsEndpoint}')")
                    .GetAsync(stoppingToken);

                _logger.LogInformation("Found {subscriptions}", subscriptions.Count);

               // if (!subscriptions.Any())
                //{
                    await CreateSubscriptionAsync(stoppingToken);
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve subscriptions");
            }
        }

        private async Task CreateSubscriptionAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Register subscription if required
                var subscription = new Subscription
                {
                    ChangeType = "updated",
                    NotificationUrl = Program.FunctionsEndpoint,
                    Resource = $"/teams/{Program.TeamId}/channels/{Program.ChannelId}/messages",
                    ExpirationDateTime = DateTime.UtcNow + TimeSpan.FromHours(1),
                    //ClientState = "secretClientValue",
                    LatestSupportedTlsVersion = "v1_2",
                    //IncludeResourceData = true,
                    // TODO certificate needed for IncludeResourceData
                };

                await _graphServiceClient.Subscriptions
                    .Request()
                    .AddAsync(subscription, stoppingToken);

                _logger.LogInformation("Successfully created subscription");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create a subscription");
            }
        }
    }
}
