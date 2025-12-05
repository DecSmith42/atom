# Azure Key Vault Module

The `DecSm.Atom.Module.AzureKeyVault` module provides seamless integration with Azure Key Vault, allowing your DecSm.Atom build processes to securely retrieve secrets and manage sensitive configuration. This module leverages Azure's robust key management service to enhance the security of your build pipelines by avoiding hardcoded credentials.

## Features

*   **Secure Secret Retrieval**: Access secrets stored in Azure Key Vault directly from your build definition.
*   **Flexible Authentication**: Supports both service principal (client ID and secret) and interactive browser authentication for connecting to Azure Key Vault.
*   **Parameter-driven Configuration**: Configure Azure Key Vault connection details using build parameters, which can be sourced from command-line arguments, environment variables, or `appsettings.json`.
*   **Workflow Integration**: Integrates with DecSm.Atom's workflow system to define how Azure Key Vault parameters are injected into the build environment.

## Getting Started

To use the Azure Key Vault module, you need to implement the `IAzureKeyVault` interface in your build definition. This interface provides the necessary parameters for connecting to your Azure Key Vault instance.

### Prerequisites

Before you begin, ensure you have:

1.  An Azure Key Vault instance.
2.  An Azure Active Directory App Registration with permissions to access secrets in your Key Vault. Note down its Application (Client) ID and a client secret.
3.  The Tenant ID of your Azure subscription.

### Implementation

Add the `IAzureKeyVault` interface to your `Build.cs` file:

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.AzureKeyVault;

namespace Atom;

[BuildDefinition]
internal partial class Build : IAzureKeyVault
{
    // Your build targets and other definitions
}
```

## Configuration Parameters

The `IAzureKeyVault` interface exposes the following parameters, which you can set via command-line arguments, environment variables, or `appsettings.json`:

*   **`azure-vault-address`**: The URI of your Azure Key Vault (e.g., `https://your-vault-name.vault.azure.net/`).
*   **`azure-vault-tenant-id`**: Your Azure Active Directory Tenant ID.
*   **`azure-vault-app-id`**: The Application (Client) ID of your Azure AD App Registration.
*   **`azure-vault-app-secret`**: The client secret for your Azure AD App Registration.

### Example `appsettings.json` Configuration

```json
{
  "Params": {
    "azure-vault-address": "https://your-vault-name.vault.azure.net/",
    "azure-vault-tenant-id": "YOUR_TENANT_ID",
    "azure-vault-app-id": "YOUR_APP_ID"
  },
  "Secrets": {
    "azure-vault-app-secret": "YOUR_APP_SECRET"
  }
}
```

**Note**: It is highly recommended to store `azure-vault-app-secret` in a secure secret store (e.g., `Secrets` section in `appsettings.json` or an environment variable marked as secret in your CI/CD system) rather than directly in `Params` or command-line arguments for production environments.

### Command-Line Example

```bash
dotnet run -- MyTarget --azure-vault-address "https://your-vault-name.vault.azure.net/" --azure-vault-tenant-id "YOUR_TENANT_ID" --azure-vault-app-id "YOUR_APP_ID" --azure-vault-app-secret "YOUR_APP_SECRET"
```

## Retrieving Secrets

Once configured, you can retrieve secrets within your build targets using the `GetSecret` method provided by the `ISecretsProvider` interface. The `AzureKeySecretsProvider` is automatically registered when you implement `IAzureKeyVault`.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.AzureKeyVault;
using DecSm.Atom.Secrets; // Required for ISecretsProvider

namespace Atom;

[BuildDefinition]
internal partial class Build : IAzureKeyVault
{
    // ISecretsProvider is automatically injected if IAzureKeyVault is implemented
    private Target MySecretTarget =>
        t => t
            .DescribedAs("Retrieves and uses a secret from Azure Key Vault")
            .Executes(async () =>
            {
                var mySecretValue = SecretsProvider.GetSecret("MyApplicationSecret");

                if (!string.IsNullOrEmpty(mySecretValue))
                {
                    Logger.LogInformation("Successfully retrieved secret: {SecretValue}", mySecretValue);
                    // Use mySecretValue in your build logic
                }
                else
                {
                    Logger.LogWarning("Secret 'MyApplicationSecret' could not be retrieved from Azure Key Vault.");
                }

                await Task.CompletedTask;
            });
}
```

## Authentication Flow

The module attempts to authenticate with Azure Key Vault using the following priority:

1.  **Client Secret Credential**: If `AzureVaultTenantId`, `AzureVaultAppId`, and `AzureVaultAppSecret` are all provided, it will use a `ClientSecretCredential`. This is ideal for non-interactive environments like CI/CD pipelines.
2.  **Interactive Browser Credential**: If the client secret is not fully provided and the build is not running in headless mode (`--headless` flag), it will attempt to use an `InteractiveBrowserCredential`. This will open a browser window for you to log in to Azure.

If running in headless mode without all client secret credentials, an `InvalidOperationException` will be thrown.

## Workflow Options

The `AzureKeyVaultValueInjections` record allows you to customize how the Azure Key Vault parameters are sourced. By default:

*   `Address`, `TenantId`, `AppId` are expected from `EnvironmentVariable`.
*   `AppSecret` is expected from `Secret` (a secure secret store).

You can override these defaults in your build definition:

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.AzureKeyVault;

namespace Atom;

[BuildDefinition]
internal partial class Build : IAzureKeyVault
{
    public AzureKeyVaultValueInjections AzureKeyVaultValueInjections =>
        new(Address: AzureKeyVaultValueInjectionType.Secret, // Get address from a secret store
            TenantId: AzureKeyVaultValueInjectionType.EnvironmentVariable,
            AppId: AzureKeyVaultValueInjectionType.EnvironmentVariable,
            AppSecret: AzureKeyVaultValueInjectionType.Secret);

    // ... rest of your build definition
}
```

The `UseAzureKeyVault` workflow option can be used to explicitly enable or disable the Azure Key Vault integration within specific workflows or globally.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.AzureKeyVault;
using DecSm.Atom.Workflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IAzureKeyVault
{
    // Globally enable Azure Key Vault
    public IReadOnlyList<IWorkflowOption> GlobalWorkflowOptions =>
        new List<IWorkflowOption> { new UseAzureKeyVault() };

    // ... or enable for a specific workflow
    private Workflow MyWorkflow =>
        w => w
            .Options(new UseAzureKeyVault())
            .Target(MySecretTarget);
}
```
