namespace DecSm.Atom.Build.Definition;

[PublicAPI]
public abstract class BuildDefinition(IServiceProvider services) : ISetup, IValidateBuild
{
    public abstract IReadOnlyDictionary<string, Target> TargetDefinitions { get; }

    public abstract IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions { get; }

    public IServiceProvider Services => services;

    public virtual IReadOnlyList<WorkflowDefinition> Workflows => [];

    public virtual IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions => [];

    public T? GetParam<T>(Expression<Func<T?>> parameterExpression, T? defaultValue = default, Func<string?, T?>? converter = null) =>
        Services
            .GetRequiredService<IParamService>()
            .GetParam(parameterExpression, defaultValue, converter);

    public Task WriteVariable(string name, string value) =>
        Services
            .GetRequiredService<IWorkflowVariableService>()
            .WriteVariable(name, value);

    public void AddReportData(IReportData reportData) =>
        Services
            .GetRequiredService<IReportService>()
            .AddReportData(reportData);
}
