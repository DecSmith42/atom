namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Marks a class as a build definition, triggering source generation to implement <see cref="IBuildDefinition" />.
/// </summary>
/// <remarks>
///     <para>
///         This attribute should be applied to a class that inherits from <see cref="MinimalBuildDefinition" /> or
///         <see cref="BuildDefinition" />.
///         The source generator uses this attribute to identify the main build class and generate the necessary code to
///         discover and register targets and parameters.
///     </para>
///     <para>
///         The generator creates a partial class implementation that populates properties like
///         <see cref="IBuildDefinition.TargetDefinitions" /> and <see cref="IBuildDefinition.ParamDefinitions" />.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// [BuildDefinition]
/// public partial class MyBuild : BuildDefinition, IMyTargets
/// {
///     // ...
/// }
///     </code>
/// </example>
/// <seealso cref="MinimalBuildDefinition" />
/// <seealso cref="BuildDefinition" />
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class MinimalBuildDefinitionAttribute : Attribute;
