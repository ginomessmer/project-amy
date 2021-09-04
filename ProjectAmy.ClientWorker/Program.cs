using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace ProjectAmy.ClientWorker
{
    public class Program
    {
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
                    services.AddHostedService<InitializerWorker>();
                });
    }
}
