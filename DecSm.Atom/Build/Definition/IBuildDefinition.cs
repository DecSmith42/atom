namespace DecSm.Atom.Build.Definition;

[PublicAPI]
public interface IBuildDefinition
{
    IReadOnlyList<WorkflowDefinition> Workflows => [];

    IReadOnlyDictionary<string, Target> TargetDefinitions { get; }

    IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions { get; }

    IServiceProvider Services { get; }

    IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions => [];

    [return: NotNullIfNotNull(nameof(defaultValue))]
    T? GetParam<T>(Expression<Func<T?>> parameterExpression, T? defaultValue = default, Func<string?, T?>? converter = null);

    Task WriteVariable(string name, string value);

    void AddReportData(IReportData reportData);

    T GetService<T>()
        where T : notnull =>
        typeof(T).GetInterface(nameof(IBuildDefinition)) != null
            ? (T)this
            : Services.GetRequiredService<T>();

    IEnumerable<T> GetServices<T>()
        where T : notnull =>
        typeof(T).GetInterface(nameof(IBuildDefinition)) != null
            ? [(T)this]
            : Services.GetServices<T>();

    static virtual void Register(IServiceCollection services) { }
}
