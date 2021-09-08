using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using ProjectAmy.ClientWorker.Options;

namespace ProjectAmy.ClientWorker
{
    public class Program
    {

        public static IEnumerable<string> Scopes = new[] { "ChannelMessage.Read.All" };

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Config
                    services.Configure<AmyClientOptions>(hostContext.Configuration.GetSection("Options"));

                    // MSAL
                    services.AddSingleton(_ => PublicClientApplicationBuilder
                        .Create("00f05160-5e0b-4645-ab5f-f35797d95168")
                        .WithDefaultRedirectUri()
                        .Build());

                    // Graph Client
                    services.AddTransient(sp => new GraphServiceClient(new DelegateAuthenticationProvider(
                        async request =>
                        {
                            var app = sp.GetRequiredService<IPublicClientApplication>();
                            var account = (await app.GetAccountsAsync()).FirstOrDefault();
                            var result = await app.AcquireTokenSilent(Scopes, account).ExecuteAsync();
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                        })));

                    // Workers
                    services.AddHostedService<InitializerWorker>();
                    services.AddHostedService<KeyboardReactionsWorker>();
                });
    }
}
