namespace DecSm.Atom.Secrets;

/// <summary>
///     Enables sourcing of secrets from the .NET user secrets store in build definitions.
///     <br /><br />
///     Automatically added to <see cref="BuildDefinition" />.
/// </summary>
/// <remarks>
///     <para>
///         Implement this interface in the Build definition to enable sourcing of secrets from the .NET user secrets
///         store.
///     </para>
///     <para>
///         The interface automatically configures the dependency injection container with a
///         <see cref="DotnetUserSecretsProvider" />
///         that can retrieve secrets from the standard user secrets location using the Microsoft.Extensions.Configuration
///         framework.
///     </para>
///     <para>
///         User secrets are stored per-user and per-project, making them ideal for development-time secrets that should
///         not
///         be committed to source control. The secrets are identified by a UserSecretsId that should be configured in the
///         project file or through the UserSecretsIdAttribute.
///     </para>
/// </remarks>
/// <example>
///     <para>Basic implementation in a build definition:</para>
///     <code>
///     [BuildDefinition]
///     partial class Build : BuildDefinition, IDotnetUserSecrets, ISomeTarget;
///     </code>
///     <para>The configured secrets provider can then be used to retrieve secrets:</para>
///     <code>
///     [TargetDefinition]
///     partial interface ISomeTarget
///     {
///         [SecretDefinition("my-secret", "My secret description")]
///         string MySecret => GetParam(() => MySecret);
///         Target SomeTarget => t => t
///             .Executes(() =>
///             {
///                 var secretValue = MySecret; // retrieves the secret value from user secrets
///                 return Task.CompletedTask;
///              });
///     }
///     </code>
/// </example>
/// <seealso cref="DotnetUserSecretsProvider" />
/// <seealso cref="ISecretsProvider" />
[ConfigureHostBuilder]
public partial interface IDotnetUserSecrets
{
    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder
            .Services
            .AddSingleton<DotnetUserSecretsProvider>()
            .AddSingleton<ISecretsProvider>(x => x.GetRequiredService<DotnetUserSecretsProvider>());
}
