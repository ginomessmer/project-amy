using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectAmy.Server;
using System;
using System.Collections.Generic;
using System.Text;
using Azure.Security.KeyVault.Keys;

[assembly: FunctionsStartup(typeof(Startup))]
namespace ProjectAmy.Server
{
    class Startup : FunctionsStartup
    {
      

        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureKeyVault(builder);
        }


        public void ConfigureKeyVault(IFunctionsHostBuilder builder)
        {
            String keyVaultUrl = builder.GetContext().Configuration["AZURE_KEY_VAULT_URL"];
            // Create a new key client using the default credential from Azure.Identity using environment variables previously set,
            // including AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, and AZURE_TENANT_ID.
            var client = new KeyClient(vaultUri: new Uri(keyVaultUrl), credential: new DefaultAzureCredential());

            builder.Services.AddSingleton(client);
        }
    }
}
