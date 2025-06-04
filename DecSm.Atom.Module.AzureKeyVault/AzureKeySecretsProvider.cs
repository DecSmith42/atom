namespace DecSm.Atom.Module.AzureKeyVault;

[PublicAPI]
public sealed class AzureKeySecretsProvider(
    IBuildDefinition buildDefinition,
    CommandLineArgs args,
    IAzureKeyVault keyVault,
    ILogger<AzureKeySecretsProvider> logger
) : ISecretsProvider, IWorkflowOptionProvider
{
    private SecretClient? _secretClient;

    public string? GetSecret(string key)
    {
        // We don't want to lookup any secrets that we need to use, or we'll end up in a loop
        if (key is nameof(IAzureKeyVault.AzureVaultAddress)
            or nameof(IAzureKeyVault.AzureVaultTenantId)
            or nameof(IAzureKeyVault.AzureVaultAppId))
            return null;

        logger.LogDebug("Getting secret {Key} from Azure Vault", key);

        if (keyVault.AzureVaultAddress is null or "")
        {
            var addressArg = buildDefinition.ParamDefinitions[nameof(IAzureKeyVault.AzureVaultAddress)].ArgName;

            throw new($"Azure Vault address '{addressArg}' must be set to use Azure Key Vault");
        }

        try
        {
            var client = _secretClient ??= new(new(keyVault.AzureVaultAddress), GetCredential(keyVault));

            return GetSecret(client, key);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get '{Key}' from Azure Vault", key);

            return null;
        }
    }

    public IReadOnlyList<IWorkflowOption> WorkflowOptions
    {
        get
        {
            if (!UseAzureKeyVault.IsEnabled(
                    buildDefinition.GlobalWorkflowOptions.Concat(buildDefinition.Workflows.SelectMany(x => x.Options))))
                return [];

            var injections = keyVault.AzureKeyVaultValueInjections;

            var valueInjections = new List<IWorkflowOption>();

            switch (injections.Address)
            {
                case AzureKeyVaultValueInjectionType.EnvironmentVariable:
                    valueInjections.Add(WorkflowSecretsEnvironmentInjection.Create(nameof(IAzureKeyVault.AzureVaultAddress)));

                    break;
                case AzureKeyVaultValueInjectionType.Secret:
                    valueInjections.Add(WorkflowSecretsSecretInjection.Create(nameof(IAzureKeyVault.AzureVaultAddress)));

                    break;
                case AzureKeyVaultValueInjectionType.None:

                    break;
                default:

                    throw new ArgumentOutOfRangeException();
            }

            switch (injections.TenantId)
            {
                case AzureKeyVaultValueInjectionType.EnvironmentVariable:
                    valueInjections.Add(WorkflowSecretsEnvironmentInjection.Create(nameof(IAzureKeyVault.AzureVaultTenantId)));

                    break;
                case AzureKeyVaultValueInjectionType.Secret:
                    valueInjections.Add(WorkflowSecretsSecretInjection.Create(nameof(IAzureKeyVault.AzureVaultTenantId)));

                    break;
                case AzureKeyVaultValueInjectionType.None:

                    break;
                default:

                    throw new ArgumentOutOfRangeException();
            }

            switch (injections.AppId)
            {
                case AzureKeyVaultValueInjectionType.EnvironmentVariable:
                    valueInjections.Add(WorkflowSecretsEnvironmentInjection.Create(nameof(IAzureKeyVault.AzureVaultAppId)));

                    break;
                case AzureKeyVaultValueInjectionType.Secret:
                    valueInjections.Add(WorkflowSecretsSecretInjection.Create(nameof(IAzureKeyVault.AzureVaultAppId)));

                    break;
                case AzureKeyVaultValueInjectionType.None:

                    break;
                default:

                    throw new ArgumentOutOfRangeException();
            }

            switch (injections.AppSecret)
            {
                case AzureKeyVaultValueInjectionType.EnvironmentVariable:
                    valueInjections.Add(WorkflowSecretsEnvironmentInjection.Create(nameof(IAzureKeyVault.AzureVaultAppSecret)));

                    break;
                case AzureKeyVaultValueInjectionType.Secret:
                    valueInjections.Add(WorkflowSecretsSecretInjection.Create(nameof(IAzureKeyVault.AzureVaultAppSecret)));

                    break;
                case AzureKeyVaultValueInjectionType.None:

                    break;
                default:

                    throw new ArgumentOutOfRangeException();
            }

            return valueInjections;
        }
    }

    private TokenCredential GetCredential(IAzureKeyVault definition)
    {
        if (definition is { AzureVaultTenantId.Length: > 0, AzureVaultAppId.Length: > 0, AzureVaultAppSecret.Length: > 0 })
            return new ClientSecretCredential(definition.AzureVaultTenantId, definition.AzureVaultAppId, definition.AzureVaultAppSecret);

        if (args.HasHeadless)
            throw new(
                "When running in headless mode, Azure Vault parameters must be set: azure-vault-address, azure-vault-tenant-id, azure-vault-app-id");

        return new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
        {
            AdditionallyAllowedTenants =
            {
                definition.AzureVaultTenantId ?? "*",
            },
        });
    }

    private string? GetSecret(SecretClient client, string key)
    {
        logger.LogInformation(
            "Getting Azure Vault secret. If login is prompted, please log in to Azure with a user that has access to the Vault");

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));

        try
        {
            var response = client.GetSecret(key, cancellationToken: cts.Token);

            if (response.HasValue)
            {
                logger.LogTrace("Got secret '{Key}' from Azure Vault: ***", key);

                return response.Value.Value;
            }

            logger.LogWarning("Secret '{Key}' not found in Azure Vault", key);

            return null;
        }
        catch (TaskCanceledException)
        {
            logger.LogWarning("Vault request timed out");
        }
        catch (AuthenticationFailedException authenticationFailed)
        {
            logger.LogWarning("Failed to login. Secrets will not be available from Azure Key Vault: {Message}",
                authenticationFailed.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to login. Secrets will not be available from Azure Key Vault");
        }

        return null;
    }
}
