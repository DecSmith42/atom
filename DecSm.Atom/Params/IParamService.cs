namespace DecSm.Atom.Params;

public interface IParamService
{
    string? GetParam(Expression<Func<string?>> paramExpression);
    
    string? GetParam(string paramName);
}