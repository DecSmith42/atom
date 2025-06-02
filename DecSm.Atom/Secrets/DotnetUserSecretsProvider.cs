﻿namespace DecSm.Atom.Secrets;

/// <summary>
///     Provides access to .NET user secrets for retrieving configuration values during build processes.
/// </summary>
/// <remarks>
///     This provider integrates with the .NET user secrets system to securely access sensitive configuration
///     values without storing them in source code or configuration files. It uses the Microsoft.Extensions.Configuration
///     framework to load user secrets from the standard user secrets location.
/// </remarks>
/// <example>
///     Example usage:
///     <code>
/// var provider = new DotnetUserSecretsProvider();
/// provider.SecretsAssembly = typeof(MyApplication).Assembly;
/// var apiKey = provider.GetSecret("MyApiKey");
/// </code>
/// </example>
internal sealed class DotnetUserSecretsProvider : ISecretsProvider
{
    /// <summary>
    ///     Gets or sets the assembly used to identify the user secrets configuration.
    /// </summary>
    /// <value>
    ///     The assembly containing the user secrets ID attribute. If not set, defaults to the entry assembly.
    /// </value>
    /// <remarks>
    ///     This assembly should have the <c>UserSecretsIdAttribute</c> applied to it, which identifies
    ///     the user secrets store to use. If null, the entry assembly will be used instead.
    /// </remarks>
    public Assembly? SecretsAssembly { get; set; }

    /// <summary>
    ///     Retrieves a secret value by its key from the .NET user secrets store.
    /// </summary>
    /// <param name="key">The key of the secret to retrieve.</param>
    /// <returns>
    ///     The secret value associated with the specified key, or <c>null</c> if the key is not found.
    /// </returns>
    /// <remarks>
    ///     This method builds a configuration using the user secrets provider and searches for the specified key.
    ///     The user secrets are loaded based on the <see cref="SecretsAssembly" /> or the entry assembly if not specified.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var connectionString = provider.GetSecret("DatabaseConnectionString");
    /// var apiKey = provider.GetSecret("ExternalApiKey");
    /// </code>
    /// </example>
    public string? GetSecret(string key)
    {
        var userSecrets = new ConfigurationBuilder()
            .AddUserSecrets(SecretsAssembly ?? Assembly.GetEntryAssembly()!)
            .Build()
            .AsEnumerable()
            .ToDictionary(x => x.Key, x => x.Value);

        return userSecrets.GetValueOrDefault(key);
    }
}
