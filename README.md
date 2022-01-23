# Project Amy
Bringing the friendly reactions we get from our co-workers into the real world. Through this repository you can learn how to use the Microsoft Graph API to notify an Azure Function everytime someone reacts to one of your messages in Microsoft Teams. A RGB keyboard on the workdesk is the edge device that runs a program to poll the Azure Storage Queue of the Azure Function for notifications. When a co-worker reacts to one of your messages then the RGB keyboard lights up. 

Presentation at Cloud Summit 2021: [YouTube Recording](https://youtu.be/INll8mavIas)

## Getting Started
This will guide you on how to set up the project for local development.
The project consists of a local command line application written in C# and an Azure Functions app developed in TypeScript.

### Prerequisites
For development we recommend using [Visual Studio 2022](https://visualstudio.microsoft.com/vs/).

Other tools needed for local development that should come with Visual Studio 2022:
* [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
* [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio) for Azure Storage emulation
* [Azure Functions local dev environment](https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-local)
* A Microsoft Teams tenant with sufficient privileges to create an app registration with delegated permissions for scopes ``Chat.Read`` and ``User.Read``. We recommend creating a free [Microsoft 365 Developer Tenant](https://developer.microsoft.com/en-us/microsoft-365/dev-program)
* C# dev environment

### Azure Functions App
The Azure Functions app provides an endpoint that the Microsoft Graph can call whenever a message in Microsoft Teams changes. The Function then extracts the available reaction from the message update and sends it to an Azure Storage Queue. The Azure Storage Queue is then consumed by the C# command line application.

### C# Command Line Application
The local command line application connects to your Corsair keyboard and polls an Azure Storage Queue to know when a message received a reaction and then lights up the keyboard.

Don't worry, you don't need to have a Corsair keyboard to use this project. Without a Corsair keyboard you can still learn how to be notified when someone reacts to your messages in Microsoft Teams. Instead of lighting up your keyboard it will print the reaction to your console. So you can get creative and extend the application to do whatever you want when someone reacts to your messages.

The command line application is also resposible for authentication and setting up the change notification subscription with Microsoft Graph to send changed Microsoft Teams messages to the Azure Function.

## Deployment
For this solution to run you need to deploy an Azure Function, Azure Queue Storage, and setup a Azure Active Directory application.

### Create Azure Function
1. On the [Azure Portal](https://portal.azure.com/) click on *'+ Create a resource'*.
2. In the Azure marketplace search for *'Function App'*, select it, and click *'Create'*.
3. Fill out all the information to **Create Function App**.
    * **Create new** resource group that you will use for the project.
    * Enter a **Function App name**.
    * Leave **Publish** as *'Code'*.
    * For the **Runtime stack** select *'Node.js'*.
    * For **Region** leave it as *'Central US'*.
    * Click *'Review + create'*.
4. Once the Function App is created, click *'Go to resource'*.
5. Keep the page for the Azure Function open in your browser.

### Azure Function Identity
1. On the page for the Azure Function you created in the previous step, click on **Identity** in the left hand side menu.
2. Under **System assigned** identity toggle the **Status** to *'On'*.
3. Click on *'Save'*.
4. Copy the **object/principial ID** for allowing the Azure Function access to the Azure Key Vault in a later step.
5. Keep the page for the Azure Function open in your browser for more configuration.

### Create Azure Key Vault
1. In a new browser tab login to the [Azure Portal](https://portal.azure.com/).
2. Click on *'+ Create a resource'*.
3. In the Azure marketplace search for *'Key Vault'*.
4. On the **Key Vault** marketplace page click *'Create'*.
5. Fill out all the information to **Create a Key Vault**.
    * Select the **resource group** that you are using for the project.
    * Enter a **Key vault name**.
    * Select *'Central US'* for the **Region**, so that the key vault is close to the Azure Function.
    * The *Standard* **pricing tier** is sufficient.
    * Click *'Review + create'*.
6. Once Azure is done creating the Key Vault, click *'Go to resource'*.
7. Keep the page for the Azure Key Vault open in your browser for the next step.

### Enable Azure Key Vault Access
1. On the browser page for your Azure Key Vault, click on **Access policies** in the left hand side menu.
2. Above the list of **Current Access Policies** click on *'+ Add Access Policy'*.
3. Configure the access policy.
    * In the **Key permissions** dropdown select *'Get'* and *'Decrypt'*.
    * In the **Certificate permissions** dropdown select *'Get'*.
    * For **Select principal** click on *'None selected'*. In the search box paste in the Azure Function's **object/principial ID** you copied in the step when creating the [Azure Function Identity](#Azure-Function-Identity). Click on the name of your Azure Function in the search results and click *'Select'* on the bottom.
    * Click on *'Add'* to add the Access Policy.
4. On the **Access Policy** click on *'Save'*.
5. Keep the Azure Key Vault page open in your browser.


### Create Azure Key Vault Certificate
1. On the **Key Vault** page click *'Certificates'*.
2. Create a new certificate for decryption by clicking on *'+ Generate/Import'*.
    * Enter a **Certificate name**.
    * Keep **Self-signed certificate** selected.
    * As **Subject** you have to enter ``CN=<your-project-name>.azurewebsites.net`` where you replace ``<your-project-name>`` with the name of your project.
    * Under **Content Type** select *'PEM'*.
    * Click on *'Not configured'* under **Advanced Policy Configuration**.
        * In the **X.509 Key Usage Flags** dropdown check *'Data Encipherment'*.
        * Click on *'OK'*.
    * Click *'Create'*.
3. Back in the list of **Certificates** click *'Refresh'* until the new cerificate you just created shows up as **Completed**.
4. Keep the page of the certificates open for the next step of downloading the public key.

### Download Public Key
1. In your browser have the Azure Key Vault **Certificates** page open.
2. On your computer open a command line instance.
3. Login to Azure using the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) by typing ``az login``.
4. After logging in you see a list of your subscriptions. Copy the **id** property value of the subscription that your Key Vault is in.
5. Download the public key of the certificate by executing ``az keyvault certificate download --file "<path-to-your-github-repositories>/project-amy/ProjectAmy.ClientWorker/cert.pem" --encoding PEM --name "<name-of-certificate>" --subscription "<subscription-id>" --vault-name "<key-vault-name>" --version "<version-of-certificate>"``.
    * Replace ``<path-to-your-github-repositories>`` with the path to the folder where you store this Git repository.
    * Replace ``<name-of-certificate>`` with the name of the certificate that you created and is shown in the list of certificates on the Azure Key Vault **Certificates** page in your browser.
    * Paste the **id** of your subscription you copied in step 4. instead of ``<subscription-id>``.
    * Replace ``<key-vault-name>`` with the name of your Key Vault. You can find the name of the Key Vault in the top left corner of the Azure Key Vault **Certificates** page in your browser.
    * Replace ``<version-of-certificate>`` with the version of the certificate that you created, which you can find by clicking on the name of the certificate in the list of **Certificates** in your browser.
6. In your browser with the Azure Key Vault page open, click on the current version of the certificate and copy the value of **Certificate Identifier** for the next step.


### Configure Azure Function
1. On the **Function App** page in the Azure Portal, click *'Configuration'* in the left hand side menu.
2. Add a new **Application setting** by clicking *'+ New application setting'*.
    * Enter ``AZURE_KEY_VAULT_DECRYPTION_KEY_URL`` as the **Name**.
    * Paste the **Certificate Identifier** you copied in the [previous step](#download-public-key) as the **Value**.
    * Click *'OK'*.
    * After chaging and adding **Application Settings** always click *'Save'*.

### Deploy Azure Function Code
There are multiple ways to deploy the code to the Azure Function. However, the easiest way for a project without having to set up additional resources is to use direct zip deployment. However, this is not recommended for a production environment. For that we advise you to take a look into the ``.github/workflows`` folder of this repository. It contains samples for using continous integration with GitHub Actions.
1. Open a command line on your computer.
2. Navigate to the ``project-amy\projectamy-server`` folder where the Azure Functions code is located.
3. With Visual Studio 2022 installed you should already have [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v4%2Cwindows%2Ccsharp%2Cportal%2Cbash#install-the-azure-functions-core-tools) and [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) installed.
4. In your command line execute ``func azure functionapp publish <FunctionAppName>``, replacing <FunctionAppName> with the name of your Azure Function App [that you specified previously](#Create-Azure-Function).
    * If you get an error **Unable to connect to Azure** then you do not have the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) installed or need to sign in to your Azure account by executing ``az login`` first.
5. Once the Azure Function is deployed the endpoint URLs will show up in your command line. Copy the URL for the Notification endpoint for configuring the C# application later.

### Configure Azure Queue Storage
We are going to use the storage account that gets created with the Azure Function App for the Queue that our solution needs. Nevertheless, you need to configure the Queue.
1. In the [Azure Portal](https://portal.azure.com/) navigate to the resource group you are using for the project.
2. Within the resource group in the list of resources there should be a resource of type **Storage Account**. Click on it.
3. On the **Storage Account** page click *'Queues'* in the left hand side menu.
4. Create a new queue by clicking *'+ Queue'*.
    * Enter *'events'* as the **Queue name**.
    * Click *'Ok'*.
5. Keep the page open for the next step.

### Get Azure Queue Storage Connection String
1. On the **Storage Account** page in your browser click on **Access keys** in the left hand side menu.
2. On top click on *'Show keys'*.
3. From one of the two keys copy the **Connection String** for later.

### Create Azure Active Directory Application
1. Login to the [Azure Portal](https://portal.azure.com/).
2. In the search bar type *'App registrations'*.
3. On the **App registrations** page, click *'+ New registration'*.
    * Give your application a **Name**.
    * Under **Supported account types** select *Multitenant* without personal account. Personal accounts will not work, because ``Chat.Read`` permission is not available for personal accounts.
    * Under **Redirect URI** select *'Public client/native (mobile & desktop)'* in the dropdown and paste ``http://localhost`` in the textbox next to the dropdown. This will allow the local C# command line application to sign you in.
5. After clicking *Register*, Azure will open the page for the new App registration. On the **Overview** tab you can copy the **Application (client) ID** for later.
6. In the left navigation bar click **API permissions**.
7. The **API permissions** tab will already have the ``User.Read`` permission. Add the ``Chat.Read`` permission by clicking on *'+ Add a permission'*.
    * In the flyout that opens click on *'Microsoft Graph'*
    * Next, click on *'Delegated permissions'*
    * Search for ``Chat.Read`` and select it.
    * Click on *'Add permissions'*


### Configure the C# application
1. Open the **ProjectAmy** solution from this repository in Visual Studio.
2. In the solution explorer right click on **ProjectAmy.ClientWorker**.
3. In the right click menu click on **Manage User Secrets**.
4. Paste the following JSON:
```
{
  "Options": {
    "FunctionsNotificationsEndpoint": "<your-azure-function-url>",
    },
  "ConnectionStrings": {
    "ApplicationId": "<your-application-id>",
    "DefaultQueueConnection": "<your-azure-storage-queue-connection>"
  }
}
```
5. Replace the placeholders with your values.
    * For ``<your-azure-function-url>`` paste the URL for your Azure Function Notification endpoint that you copied from your command line [when deploying the Azure Function code](#Deploy-Azure-Function-Code).
    * For ``<your-application-id>`` paste the **Application (client) ID** you copied from the [previous step](#Create-Azure-Active-Directory-Application).
    * For ``<your-azure-storage-queue-connection>`` paste the **Connection string** from the [Get Azure Queue Storage Connection String](#Get-Azure-Queue-Storage-Connection-String) step.
6. Save the ``secrets.json`` file.

## Using the Application
After you [deployed](#Deployment) the application, you can use it.
1. Open the **ProjectAmy** solution from this repository in Visual Studio.
2. Run the **ProjectAmy.ClientWorker** project by clicking the green run button.
3. The application will require you to sign in in your browser with your Microsoft account that you use for Microsoft Teams.
4. After you sign in, the application will subscribe to your Microsoft Teams 1-on-1 and group chat messages for the next hour.
5. If someone reacts to your message in one of your Microsoft Teams 1-on-1 and group chats, the application will show the reaction in your local command line or light up your keyboard if you have a Corsair keyboard connected. 