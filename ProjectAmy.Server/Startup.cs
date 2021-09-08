using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ProjectAmy.Server;
using System;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;

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
           // String keyVaultUrl = builder.GetContext().Configuration["AZURE_KEY_VAULT_URL"];
            // Create a new key client using the default credential from Azure.Identity using environment variables previously set,
            // including AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, and AZURE_TENANT_ID.
           // var client = new KeyClient(vaultUri: new Uri(keyVaultUrl), credential: new DefaultAzureCredential());

            String keyVaultDecryptionKeyUrl = builder.GetContext().Configuration["AZURE_KEY_VAULT_DECRYPTION_KEY_URL"];

            // Create a new cryptography client using the default credential from Azure.Identity using environment variables previously set,
            // including AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, and AZURE_TENANT_ID.
            var cryptoClient = new CryptographyClient(keyId: new Uri(keyVaultDecryptionKeyUrl), credential: new DefaultAzureCredential());
           

            builder.Services.AddSingleton(cryptoClient);
        }
    }
}
