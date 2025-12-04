namespace DecSm.Atom.Build.Definition;

/// <summary>
///     An abstract base class for creating custom build definitions in the Atom framework.
///     It provides default implementations for the <see cref="IBuildDefinition" /> interface.
/// </summary>
/// <remarks>
///     <para>
///         Developers should typically inherit from <see cref="BuildDefinition" /> for a more comprehensive
///         set of pre-configured targets and options. However, <c>BuildDefinition</c> can be used as a
///         more minimal base if finer-grained control or a very lean setup is desired.
///     </para>
///     <para>
///         The properties <see cref="TargetDefinitions" /> and <see cref="ParamDefinitions" /> are intended to be
///         populated by source generators based on the interfaces and attributes used in the derived class.
///     </para>
///     <para>
///         A class deriving from <c>BuildDefinition</c> (or <see cref="BuildDefinition" />) must be
///         decorated with the <see cref="MinimalBuildDefinitionAttribute" /> to be recognized by the Atom framework.
///     </para>
/// </remarks>
/// <example>
///     A minimal build definition:
///     <code>
/// using DecSm.Atom.Build.Definition;
/// [BuildDefinition]
/// internal partial class MinimalBuild : BuildDefinition, IMyTargets
/// {
///     // Override Workflows or GlobalOptions if needed
///     public override IReadOnlyList&lt;WorkflowDefinition&gt; Workflows =>
///     [
///         new("MyWorkflow")
///         {
///             Targets = [Targets.MyTarget], // Assumes IMyTargets defines MyTarget
///             WorkflowTypes = [ Github.WorkflowType ] // Example for GitHub Actions
///         }
///     ];
/// }
/// [TargetDefinition]
/// public partial interface IMyTargets
/// {
///     Target MyTarget => t => t.DescribedAs("A minimal target.").Executes(() => Console.WriteLine("Executing MyTarget"));
/// }
/// </code>
/// </example>
/// <seealso cref="IBuildDefinition" />
/// <seealso cref="BuildDefinition" />
/// <seealso cref="MinimalBuildDefinitionAttribute" />
[PublicAPI]
public abstract class MinimalBuildDefinition(IServiceProvider services) : IBuildDefinition
{
    /// <inheritdoc cref="IBuildAccessor.Services" />
    public IServiceProvider Services => services;

    /// <inheritdoc cref="IBuildDefinition.TargetDefinitions" />
    public abstract IReadOnlyDictionary<string, Target> TargetDefinitions { get; }

    /// <inheritdoc cref="IBuildDefinition.ParamDefinitions" />
    public abstract IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions { get; }

    /// <inheritdoc cref="IBuildDefinition.AccessParam" />
    public abstract object? AccessParam(string paramName);

    /// <inheritdoc cref="IBuildDefinition.Workflows" />
    public virtual IReadOnlyList<WorkflowDefinition> Workflows => [];

    /// <inheritdoc cref="IBuildDefinition.GlobalWorkflowOptions" />
    public virtual IReadOnlyList<IWorkflowOption> GlobalWorkflowOptions => [];

    /// <inheritdoc cref="IBuildDefinition.SuppressParamResolution" />
    public bool SuppressParamResolution { get; set; }
}
