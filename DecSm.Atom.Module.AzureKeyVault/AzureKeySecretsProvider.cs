namespace DecSm.Atom.Module.AzureKeyVault;

[PublicAPI]
public sealed class AzureKeySecretsProvider(IBuildDefinition buildDefinition, CommandLineArgs args, ILogger<AzureKeySecretsProvider> logger)
    : ISecretsProvider, IWorkflowOptionProvider
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

        if (buildDefinition is not IAzureKeyVault definition)
            throw new("The build definition must implement IAzureKeyVault to use Azure Key Vault");

        if (definition.AzureVaultAddress is null or "")
        {
            var addressArg = buildDefinition.ParamDefinitions[nameof(IAzureKeyVault.AzureVaultAddress)].ArgName;

            throw new($"Azure Vault address '{addressArg}' must be set to use Azure Key Vault");
        }

        try
        {
            var client = _secretClient ??= new(new(definition.AzureVaultAddress), GetCredential(definition));
            var value = client.GetSecret(key);

            if (value.HasValue)
            {
                logger.LogTrace("Got secret '{Key}' from Azure Vault: ***", key);

                return value.Value.Value;
            }

            logger.LogTrace("Did not find secret '{Key}' in Azure Vault", key);

            return null;
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
                    buildDefinition.DefaultWorkflowOptions.Concat(buildDefinition.Workflows.SelectMany(x => x.Options))))
                return [];

            if (buildDefinition is not IAzureKeyVault keyVaultDefinition)
                throw new("The build definition must implement IAzureKeyVault to use Azure Key Vault");

            var injections = keyVaultDefinition.AzureKeyVaultValueInjections;

            var valueInjections = new List<IWorkflowOption>();

            switch (injections.Address)
            {
                case AzureKeyVaultValueInjectionType.EnvironmentVariable:
                    valueInjections.Add(WorkflowSecretsEnvironmentInjection.Create(nameof(IAzureKeyVault.AzureVaultAddress)));

                    break;
                case AzureKeyVaultValueInjectionType.Secret:
                    valueInjections.Add(WorkflowSecretsSecretInjection.Create(nameof(IAzureKeyVault.AzureVaultAddress)));

                    break;
            }

            switch (injections.TenantId)
            {
                case AzureKeyVaultValueInjectionType.EnvironmentVariable:
                    valueInjections.Add(WorkflowSecretsEnvironmentInjection.Create(nameof(IAzureKeyVault.AzureVaultTenantId)));

                    break;
                case AzureKeyVaultValueInjectionType.Secret:
                    valueInjections.Add(WorkflowSecretsSecretInjection.Create(nameof(IAzureKeyVault.AzureVaultTenantId)));

                    break;
            }

            switch (injections.AppId)
            {
                case AzureKeyVaultValueInjectionType.EnvironmentVariable:
                    valueInjections.Add(WorkflowSecretsEnvironmentInjection.Create(nameof(IAzureKeyVault.AzureVaultAppId)));

                    break;
                case AzureKeyVaultValueInjectionType.Secret:
                    valueInjections.Add(WorkflowSecretsSecretInjection.Create(nameof(IAzureKeyVault.AzureVaultAppId)));

                    break;
            }

            switch (injections.AppSecret)
            {
                case AzureKeyVaultValueInjectionType.EnvironmentVariable:
                    valueInjections.Add(WorkflowSecretsEnvironmentInjection.Create(nameof(IAzureKeyVault.AzureVaultAppSecret)));

                    break;
                case AzureKeyVaultValueInjectionType.Secret:
                    valueInjections.Add(WorkflowSecretsSecretInjection.Create(nameof(IAzureKeyVault.AzureVaultAppSecret)));

                    break;
            }

            return valueInjections;
        }
    }

    private TokenCredential GetCredential(IAzureKeyVault definition)
    {
        if (definition.AzureVaultTenantId is null or "" ||
            definition.AzureVaultAppId is null or "" ||
            definition.AzureVaultAppSecret is null or "")
        {
            if (args.HasHeadless)
                throw new(
                    "When running in headless mode, Azure Vault parameters must be set: azure-vault-address, azure-vault-tenant-id, azure-vault-app-id");

            return GetUserCredential(definition.AzureVaultTenantId);
        }

        return new ClientSecretCredential(definition.AzureVaultTenantId, definition.AzureVaultAppId, definition.AzureVaultAppSecret);
    }

    private InteractiveBrowserCredential GetUserCredential(string? tenantId)
    {
        logger.LogInformation(
            "Getting Azure Vault credentials interactively, please log in to Azure with a user that has access to the Vault");

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));

        var cred = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
        {
            AdditionallyAllowedTenants =
            {
                tenantId ?? "*",
            },
        });

        try
        {
            cred.Authenticate(cts.Token);
        }
        catch (TaskCanceledException)
        {
            logger.LogWarning("Login timed out. Secrets will not be available from Azure Key Vault");
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

        return cred;
    }
}
