namespace DecSm.Atom.Build.Definition;

public interface IBuildDefinition
{
    IReadOnlyList<WorkflowDefinition> Workflows => [];

    IReadOnlyDictionary<string, Target> TargetDefinitions { get; }

    IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions { get; }

    IReadOnlyDictionary<string, Func<string?>> ParamAccessors { get; }

    IServiceProvider Services { get; }

    IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions => [];

    string? GetParam(Expression<Func<string?>> parameterExpression);

    Task WriteVariable(string name, string value);

    void AddReportData(IReportData reportData);

    T GetService<T>()
        where T : notnull =>
        Services.GetRequiredService<T>();

    static virtual void RegisterTargets(IServiceCollection services) =>
        throw new InvalidOperationException("RegisterTargets must be implemented in a derived type.");

    static virtual void Register(IServiceCollection services) =>
        throw new InvalidOperationException("Register must be implemented in a derived type.");
}