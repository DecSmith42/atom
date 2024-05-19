namespace DecSm.Atom.Params;

public class ParamService(IAtomBuildDefinition buildDefinition, CommandLineArgs args, IConfiguration config) : IParamService
{
    private readonly Dictionary<ParamDefinition, string?> _cache = [];
    
    public string? GetParam(Expression<Func<string?>> paramExpression)
    {
        var paramName = paramExpression switch
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
        var paramDefinition = buildDefinition.ParamDefinitions.GetValueOrDefault(paramName);
        
        if (paramDefinition is null)
            throw new InvalidOperationException($"Parameter '{paramName}' not found.");
        
        return GetParam(paramDefinition);
    }
    
    public string? GetParam(ParamDefinition paramDefinition)
    {
        if (_cache.TryGetValue(paramDefinition, out var value))
            return value;
        
        if (paramDefinition.Attribute.SourceFromCliArguments)
        {
            var matchingArg = args.Params.FirstOrDefault(x => x.ParamName == paramDefinition.Name);
            
            if (matchingArg is not null)
                return _cache[paramDefinition] = matchingArg.ParamValue;
        }
        
        if (paramDefinition.Attribute.SourceFromEnvironmentVariables)
        {
            var envVar = Environment.GetEnvironmentVariable(paramDefinition.Name) ??
                         Environment.GetEnvironmentVariable(paramDefinition.Attribute.ArgName) ??
                         Environment.GetEnvironmentVariable(paramDefinition
                             .Attribute
                             .ArgName
                             .ToUpperInvariant()
                             .Replace('-', '_'));
            
            if (envVar is not null)
                return _cache[paramDefinition] = envVar;
        }
        
        if (paramDefinition.Attribute.SourceFromConfigurationFiles)
        {
            var configValue = config
                .GetSection("Params")[paramDefinition.Name];
            
            if (configValue is not null)
                return _cache[paramDefinition] = configValue;
        }
        
        if (paramDefinition.Attribute.SourceFromSecrets)
        {
            var userSecretsConfigValue = config
                .GetSection("Secrets")[paramDefinition.Name];
            
            if (userSecretsConfigValue is not null)
                return _cache[paramDefinition] = userSecretsConfigValue;
        }
        
        return _cache[paramDefinition] = null;
    }
}