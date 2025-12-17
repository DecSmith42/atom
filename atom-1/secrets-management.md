# Secrets Management

Atom provides a flexible and extensible system for managing sensitive configuration values, often referred to as \* _secrets_\*. This system ensures that secrets are handled securely, masked in logs, and can be sourced from various secure storage providers.

### `ISecretsProvider` Interface

The `ISecretsProvider` interface is the core abstraction for Atom's secret management. It defines a contract for retrieving secret values by a given key. Atom can integrate with multiple `ISecretsProvider` implementations, querying them in a chain of responsibility until a secret is found.

**When to use it:**

* To integrate a new secure storage system (e.g., a custom vault, cloud secret manager) with Atom.
* When you need to retrieve sensitive data that should not be hardcoded or stored in plain text.

**How to implement it:** Implement the `GetSecret(string key)` method, which should return the secret value as a string or `null` if the secret is not found in that provider.

```csharp
// Example: A custom secrets provider
public class MyCustomSecretsProvider : ISecretsProvider
{
    private readonly IConfiguration _config;

    public MyCustomSecretsProvider(IConfiguration config)
    {
        _config = config;
    }

    public string? GetSecret(string key)
    {
        // Logic to retrieve secret from a custom secure store or configuration
        // For example, from a specific section in appsettings.json
        return _config[$"MySecrets:{key}"];
    }
}
```

**How to register it:** Register your custom `ISecretsProvider` implementation in your build definition's `ConfigureServices` method.

```csharp
// In your build definition project (e.g., _atom/Build.cs)
[BuildDefinition]
public partial class MyBuild : BuildDefinition, IMyCustomTargets
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddSingleton<ISecretsProvider, MyCustomSecretsProvider>();
    }
}
```

### `IDotnetUserSecrets` Interface and `DotnetUserSecretsProvider`

Atom provides built-in support for .NET User Secrets, which are ideal for development-time secrets that should not be committed to source control.

* **`IDotnetUserSecrets`**: This interface, when implemented by your build definition, automatically enables the `DotnetUserSecretsProvider`. It's included by default when you inherit from `BuildDefinition`.
* **`DotnetUserSecretsProvider`**: This is the concrete implementation of `ISecretsProvider` that reads secrets from the .NET user secrets store. It uses `Microsoft.Extensions.Configuration` to load secrets associated with your project's `UserSecretsId`.

**When to use it:**

* For local development secrets (e.g., API keys for testing, local database connection strings).
* When you want to keep sensitive data out of your source code during development.

**How to use it:** If your build definition inherits from `BuildDefinition`, `IDotnetUserSecrets` is already implemented. You just need to define your secrets using `[SecretDefinition]` and manage them via the `dotnet user-secrets` CLI tool.

```csharp
// In IMyCustomTargets.cs
[TargetDefinition]
public partial interface IMyCustomTargets : IBuildAccessor
{
    [SecretDefinition("my-dev-api-key", "API key for development environment")]
    string DevApiKey => GetParam(() => DevApiKey);

    Target UseDevApiKey => t => t.Executes(() =>
    {
        Logger.LogInformation($"Using API Key: {DevApiKey}"); // Value will be masked in logs
    });
}
```

To set the secret:

```bash
dotnet user-secrets set "my-dev-api-key" "your-secret-value" --project path/to/your/build-definition-project.csproj
```

### `[SecretDefinition]` Attribute

As mentioned in the Parameters documentation, the `[SecretDefinition]` attribute marks a parameter as sensitive. This triggers Atom's secret handling mechanisms:

* **Masking in Logs**: Any occurrence of the secret's value in log output will be replaced with `*****`.
* **Prioritized Resolution**: The `IParamService` will prioritize resolving secrets from `ISecretsProvider` implementations.

**How to use it:** Apply `[SecretDefinition]` to properties in your build definition interfaces, just like `[ParamDefinition]`.

```csharp
// In IMyCustomTargets.cs
[TargetDefinition]
public partial interface IMyCustomTargets : IBuildAccessor
{
    [SecretDefinition("database-password", "Password for the production database")]
    string DbPassword => GetParam(() => DbPassword);
}
```

By leveraging Atom's secrets management, you can build secure and robust automation workflows that handle sensitive information responsibly across different environments.
