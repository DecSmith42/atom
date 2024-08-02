namespace DecSm.Atom.Build.Definition;

public interface IBuildDefinition
{
    IReadOnlyList<WorkflowDefinition> Workflows => [];

    IReadOnlyDictionary<string, Target> TargetDefinitions { get; }

    IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions { get; }

    IServiceProvider Services { get; }

    IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions => [];

    T? GetParam<T>(Expression<Func<T?>> parameterExpression, Func<string?, T?>? converter = null);

    Task WriteVariable(string name, string value);

    void AddReportData(IReportData reportData);

    T GetService<T>()
        where T : notnull =>
        typeof(T).GetInterface(nameof(IBuildDefinition)) != null
            ? (T)this
            : Services.GetRequiredService<T>();

    static virtual void Register(IServiceCollection services) { }
}