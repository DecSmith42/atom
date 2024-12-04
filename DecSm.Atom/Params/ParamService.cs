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
}

internal sealed class ParamService(
    IBuildDefinition buildDefinition,
    CommandLineArgs args,
    IConfiguration config,
    IEnumerable<IVaultProvider> vaultProviders
) : IParamService
{
    private readonly Dictionary<ParamDefinition, object?> _cache = [];
    private readonly List<string> _knownSecrets = [];
    private readonly IVaultProvider[] _vaultProviders = vaultProviders.ToArray();

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
        if (_cache.TryGetValue(paramDefinition, out var value))
            return value switch
            {
                T valueAsT => valueAsT,
                string valueAsString => TypeUtils.Convert(valueAsString, converter),
                _ => defaultValue,
            };

        var matchingArg = args.Params.FirstOrDefault(x => x.ParamName == paramDefinition.Name);

        if (matchingArg is not null)
        {
            var convertedMatchingArg = TypeUtils.Convert(matchingArg.ParamValue, converter);
            _cache[paramDefinition] = convertedMatchingArg;

            return convertedMatchingArg;
        }

        var envVar = Environment.GetEnvironmentVariable(paramDefinition.Name) ??
                     Environment.GetEnvironmentVariable(paramDefinition.Attribute.ArgName) ??
                     Environment.GetEnvironmentVariable(paramDefinition
                         .Attribute
                         .ArgName
                         .ToUpperInvariant()
                         .Replace('-', '_'));

        if (envVar is not null)
        {
            var convertedEnvVar = TypeUtils.Convert(envVar, converter);
            _cache[paramDefinition] = convertedEnvVar;

            return convertedEnvVar;
        }

        var configValue = config
                              .GetSection("Params")
                              .GetSection(paramDefinition.Attribute.ArgName)
                              .Get<T>() ??
                          TypeUtils.Convert(config
                                  .GetSection("Params")[paramDefinition.Attribute.ArgName],
                              converter);

        if (configValue is not null)
        {
            _cache[paramDefinition] = configValue;

            return configValue;
        }

        if (!paramDefinition.Attribute.IsSecret)
        {
            _cache[paramDefinition] = defaultValue;

            return defaultValue;
        }

        // User secrets
        var userSecretsConfigValue = config
            .GetSection("Secrets")
            .GetValue<T>(paramDefinition.Attribute.ArgName);

        if (userSecretsConfigValue is not null)
        {
            _cache[paramDefinition] = userSecretsConfigValue;

            return userSecretsConfigValue;
        }

        // Vaults
        foreach (var vaultProvider in _vaultProviders)
        {
            var vaultValue = vaultProvider.GetSecret(paramDefinition.Attribute.ArgName);

            if (vaultValue is null)
                continue;

            _knownSecrets.Add(vaultValue);

            var convertedVaultValue = TypeUtils.Convert(vaultValue, converter);
            _cache[paramDefinition] = convertedVaultValue;

            return convertedVaultValue;
        }

        _cache[paramDefinition] = defaultValue;

        return defaultValue;
    }
}
