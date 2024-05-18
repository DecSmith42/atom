namespace DecSm.Atom;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class AtomBuild(IServiceProvider services) : IAtomBuild
{
    private ExecutionPlan? _executionPlan;

    public ExecutionPlan ExecutionPlan => _executionPlan ??= BuildUtil.GetExecutionPlan(TargetDefinitions);
    public abstract Dictionary<string, Target> TargetDefinitions { get; }
    public abstract Dictionary<string, ParamDefinition> ParamDefinitions { get; }
    public IServiceProvider Services => services;

    public virtual WorkflowDefinition[] Workflows => [];

    public string? GetParam(Expression<Func<string?>> parameterExpression)
    {
        var paramName = parameterExpression switch
        {
            LambdaExpression lambdaExpression => lambdaExpression.Body switch
            {
                MemberExpression memberExpression => memberExpression.Member.Name,
                _ => throw new ArgumentException("Invalid expression type."),
            },
            _ => throw new ArgumentException("Invalid expression type."),
        };

        return GetParam(paramName);
    }

    public string? GetParam(string paramName)
    {
        var paramDefinition = ParamDefinitions.GetValueOrDefault(paramName);

        if (paramDefinition is null)
            throw new InvalidOperationException($"Parameter '{paramName}' not found.");

        return Services.GetRequiredService<IParamService>().GetParam(paramDefinition);
    }
}