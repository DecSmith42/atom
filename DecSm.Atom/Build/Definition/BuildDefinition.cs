namespace DecSm.Atom.Build.Definition;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class BuildDefinition(IServiceProvider services) : ISetup
{
    public abstract IReadOnlyDictionary<string, Target> TargetDefinitions { get; }
    
    public abstract IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions { get; }
    
    public abstract IReadOnlyDictionary<string, Func<string?>> ParamAccessors { get; }
    
    public IServiceProvider Services => services;
    
    public virtual IReadOnlyList<WorkflowDefinition> Workflows => [];
    
    public virtual IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions => [];
    
    public string? GetParam(Expression<Func<string?>> parameterExpression) =>
        Services
            .GetRequiredService<IParamService>()
            .GetParam(parameterExpression);
    
    public Task WriteVariable(string name, string value) =>
        Services
            .GetRequiredService<IWorkflowVariableService>()
            .WriteVariable(name, value);
}