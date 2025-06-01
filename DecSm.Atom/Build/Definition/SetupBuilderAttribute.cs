namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Marks an interface as a target setup.
/// </summary>
/// <remarks>
///     Ensure the annotated interface is partial.
/// </remarks>
/// <example>
///     <code>
/// [TargetSetup]
/// public partial interface IMyTargetSetup
/// {
///     Target MyTarget => { ... }
/// 
///     // A stub for this will be created and must be implemented.
///     // It will automatically be called by the build system
///     // if the BuildDefinition implements this interface.
///     public void SetupTarget(IHostApplicationBuilder builder)
///     {
///         // Setup code here
///     }
/// }
/// </code>
/// </example>
[PublicAPI]
[AttributeUsage(AttributeTargets.Interface)]
public sealed class ConfigureBuilderAttribute : Attribute;

[PublicAPI]
[AttributeUsage(AttributeTargets.Interface)]
public sealed class ConfigureHostAttribute : Attribute;
