namespace DecSm.Atom.Hosting;

/// <summary>
///     Marks an interface as a HostBuilder configurator.
/// </summary>
/// <example>
///     <code lang="csharp">
///     [BuildDefinition]
///     partial class Build : BuildDefinition, IMyHostBuilderConfigurator;
///     </code>
///     <code lang="csharp">
///     [ConfigureHostBuilder]
///     public interface IMyHostBuilderConfigurator
///     {
///         void Configure(IHostBuilder hostBuilder)
///         {
///             // Add your host builder configuration logic here.
///         }
///     }
///     </code>
/// </example>
[PublicAPI]
[AttributeUsage(AttributeTargets.Interface)]
public sealed class ConfigureHostBuilderAttribute : Attribute;
