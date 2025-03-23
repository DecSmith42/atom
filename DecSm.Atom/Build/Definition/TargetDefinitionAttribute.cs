namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Marks an interface as a target definition.
/// </summary>
/// <remarks>
///     Ensure the annotated interface is partial.
/// </remarks>
/// <example>
///     <code>
/// [TargetDefinition]
/// public partial interface IMyTarget
/// {
///     Target MyTarget => { ... }
/// }
/// </code>
/// </example>
[PublicAPI]
[AttributeUsage(AttributeTargets.Interface)]
public sealed class TargetDefinitionAttribute : Attribute;
