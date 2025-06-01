namespace DecSm.Atom.Hosting;

/// <summary>
///     Marks an interface as a HostBuilder configurator.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Interface)]
public sealed class ConfigureHostBuilderAttribute : Attribute;
