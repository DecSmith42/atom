namespace DecSm.Atom;

public interface IAtomBuildDefinition
{
    Dictionary<string, Target> TargetDefinitions { get; }
    Dictionary<string, ParamDefinition> ParamDefinitions { get; }
    IServiceProvider Services { get; }

    string? GetParam(Expression<Func<string?>> parameterExpression);
    
    // string? GetParam(string parameterName);
}