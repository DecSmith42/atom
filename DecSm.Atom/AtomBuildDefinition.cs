namespace DecSm.Atom;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class AtomBuildDefinition(IServiceProvider services) : IAtomBuildDefinition
{
    public abstract Dictionary<string, Target> TargetDefinitions { get; }
    
    public abstract Dictionary<string, ParamDefinition> ParamDefinitions { get; }
    
    public IServiceProvider Services => services;
    
    public virtual WorkflowDefinition[] Workflows => [];
    
    public string? GetParam(Expression<Func<string?>> parameterExpression) =>
        Services
            .GetRequiredService<IParamService>()
            .GetParam(parameterExpression);
    
    public Task WriteVariable(string name, string value) =>
        Services
            .GetRequiredService<IWorkflowVariableProvider>()
            .WriteVariable(name, value);
}