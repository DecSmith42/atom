namespace DecSm.Atom.Build.Definition;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class BuildDefinition(IServiceProvider services) : ISetup
{
    public abstract IReadOnlyDictionary<string, Target> TargetDefinitions { get; }

    public abstract IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions { get; }

    public IServiceProvider Services => services;

    public virtual IReadOnlyList<WorkflowDefinition> Workflows => [];

    public virtual IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions => [];

    public T? GetParam<T>(Expression<Func<T?>> parameterExpression, Func<string?, T?>? converter = null)
    {
        var stringValue = Services
            .GetRequiredService<IParamService>()
            .GetParam(parameterExpression);

        return stringValue is null
            ? default
            : converter is not null
                ? converter(stringValue)
                : TypeDescriptor
                    .GetConverter(typeof(T))
                    .ConvertFromInvariantString(stringValue) is T tValue
                    ? tValue
                    : default;
    }

    public Task WriteVariable(string name, string value) =>
        Services
            .GetRequiredService<IWorkflowVariableService>()
            .WriteVariable(name, value);

    public void AddReportData(IReportData reportData) =>
        Services
            .GetRequiredService<IReportService>()
            .AddReportData(reportData);
}