namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Core definition of a build process within the Atom framework.
/// </summary>
/// <remarks>
///     <para>
///         Outlines the fundamental components that constitute a build, including its targets,
///         parameters, associated project file, workflow configurations, and global options.
///         Implementations of this interface, typically through the <see cref="BuildDefinition" /> or
///         <see cref="DefaultBuildDefinition" /> base classes and decorated with the <see cref="BuildDefinitionAttribute" />,
///         serve as the central point for Atom to understand and execute a build.
///     </para>
///     <para>
///         The <c>DecSm.Atom.SourceGenerators.BuildDefinitionSourceGenerator</c> plays a crucial role in populating
///         properties like <see cref="TargetDefinitions" /> and <see cref="ParamDefinitions" /> by inspecting
///         the build definition class for interfaces marked with <see cref="TargetDefinitionAttribute" />
///         and properties marked with <see cref="ParamDefinitionAttribute" /> or <see cref="SecretDefinitionAttribute" />.
///     </para>
/// </remarks>
/// <seealso cref="BuildDefinitionAttribute" />
/// <seealso cref="BuildDefinition" />
/// <seealso cref="DefaultBuildDefinition" />
/// <seealso cref="TargetDefinition" />
/// <seealso cref="ParamDefinition" />
/// <seealso cref="DecSm.Atom.Workflows.Definition.WorkflowDefinition" />
/// <seealso cref="DecSm.Atom.Workflows.Definition.Options.IWorkflowOption" />
[PublicAPI]
public interface IBuildDefinition
{
    /// <summary>
    ///     Gets the collection of workflow definitions for the build.
    /// </summary>
    /// <remarks>
    ///     Workflows define how targets are orchestrated, potentially across different CI/CD platforms.
    ///     Each <see cref="DecSm.Atom.Workflows.Definition.WorkflowDefinition" /> specifies triggers, steps, options, and the
    ///     CI/CD systems it applies to (e.g., GitHub Actions, Azure DevOps).
    ///     This is typically overridden in the user's main build class (e.g., <c>_atom/Build.cs</c>).
    /// </remarks>
    /// <example>
    ///     <code>
    /// public override IReadOnlyList&lt;WorkflowDefinition&gt; Workflows =>
    /// [
    ///     new("ContinuousIntegration")
    ///     {
    ///         Triggers = [GitPushTrigger.ToMain],
    ///         Targets = [ /* list of targets */ ],
    ///         WorkflowTypes = [Github.WorkflowType, Devops.WorkflowType]
    ///     }
    /// ];
    /// </code>
    /// </example>
    IReadOnlyList<WorkflowDefinition> Workflows { get; }

    /// <summary>
    ///     Gets the collection of target definitions for the build.
    /// </summary>
    /// <remarks>
    ///     Targets represent individual units of work or actions that can be executed as part of the build process.
    ///     This collection is typically populated by the <c>DecSm.Atom.SourceGenerators.BuildDefinitionSourceGenerator</c>
    ///     based on interfaces implementing <see cref="TargetDefinitionAttribute" /> within the build definition class.
    /// </remarks>
    /// <example>
    ///     If your build definition implements an interface <c>IMyTargets</c> which defines a target <c>Compile</c>:
    ///     <code>
    /// [TargetDefinition]
    /// public partial interface IMyTargets { Target Compile => ...; }
    /// [BuildDefinition]
    /// public partial class MyBuild : BuildDefinition, IMyTargets {}
    /// </code>
    ///     The <c>TargetDefinitions</c> property would then include the definition for the <c>Compile</c> target.
    /// </example>
    IReadOnlyDictionary<string, Target> TargetDefinitions { get; }

    /// <summary>
    ///     Gets the collection of parameter definitions for the build.
    /// </summary>
    /// <remarks>
    ///     Parameters allow for external input to customize the build process.
    ///     This collection is typically populated by the <c>DecSm.Atom.SourceGenerators.BuildDefinitionSourceGenerator</c>
    ///     based on properties within interfaces marked with <see cref="ParamDefinitionAttribute" /> or <see cref="SecretDefinitionAttribute" />
    ///     that are part of the build definition class.
    /// </remarks>
    /// <example>
    ///     If your build definition implements an interface <c>IMyParams</c> with a parameter <c>Configuration</c>:
    ///     <code>
    /// public partial interface IMyParams
    /// {
    ///     [ParamDefinition("config", "The build configuration (e.g., Debug, Release)")]
    ///     string Configuration => GetParam(() => Configuration, "Release");
    /// }
    /// [BuildDefinition]
    /// public partial class MyBuild : BuildDefinition, IMyParams {}
    /// </code>
    ///     The <c>ParamDefinitions</c> property would include the definition for the <c>Configuration</c> parameter.
    /// </example>
    IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions { get; }

    /// <summary>
    ///     Retrieves the value of a build parameter by its name.
    /// </summary>
    /// <param name="paramName">
    ///     The name of the parameter to access. This should match the key defined in <see cref="ParamDefinitions"/>.
    /// </param>
    /// <returns>
    ///     The value of the specified parameter, or <c>null</c> if the parameter is not defined or has no value.
    /// </returns>
    /// <remarks>
    ///     This method provides dynamic access to build parameters, allowing retrieval by name at runtime.
    ///     It is useful for scenarios where parameter names are looked up dynamically
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="paramName"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    ///     Thrown if the specified parameter name does not exist in <see cref="ParamDefinitions"/>.
    /// </exception>
    object? AccessParam(string paramName);

    /// <summary>
    ///     Gets the collection of global workflow options.
    /// </summary>
    /// <remarks>
    ///     These options apply to all workflows unless overridden at the individual workflow level.
    ///     They can control aspects like .NET SDK setup, artifact provider usage, or custom step injections.
    ///     <see cref="DefaultBuildDefinition" /> provides a set of common global options.
    /// </remarks>
    /// <example>
    ///     <code>
    /// public override IReadOnlyList&lt;IWorkflowOption&gt; GlobalWorkflowOptions =>
    /// [
    ///    SetupDotnetStep.FromGlobalJson(), // Sets up .NET SDK based on global.json
    ///    UseCustomArtifactProvider.Disabled // Uses CI/CD native artifacts by default
    /// ];
    /// </code>
    /// </example>
    IReadOnlyList<IWorkflowOption> GlobalWorkflowOptions { get; }

    bool SuppressParamResolution { get; protected internal set; }

    IDisposable CreateParamResolutionSuppressionScope() =>
        new ParamResolutionSuppressionScope(this);

    private readonly record struct ParamResolutionSuppressionScope : IDisposable
    {
        private readonly IBuildDefinition _buildDefinition;

        public ParamResolutionSuppressionScope(IBuildDefinition buildDefinition)
        {
            _buildDefinition = buildDefinition;
            buildDefinition.SuppressParamResolution = true;
        }

        public void Dispose() =>
            _buildDefinition.SuppressParamResolution = false;
    }
}
