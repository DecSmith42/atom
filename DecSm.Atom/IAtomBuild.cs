namespace DecSm.Atom;

public interface IAtomBuild
{
    Dictionary<string, Target> TargetDefinitions { get; }
    Dictionary<string, ParamDefinition> ParamDefinitions { get; }
    IServiceProvider Services { get; }
    ExecutionPlan ExecutionPlan { get; }

    string? GetParam(Expression<Func<string?>> parameterExpression);
    string? GetParam(string parameterName);
}