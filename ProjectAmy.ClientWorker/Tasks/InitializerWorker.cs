using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using ProjectAmy.ClientWorker.Options;

namespace ProjectAmy.ClientWorker.Tasks
{
    public class InitializerWorker : BackgroundService
    {
        private readonly ILogger<InitializerWorker> _logger;
        private readonly IPublicClientApplication _app;
        private readonly GraphServiceClient _graphServiceClient;
        private readonly AmyClientOptions _options;

        public InitializerWorker(ILogger<InitializerWorker> logger,
            IOptions<AmyClientOptions> options,
            IPublicClientApplication app, GraphServiceClient graphServiceClient)
        {
            _logger = logger;
            _options = options.Value;
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
            
            // Check and create subscriptions
            await CheckSubscriptionsAsync(stoppingToken);
            await CreateSubscriptionAsync(stoppingToken);
        }

        private async Task CheckSubscriptionsAsync(CancellationToken stoppingToken)
        {
            try
            {
                var subscriptions = await _graphServiceClient.Subscriptions.Request()
                    .GetAsync(stoppingToken);

                _logger.LogInformation("Found {subscriptions}", subscriptions.Count);
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
                var base64PublicKey = ReadCertificate();
                var loggedInUser = await _graphServiceClient.Me.Request().GetAsync(stoppingToken);
                
                // Register subscription if required
                var subscription = new Subscription
                {
                    ChangeType = "updated",
                    NotificationUrl = _options.FunctionsNotificationsEndpoint,
                    Resource = $"users/{loggedInUser.Id}/chats/getAllMessages",
                    ExpirationDateTime = DateTime.UtcNow + TimeSpan.FromHours(1),
                    LatestSupportedTlsVersion = "v1_2",
                    IncludeResourceData = true,
                    EncryptionCertificate = base64PublicKey,
                    EncryptionCertificateId = "graph-change-notification-cert",
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

        private string ReadCertificate()
        {
            using var fs = new FileStream(_options.PublicKeyCertificatePath, FileMode.Open);
            using var binaryReader = new BinaryReader(fs);
            var bytes = binaryReader.ReadBytes((int)fs.Length);

            var base64PublicKey = Convert.ToBase64String(bytes, 0, bytes.Length);

            return base64PublicKey;
        }
    }
}
