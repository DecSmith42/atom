namespace DecSm.Atom.Params;

[PublicAPI]
public interface IParamService
{
    [return: NotNullIfNotNull(nameof(defaultValue))]
    T? GetParam<T>(Expression<Func<T?>> paramExpression, T? defaultValue = default, Func<string?, T?>? converter = null);

    [return: NotNullIfNotNull(nameof(defaultValue))]
    T? GetParam<T>(string paramName, T? defaultValue = default, Func<string?, T?>? converter = null);

    [return: NotNullIfNotNull(nameof(defaultValue))]
    string? GetParam(string paramName, string? defaultValue = default) =>
        GetParam<string>(paramName, defaultValue);

    string MaskSecrets(string text);

    IDisposable CreateNoCacheScope();
}

internal sealed class ParamService(
    IBuildDefinition buildDefinition,
    CommandLineArgs args,
    IConfiguration config,
    IEnumerable<IVaultProvider> vaultProviders
) : IParamService
{
    private readonly record struct NoCacheScope : IDisposable
    {
        private readonly ParamService _paramService;

        public NoCacheScope(ParamService paramService)
        {
            _paramService = paramService;
            paramService.NoCache = true;
        }

        public void Dispose() =>
            _paramService.NoCache = false;
    }

    private readonly Dictionary<ParamDefinition, object?> _cache = [];
    private readonly List<string> _knownSecrets = [];
    private readonly IVaultProvider[] _vaultProviders = vaultProviders.ToArray();

    private bool NoCache { get; set; }

    public IDisposable CreateNoCacheScope() =>
        new NoCacheScope(this);

    public string MaskSecrets(string text) =>
        _knownSecrets.Aggregate(text, (current, knownSecret) => current.Replace(knownSecret, "*****", StringComparison.OrdinalIgnoreCase));

    public T? GetParam<T>(Expression<Func<T?>> paramExpression, T? defaultValue = default, Func<string?, T?>? converter = null)
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

        return GetParam(paramName, defaultValue, converter);
    }

    public T? GetParam<T>(string paramName, T? defaultValue = default, Func<string?, T?>? converter = null)
    {
        var paramDefinition = buildDefinition.ParamDefinitions.GetValueOrDefault(paramName);

        if (paramDefinition is null)
            throw new InvalidOperationException($"Parameter '{paramName}' not found.");

        return GetParam(paramDefinition, defaultValue, converter);
    }

    private T? GetParam<T>(ParamDefinition paramDefinition, T? defaultValue = default, Func<string?, T?>? converter = null)
    {
        T? result;

        if (paramDefinition.Attribute.Sources.HasFlag(ParamSource.Cache) &&
            TryGetParamFromCache(paramDefinition, converter) is (true, { } cacheValue))
            result = cacheValue;
        else if (paramDefinition.Attribute.Sources.HasFlag(ParamSource.CommandLineArgs) &&
                 TryGetParamFromArgs(paramDefinition, converter) is (true, { } argsValue))
            result = argsValue;
        else if (paramDefinition.Attribute.Sources.HasFlag(ParamSource.EnvironmentVariables) &&
                 TryGetParamFromEnvironmentVariables(paramDefinition, converter) is (true, { } envVarValue))
            result = envVarValue;
        else if (paramDefinition.Attribute.Sources.HasFlag(ParamSource.Configuration) &&
                 TryGetParamFromConfig(paramDefinition, converter) is (true, { } configValue))
            result = configValue;
        else if (paramDefinition.Attribute.Sources.HasFlag(ParamSource.UserSecrets) &&
                 paramDefinition.Attribute.IsSecret &&
                 TryGetParamFromUserSecrets(paramDefinition, converter) is (true, { } userSecretsValue))
            result = userSecretsValue;
        else if (paramDefinition.Attribute.Sources.HasFlag(ParamSource.Vault) &&
                 paramDefinition.Attribute.IsSecret &&
                 TryGetParamFromVault(paramDefinition, converter) is (true, { } vaultValue))
            result = vaultValue;
        else
            result = defaultValue;

        if (!NoCache)
            _cache[paramDefinition] = result;

        return result;
    }

    private (bool HasValue, T? Value) TryGetParamFromCache<T>(ParamDefinition paramDefinition, Func<string?, T?>? converter)
    {
        if (_cache.TryGetValue(paramDefinition, out var value))
            return (true, value switch
            {
                T valueAsT => valueAsT,
                string valueAsString => TypeUtils.Convert(valueAsString, converter),
                _ => default,
            });

        return (false, default);
    }

    private (bool HasValue, T? Value) TryGetParamFromArgs<T>(ParamDefinition paramDefinition, Func<string?, T?>? converter)
    {
        var matchingArg = args.Params.FirstOrDefault(x => x.ParamName == paramDefinition.Name);

        if (matchingArg is null)
            return (false, default);

        var convertedMatchingArg = TypeUtils.Convert(matchingArg.ParamValue, converter);

        return (true, convertedMatchingArg);
    }

    private (bool HasValue, T? Value) TryGetParamFromEnvironmentVariables<T>(ParamDefinition paramDefinition, Func<string?, T?>? converter)
    {
        var envVar = Environment.GetEnvironmentVariable(paramDefinition.Name);

        if (string.IsNullOrEmpty(envVar))
            envVar = Environment.GetEnvironmentVariable(paramDefinition.Attribute.ArgName);

        if (string.IsNullOrEmpty(envVar))
            envVar = Environment.GetEnvironmentVariable(paramDefinition
                .Attribute
                .ArgName
                .ToUpperInvariant()
                .Replace('-', '_'));

        if (string.IsNullOrEmpty(envVar))
            return (false, default);

        var convertedEnvVar = TypeUtils.Convert(envVar, converter);

        return (true, convertedEnvVar);
    }

    private (bool HasValue, T? Value) TryGetParamFromConfig<T>(ParamDefinition paramDefinition, Func<string?, T?>? converter)
    {
        var configValue = config
                              .GetSection("Params")
                              .GetSection(paramDefinition.Attribute.ArgName)
                              .Get<T>() ??
                          TypeUtils.Convert(config
                                  .GetSection("Params")[paramDefinition.Attribute.ArgName],
                              converter);

        return configValue is null
            ? (false, default)
            : (true, configValue);
    }

    private (bool HasValue, T? Value) TryGetParamFromUserSecrets<T>(ParamDefinition paramDefinition, Func<string?, T?>? converter)
    {
        var userSecretsConfigValue = config
                                         .GetSection("Secrets")
                                         .GetSection(paramDefinition.Attribute.ArgName)
                                         .Get<T>() ??
                                     TypeUtils.Convert(config
                                             .GetSection("Secrets")[paramDefinition.Attribute.ArgName],
                                         converter);

        if (userSecretsConfigValue is string userSecretsString)
            _knownSecrets.Add(userSecretsString);

        return userSecretsConfigValue is null
            ? (false, default)
            : (true, userSecretsConfigValue);
    }

    private (bool HasValue, T? Value) TryGetParamFromVault<T>(ParamDefinition paramDefinition, Func<string?, T?>? converter)
    {
        foreach (var vaultProvider in _vaultProviders)
        {
            var vaultValue = vaultProvider.GetSecret(paramDefinition.Attribute.ArgName);

            if (vaultValue is null)
                continue;

            _knownSecrets.Add(vaultValue);

            var convertedVaultValue = TypeUtils.Convert(vaultValue, converter);

            return (true, convertedVaultValue);
        }

        return (false, default);
    }
}
