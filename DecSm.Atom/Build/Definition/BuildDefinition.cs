namespace DecSm.Atom.Build.Definition;

/// <inheritdoc cref="IBuildDefinition" />
[PublicAPI]
public abstract class BuildDefinition(IServiceProvider services) : IBuildDefinition
{
    /// <inheritdoc cref="IBuildAccessor.Services" />
    public IServiceProvider Services => services;

    /// <inheritdoc cref="IBuildDefinition.TargetDefinitions" />
    public abstract IReadOnlyDictionary<string, Target> TargetDefinitions { get; }

    /// <inheritdoc cref="IBuildDefinition.ParamDefinitions" />
    public abstract IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions { get; }

    /// <inheritdoc cref="IBuildDefinition.Workflows" />
    public virtual IReadOnlyList<WorkflowDefinition> Workflows => [];

    /// <inheritdoc cref="IBuildDefinition.DefaultWorkflowOptions" />
    public virtual IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions => [];
}
