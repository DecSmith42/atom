namespace DecSm.Atom.Secrets;

/// <summary>
///     Defines a contract for retrieving sensitive configuration values (secrets) from various secure storage providers.
/// </summary>
/// <remarks>
///     <para>
///         This interface serves as the core abstraction for the Atom framework's secret management system,
///         enabling secure retrieval of sensitive data such as API keys, connection strings, certificates,
///         and other confidential configuration values from various storage backends.
///     </para>
///     <para>
///         Implementations of this interface integrate with different secret storage systems including:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>Azure Key Vault - For cloud-based secret management</description>
///         </item>
///         <item>
///             <description>.NET User Secrets - For development-time secrets</description>
///         </item>
///         <item>
///             <description>Environment-specific secret stores</description>
///         </item>
///         <item>
///             <description>Custom secure storage solutions</description>
///         </item>
///     </list>
///     <para>
///         The framework automatically discovers and registers implementations through dependency injection,
///         allowing the <see cref="IParamService" /> to query multiple providers when resolving secret parameters
///         marked with <see cref="SecretDefinitionAttribute" />.
///     </para>
///     <para>
///         Secret providers are queried in registration order until a value is found, providing a fallback
///         mechanism for different environments (e.g., Azure Key Vault in production, user secrets in development).
///     </para>
/// </remarks>
/// <example>
///     <para>Basic implementation of a custom secrets provider:</para>
///     <code>
/// public class CustomSecretsProvider : ISecretsProvider
/// {
///     public string? GetSecret(string key)
///     {
///         // Custom logic to retrieve secret from your storage system
///         return RetrieveFromCustomStore(key);
///     }
/// }
/// </code>
///     <para>Registration in a build definition:</para>
///     <code>
/// [BuildDefinition]
/// public partial class Build : BuildDefinition, ISomeTarget
/// {
///     protected override void ConfigureServices(IServiceCollection services)
///     {
///         services.AddSingleton&lt;ISecretsProvider, CustomSecretsProvider&gt;();
///     }
/// }
/// </code>
///     <para>Usage with secret parameters:</para>
///     <code>
/// [TargetDefinition]
/// public partial interface ISomeTarget
/// {
///     [SecretDefinition("database-password", "Database connection password")]
///     string DatabasePassword => GetParam(() => DatabasePassword);
///     Target DeployApp => t => t
///         .Executes(() =>
///         {
///             var password = DatabasePassword; // Retrieved securely from providers
///             // Use password for deployment...
///         });
/// }
/// </code>
/// </example>
/// <seealso cref="SecretDefinitionAttribute" />
/// <seealso cref="IParamService" />
/// <seealso cref="DotnetUserSecretsProvider" />
/// <seealso cref="IDotnetUserSecrets" />
[PublicAPI]
public interface ISecretsProvider
{
    /// <summary>
    ///     Retrieves a secret value by its unique identifier key.
    /// </summary>
    /// <param name="key">
    ///     The unique identifier for the secret to retrieve. This typically corresponds to the
    ///     parameter name or a custom key defined in the secret storage system.
    /// </param>
    /// <returns>
    ///     The secret value associated with the specified key, or <c>null</c> if the secret
    ///     is not found in this provider's storage system.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         Implementations should handle key lookup in a case-sensitive manner unless the underlying
    ///         storage system specifically requires case-insensitive matching. The method should return
    ///         <c>null</c> rather than throwing exceptions when a key is not found, allowing the framework
    ///         to query other providers in the chain.
    ///     </para>
    ///     <para>
    ///         This method may be called frequently during parameter resolution, so implementations should
    ///         consider caching strategies if the underlying storage system has significant latency.
    ///         However, be mindful of security implications when caching sensitive data.
    ///     </para>
    ///     <para>
    ///         The returned value should be the raw secret string as stored in the provider. Any additional
    ///         processing, encoding, or transformation should be handled by the consuming code.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <para>Simple implementation with error handling:</para>
    ///     <code>
    /// public string? GetSecret(string key)
    /// {
    ///     try
    ///     {
    ///         return secretStore.RetrieveSecret(key);
    ///     }
    ///     catch (SecretNotFoundException)
    ///     {
    ///         return null; // Let other providers try
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         logger.LogWarning("Failed to retrieve secret {Key}: {Error}", key, ex.Message);
    ///         return null;
    ///     }
    /// }
    /// </code>
    /// </example>
    string? GetSecret(string key);
}
