namespace DecSm.Atom.Build.Definition;

/// <summary>
/// A comprehensive abstract base class for creating build definitions in the Atom framework.
/// It inherits from <see cref="BuildDefinition"/> and pre-configures a rich set of common build targets and global options.
/// </summary>
/// <remarks>
/// <para>
/// <c>DefaultBuildDefinition</c> is the recommended starting point for most Atom build projects.
/// It implements several standard interfaces, providing targets for common operations such as:
/// <list type="bullet">
///   <item><see cref="ISetupBuildInfo"/>: Sets up essential build information (ID, name, version).</item>
///   <item><see cref="IValidateBuild"/>: A target for validating the build setup or outputs.</item>
///   <item><see cref="IDotnetUserSecrets"/>: Manages .NET user secrets for secure configuration.</item>
/// </list>
/// </para>
/// <para>
/// The main build definition class in an Atom project (typically <c>_atom/Build.cs</c>) would inherit from <c>DefaultBuildDefinition</c>
/// and be decorated with <see cref="BuildDefinitionAttribute"/>.
/// </para>
/// </remarks>
/// <example>
/// A typical build definition class in <c>_atom/Build.cs</c>:
/// <code>
/// using DecSm.Atom.Build.Definition;
/// using DecSm.Atom.Workflows.Definition;
/// using DecSm.Atom.Workflows.Definition.Triggers;
///
/// [BuildDefinition]
/// [GenerateEntryPoint] // Generates Program.cs
/// internal partial class Build : DefaultBuildDefinition
/// {
///     public override IReadOnlyList&lt;WorkflowDefinition&gt; Workflows =>
///     [
///         new("BuildAndTest")
///         {
///             Triggers = [GitPushTrigger.ToBranch("main"), GitPullRequestTrigger.ToBranch("main")],
///             StepDefinitions =
///             [
///                 Targets.SetupBuildInfo,
///                 Targets.DotnetRestore,
///                 Targets.DotnetBuild,
///                 Targets.DotnetTest,
///                 Targets.DotnetPack
///             ],
///             WorkflowTypes = [ Github.WorkflowType ]
///         }
///     ];
/// }
/// </code>
/// </example>
/// <seealso cref="BuildDefinition"/>
/// <seealso cref="IBuildDefinition"/>
/// <seealso cref="BuildDefinitionAttribute"/>
/// <seealso cref="ISetupBuildInfo"/>
/// <seealso cref="IStoreArtifact"/>
/// <seealso cref="IRetrieveArtifact"/>
[PublicAPI]
public abstract class DefaultBuildDefinition(IServiceProvider services)
    : BuildDefinition(services), ISetupBuildInfo, IValidateBuild, IDotnetUserSecrets;
