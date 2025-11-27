namespace DecSm.Atom.Build.Definition;

/// <summary>
///     An attribute used to mark a class as a build definition within the Atom framework.
///     This attribute should be applied to a class that defines the structure and behavior of a build process.
/// </summary>
/// <remarks>
///     <para>
///         When a class is decorated with <c>[BuildDefinition]</c>, the Atom framework's source generators
///         identify it as the primary definition for the build.
///         The <c>BuildDefinitionSourceGenerator</c> specifically looks for this attribute
///         to generate partial class implementations. This generated code typically includes:
///         <list type="bullet">
///             <item>
///                 Implementation of <see cref="IBuildDefinition" /> properties like
///                 <see cref="IBuildDefinition.TargetDefinitions" /> and
///                 <see cref="IBuildDefinition.ParamDefinitions" /> by discovering targets and parameters from implemented
///                 interfaces.
///             </item>
///             <item>
///                 Implementation of <see cref="DecSm.Atom.Hosting.IConfigureHost" /> methods if the build definition
///                 class or its
///                 implemented interfaces use <see cref="DecSm.Atom.Hosting.ConfigureHostBuilderAttribute" /> or
///                 <see cref="DecSm.Atom.Hosting.ConfigureHostAttribute" />.
///             </item>
///             <item>
///                 Static helper classes (e.g., <c>Targets</c> and <c>Params</c>) for easier access to defined targets and
///                 parameters within
///                 the build definition.
///             </item>
///         </list>
///         This attribute is essential for the Atom framework to correctly initialize and execute the build.
///     </para>
///     <para>
///         It is recommended that the class marked with this attribute also inherits from <see cref="BuildDefinition" />
///         or <see cref="DefaultBuildDefinition" /> to gain base functionality.
///     </para>
/// </remarks>
/// <example>
///     The following example shows a typical usage of the <c>BuildDefinitionAttribute</c>:
///     <code>
/// namespace MyProject.Build;
/// using DecSm.Atom.Build.Definition;
/// using DecSm.Atom.Hosting;
/// [BuildDefinition] // Marks this class as the build definition
/// [GenerateEntryPoint] // Often used with GenerateEntryPoint to create a Program.cs
/// internal partial class MyBuild : DefaultBuildDefinition, IMyCustomTarget
/// {
///     // Implement custom workflows, override default options, etc.
///     public override IReadOnlyList&lt;WorkflowDefinition&gt; Workflows =>
///     [
///         new("DefaultBuild")
///         {
///             Triggers = [GitPushTrigger.ToMain],
///             Targets = [Targets.MyCustomTarget], // Targets are available via generated static class
///             WorkflowTypes = [Github.WorkflowType]
///         }
///     ];
/// }
/// [TargetDefinition]
/// public partial interface IMyCustomTarget
/// {
///     Target MyCustomTarget => t => t.DescribedAs("My custom build target.").Executes(() => { /* ... */ });
/// }
/// </code>
///     This example demonstrates how <c>BuildDefinitionAttribute</c> is applied to the <c>MyBuild</c> class.
/// </example>
/// <seealso cref="BuildDefinition" />
/// <seealso cref="DefaultBuildDefinition" />
/// <seealso cref="IBuildDefinition" />
/// <seealso cref="TargetDefinitionAttribute" />
/// <seealso cref="DecSm.Atom.Hosting.GenerateEntryPointAttribute" />
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class BuildDefinitionAttribute : Attribute;
