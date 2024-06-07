namespace DecSm.Atom.Params;

internal sealed class ParamService(
    IBuildDefinition buildDefinition,
    CommandLineArgs args,
    IConfiguration config,
    IEnumerable<IVaultProvider> vaultProviders
) : IParamService
{
    private readonly Dictionary<ParamDefinition, string?> _cache = [];
    private readonly IVaultProvider[] _vaultProviders = vaultProviders.ToArray();
    private readonly List<string> _knownSecrets = [];
    
    public string MaskSecrets(string text) =>
        _knownSecrets.Aggregate(text, (current, knownSecret) => current.Replace(knownSecret, "*****", StringComparison.OrdinalIgnoreCase));
    
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
    
    private string? GetParam(ParamDefinition paramDefinition)
    {
        if (_cache.TryGetValue(paramDefinition, out var value))
            return value;
        
        var matchingArg = args.Params.FirstOrDefault(x => x.ParamName == paramDefinition.Name);
        
        if (matchingArg is not null)
            return _cache[paramDefinition] = matchingArg.ParamValue;
        
        var envVar = Environment.GetEnvironmentVariable(paramDefinition.Name) ??
                     Environment.GetEnvironmentVariable(paramDefinition.Attribute.ArgName) ??
                     Environment.GetEnvironmentVariable(paramDefinition
                         .Attribute
                         .ArgName
                         .ToUpperInvariant()
                         .Replace('-', '_'));
        
        if (envVar is not null)
            return _cache[paramDefinition] = envVar;
        
        // Config
        var configValue = config
            .GetSection("Params")[paramDefinition.Attribute.ArgName];
        
        if (configValue is not null)
            return _cache[paramDefinition] = configValue;
        
        if (!paramDefinition.Attribute.IsSecret)
            return _cache[paramDefinition] = null;
        
        // User secrets
        var userSecretsConfigValue = config
            .GetSection("Secrets")[paramDefinition.Attribute.ArgName];
        
        if (userSecretsConfigValue is not null)
            return _cache[paramDefinition] = userSecretsConfigValue;
        
        // Vaults
        foreach (var vaultProvider in _vaultProviders)
        {
            var vaultValue = vaultProvider.GetSecret(paramDefinition.Attribute.ArgName);
            
            if (vaultValue is null)
                continue;
            
            _knownSecrets.Add(vaultValue);
            
            return _cache[paramDefinition] = vaultValue;
        }
        
        return _cache[paramDefinition] = null;
    }
}