namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Marks an interface as a container for target definitions, triggering source generation.
/// </summary>
/// <remarks>
///     When this attribute is applied to a partial interface, the source generator discovers all properties
///     of type <see cref="Target" /> within it and makes them available to the build system.
/// </remarks>
/// <example>
///     <code>
/// [TargetDefinition]
/// public partial interface IMyTargets
/// {
///     Target MyFirstTarget => t => t.DescribedAs("An example target.");
/// }
/// [BuildDefinition]
/// public partial class MyBuild : BuildDefinition, IMyTargets;
///     </code>
/// </example>
[PublicAPI]
[AttributeUsage(AttributeTargets.Interface)]
public sealed class TargetDefinitionAttribute : Attribute;
