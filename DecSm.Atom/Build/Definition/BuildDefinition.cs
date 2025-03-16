namespace DecSm.Atom.Build.Definition;

/// <inheritdoc cref="IBuildDefinition" />
[PublicAPI]
public abstract class BuildDefinition(IServiceProvider services) : IBuildInfo
{
    /// <inheritdoc cref="IBuildDefinition.TargetDefinitions" />
    public abstract IReadOnlyDictionary<string, Target> TargetDefinitions { get; }

    /// <inheritdoc cref="IBuildDefinition.ParamDefinitions" />
    public abstract IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions { get; }

    /// <inheritdoc cref="IBuildDefinition.Services" />
    public IServiceProvider Services => services;

    /// <inheritdoc cref="IBuildDefinition.Workflows" />
    public virtual IReadOnlyList<WorkflowDefinition> Workflows => [];

    /// <inheritdoc cref="IBuildDefinition.DefaultWorkflowOptions" />
    public virtual IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions => [];

    /// <inheritdoc cref="IBuildDefinition.GetParam{T}(Expression{Func{T}}, T?, Func{string?, T?})" />
    public T? GetParam<T>(Expression<Func<T?>> parameterExpression, T? defaultValue = default, Func<string?, T?>? converter = null) =>
        Services
            .GetRequiredService<IParamService>()
            .GetParam(parameterExpression, defaultValue, converter);

    /// <inheritdoc cref="IBuildDefinition.WriteVariable" />
    public Task WriteVariable(string name, string value) =>
        Services
            .GetRequiredService<IWorkflowVariableService>()
            .WriteVariable(name, value);

    /// <inheritdoc cref="IBuildDefinition.AddReportData" />
    public void AddReportData(IReportData reportData) =>
        Services
            .GetRequiredService<ReportService>()
            .AddReportData(reportData);
}
