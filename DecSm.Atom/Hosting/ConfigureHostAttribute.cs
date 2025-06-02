namespace DecSm.Atom.Hosting;

/// <summary>
///     Marks an interface as a host configurator.
/// </summary>
/// <example>
///     <code lang="csharp">
///     [BuildDefinition]
///     partial class Build : BuildDefinition, IMyHostConfigurator;
///     </code>
///     <code lang="csharp">
///     [ConfigureHost]
///     public interface IMyHostConfigurator
///     {
///         void Configure(IHost host)
///         {
///             // Add your host configuration logic here.
///         }
///     }
///     </code>
/// </example>
[PublicAPI]
[AttributeUsage(AttributeTargets.Interface)]
public sealed class ConfigureHostAttribute : Attribute;
