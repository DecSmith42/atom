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
}