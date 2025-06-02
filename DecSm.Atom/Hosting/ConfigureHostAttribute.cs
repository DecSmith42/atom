namespace DecSm.Atom.Hosting;

/// <summary>
///     Marks an interface as a host configurator.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Interface)]
public sealed class ConfigureHostAttribute : Attribute;
