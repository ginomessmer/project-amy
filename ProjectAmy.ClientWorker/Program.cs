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

namespace ProjectAmy.ClientWorker
{
    public class Program
    {

        public static IEnumerable<string> Scopes = new[] { "ChannelMessage.Read.All" };

        public const string FunctionsEndpoint = "https://project-amy.azurewebsites.net/api/Notification?key=LKo9aT1Jc1FHhYQVXbgJJCSWZq0XuBfkzu9HHaNPOVuO8sjX92VzPw=="; // TODO: Change key
        public const string TeamId = "1047af6d-3a4b-4270-97c5-8694e61582e5";
        public const string ChannelId = "19:ozBKIey0-SsHoj8F9f2-U6cKbdZ3FOHxupLR2kDIxww1@thread.tacv2";

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(_ => PublicClientApplicationBuilder
                        .Create("00f05160-5e0b-4645-ab5f-f35797d95168")
                        .WithDefaultRedirectUri()
                        .Build());

                    services.AddTransient(sp => new GraphServiceClient(new DelegateAuthenticationProvider(
                        async request =>
                        {
                            var app = sp.GetRequiredService<IPublicClientApplication>();
                            var account = (await app.GetAccountsAsync()).FirstOrDefault();
                            var result = await app.AcquireTokenSilent(Scopes, account).ExecuteAsync();
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                        })));

                    services.AddHostedService<InitializerWorker>();
                });
    }
}
