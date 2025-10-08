namespace DecSm.Atom.Params;

/// <summary>
///     Provides a comprehensive parameter management system for the Atom framework.
///     Enables retrieval of configuration parameters from multiple sources with flexible
///     type conversion and security features for handling sensitive data.
/// </summary>
/// <remarks>
///     <para>
///         The service resolves parameters from multiple sources in the following priority order:
///     </para>
///     <list type="number">
///         <item>
///             <description>Cache - Previously resolved values (if caching enabled)</description>
///         </item>
///         <item>
///             <description>Command Line Arguments - Parameters passed via command line</description>
///         </item>
///         <item>
///             <description>Environment Variables - System environment variables</description>
///         </item>
///         <item>
///             <description>Configuration - Application configuration files</description>
///         </item>
///         <item>
///             <description>Secrets - Secure parameter providers (for sensitive data)</description>
///         </item>
///         <item>
///             <description>Interactive Console - User input prompts (when in interactive mode)</description>
///         </item>
///         <item>
///             <description>Default Value - The provided fallback value</description>
///         </item>
///     </list>
///     <para>
///         This interface is typically used in conjunction with <see cref="ParamDefinitionAttribute" />
///         for defining parameter metadata, <see cref="SecretDefinitionAttribute" /> for sensitive parameters,
///         and <see cref="IBuildDefinition" /> for build configuration.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// // Using lambda expression
/// var apiKey = paramService.GetParam(() => ApiKey, "default-key");
/// // Using string name with type conversion
/// var timeout = paramService.GetParam&lt;int&gt;("request-timeout", 30);
/// // String convenience method
/// var environment = paramService.GetParam("environment", "development");
/// // Custom converter
/// var customValue = paramService.GetParam&lt;MyType&gt;("custom-param",
///     converter: str => MyType.Parse(str));
/// // Disabling cache temporarily
/// using (paramService.CreateNoCacheScope())
/// {
///     var freshValue = paramService.GetParam(() => SomeParam);
/// }
/// // Masking secrets in logs
/// var logMessage = paramService.MaskMatchingSecrets("API key: secret123");
/// </code>
/// </example>
[PublicAPI]
public interface IParamService
{
    /// <summary>
    ///     Retrieves a parameter value using a lambda expression to specify the parameter name.
    /// </summary>
    /// <typeparam name="T">The type of the parameter value to retrieve.</typeparam>
    /// <param name="paramExpression">
    ///     Lambda expression that identifies the parameter (e.g., <c>() => MyParam</c>).
    ///     The expression body must be a member access expression.
    /// </param>
    /// <param name="defaultValue">
    ///     Value to return if parameter is not found or cannot be resolved from any source.
    /// </param>
    /// <param name="converter">
    ///     Optional custom conversion function for transforming string values to the target type.
    ///     If not provided, default type conversion will be used.
    /// </param>
    /// <returns>
    ///     The parameter value of type <typeparamref name="T" />, or the <paramref name="defaultValue" />
    ///     if the parameter cannot be resolved from any configured source.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="paramExpression" /> is not a valid lambda expression
    ///     or does not contain a member access expression.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the parameter name extracted from the expression is not found
    ///     in the build definition's parameter definitions.
    /// </exception>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    T? GetParam<T>(Expression<Func<T?>> paramExpression, T? defaultValue = default, Func<string?, T?>? converter = null);

    /// <summary>
    ///     Retrieves a parameter value by its string name.
    /// </summary>
    /// <typeparam name="T">The type of the parameter value to retrieve.</typeparam>
    /// <param name="paramName">
    ///     The name of the parameter to retrieve. Must match a parameter defined
    ///     in the build definition's parameter definitions.
    /// </param>
    /// <param name="defaultValue">
    ///     Value to return if parameter is not found or cannot be resolved from any source.
    /// </param>
    /// <param name="converter">
    ///     Optional custom conversion function for transforming string values to the target type.
    ///     If not provided, default type conversion will be used.
    /// </param>
    /// <returns>
    ///     The parameter value of type <typeparamref name="T" />, or the <paramref name="defaultValue" />
    ///     if the parameter cannot be resolved from any configured source.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when <paramref name="paramName" /> is not found in the build definition's
    ///     parameter definitions.
    /// </exception>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    T? GetParam<T>(string paramName, T? defaultValue = default, Func<string?, T?>? converter = null);

    /// <summary>
    ///     Convenience method for retrieving string parameters without explicit type specification.
    /// </summary>
    /// <param name="paramName">
    ///     The name of the parameter to retrieve. Must match a parameter defined
    ///     in the build definition's parameter definitions.
    /// </param>
    /// <param name="defaultValue">
    ///     String value to return if parameter is not found or cannot be resolved from any source.
    /// </param>
    /// <returns>
    ///     The parameter value as a string, or the <paramref name="defaultValue" /> if the parameter
    ///     cannot be resolved from any configured source.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when <paramref name="paramName" /> is not found in the build definition's
    ///     parameter definitions.
    /// </exception>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    string? GetParam(string paramName, string? defaultValue = null) =>
        GetParam<string>(paramName, defaultValue);

    /// <summary>
    ///     Masks known secret values in the provided text for secure logging and output.
    /// </summary>
    /// <param name="text">The text to scan and mask for secrets.</param>
    /// <returns>
    ///     Text with secret values replaced by asterisks (<c>*****</c>) using case-insensitive matching.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         This method scans the input text for any values that have been previously resolved
    ///         as secrets through the parameter service. When a match is found, the secret value
    ///         is replaced with asterisks to prevent accidental exposure in logs or console output.
    ///     </para>
    ///     <para>
    ///         The masking uses case-insensitive string comparison and only masks values that have
    ///         a length greater than zero.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Assuming "secret123" was previously resolved as a secret parameter
    /// var masked = paramService.MaskMatchingSecrets("API key: secret123");
    /// // Result: "API key: *****"
    /// </code>
    /// </example>
    string MaskMatchingSecrets(string text);

    /// <summary>
    ///     Creates a scope where parameter caching is temporarily disabled.
    /// </summary>
    /// <returns>
    ///     A disposable scope object that restores caching when disposed.
    ///     Use within a <c>using</c> statement to ensure proper cleanup.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         When caching is disabled, all parameter lookups will bypass the cache and
    ///         resolve values directly from the configured sources. This is useful when you need
    ///         to ensure fresh values are retrieved, such as during testing scenarios or when
    ///         parameters may have changed externally.
    ///     </para>
    ///     <para>
    ///         The cache state is automatically restored when the returned scope is disposed,
    ///         making this safe to use in nested scenarios.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Normal caching behavior
    /// var cachedValue = paramService.GetParam(() => MyParam);
    /// // Temporarily disable caching
    /// using (paramService.CreateNoCacheScope())
    /// {
    ///     var freshValue = paramService.GetParam(() => MyParam);
    ///     // This will always resolve from sources, not cache
    /// }
    /// // Caching restored
    /// var cachedAgain = paramService.GetParam(() => MyParam);
    /// </code>
    /// </example>
    IDisposable CreateNoCacheScope();
}

internal sealed class ParamService(
    IBuildDefinition buildDefinition,
    CommandLineArgs args,
    IAnsiConsole console,
    IConfiguration config,
    ILogger<ParamService> logger,
    IEnumerable<ISecretsProvider> secretsProviders
) : IParamService
{
    private readonly Dictionary<string, object?> _cache = [];
    private readonly List<string> _knownSecrets = [];
    private readonly ISecretsProvider[] _secretsProviders = secretsProviders.ToArray();

    private bool NoCache { get; set; }

    public IDisposable CreateNoCacheScope() =>
        new NoCacheScope(this);

    public string MaskMatchingSecrets(string text) =>
        _knownSecrets.Aggregate(text,
            (current, knownSecret) => knownSecret is { Length: > 0 }
                ? current.Replace(knownSecret, "*****", StringComparison.OrdinalIgnoreCase)
                : current);

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

        return paramDefinition is null
            ? throw new InvalidOperationException($"Parameter '{paramName}' not found.")
            : GetParam(paramDefinition, defaultValue, converter);
    }

    private T? GetParam<T>(ParamDefinition paramDefinition, T? defaultValue = default, Func<string?, T?>? converter = null)
    {
        if (buildDefinition.SuppressParamResolution)
            return defaultValue;

        T? result;

        if (paramDefinition.Sources.HasFlag(ParamSource.Cache) &&
            TryGetParamFromCache(paramDefinition, _cache, converter) is (true, { } cacheValue))
        {
            result = cacheValue;
            logger.LogDebug("Resolved param {ParamName} from cache", paramDefinition.Name);
        }
        else if (paramDefinition.Sources.HasFlag(ParamSource.CommandLineArgs) &&
                 TryGetParamFromArgs(paramDefinition, args, converter) is (true, { } argsValue))
        {
            result = argsValue;
            logger.LogDebug("Resolved param {ParamName} from command line args", paramDefinition.Name);
        }
        else if (paramDefinition.Sources.HasFlag(ParamSource.EnvironmentVariables) &&
                 TryGetParamFromEnvironmentVariables(paramDefinition, converter) is (true, { } envVarValue))
        {
            result = envVarValue;
            logger.LogDebug("Resolved param {ParamName} from environment variables", paramDefinition.Name);
        }
        else if (paramDefinition.Sources.HasFlag(ParamSource.Configuration) &&
                 TryGetParamFromConfig(paramDefinition, config, converter) is (true, { } configValue))
        {
            result = configValue;
            logger.LogDebug("Resolved param {ParamName} from configuration", paramDefinition.Name);
        }
        else if (paramDefinition.Sources.HasFlag(ParamSource.Secrets) &&
                 paramDefinition.IsSecret &&
                 TryGetParamFromSecrets(paramDefinition, _secretsProviders, converter) is (true, { } secretValue))
        {
            result = secretValue;
            logger.LogDebug("Resolved param {ParamName} from secrets", paramDefinition.Name);
        }
        else if (args is { HasHeadless: false, HasInteractive: true, HasHelp: false } &&
                 TryGetParamFromConsole(paramDefinition, console, defaultValue, _cache, converter) is (true, { } userValue))
        {
            result = userValue;
            logger.LogDebug("Resolved param {ParamName} from user input", paramDefinition.Name);
        }
        else
        {
            result = defaultValue;

            logger.LogDebug("Parameter '{ParamName}' not resolved, using default value '{DefaultValue}'",
                paramDefinition.Name,
                result is null
                    ? "(null)"
                    : result is ""
                        ? "(empty string)"
                        : paramDefinition.IsSecret
                            ? "*****"
                            : result.ToString());
        }

        if (!NoCache)
            _cache[paramDefinition.Name] = result;

        if (paramDefinition.IsSecret)
            _knownSecrets.Add(result?.ToString() ?? string.Empty);

        logger.LogDebug("Parameter '{ParamName}' resolved to '{Value}'",
            paramDefinition.Name,
            result is null
                ? "(null)"
                : result is ""
                    ? "(empty string)"
                    : paramDefinition.IsSecret
                        ? "*****"
                        : result.ToString());

        return result;
    }

    private static (bool HasValue, T? Value) TryGetParamFromCache<T>(
        ParamDefinition paramDefinition,
        Dictionary<string, object?> cache,
        Func<string?, T?>? converter)
    {
        if (cache.TryGetValue(paramDefinition.Name, out var value))
            return (true, value switch
            {
                T valueAsT => valueAsT,
                string valueAsString => TypeUtil.Convert(valueAsString, converter),
                _ => default,
            });

        return (false, default);
    }

    private static (bool HasValue, T? Value) TryGetParamFromArgs<T>(
        ParamDefinition paramDefinition,
        CommandLineArgs commandLineArgs,
        Func<string?, T?>? converter)
    {
        var matchingArg = commandLineArgs.Params.FirstOrDefault(x => x.ParamName == paramDefinition.Name);

        if (matchingArg is null)
            return (false, default);

        var convertedMatchingArg = TypeUtil.Convert(matchingArg.ParamValue, converter);

        return (true, convertedMatchingArg);
    }

    private static (bool HasValue, T? Value) TryGetParamFromEnvironmentVariables<T>(
        ParamDefinition paramDefinition,
        Func<string?, T?>? converter)
    {
        var envVar = Environment.GetEnvironmentVariable(paramDefinition.Name);

        if (string.IsNullOrEmpty(envVar))
            envVar = Environment.GetEnvironmentVariable(paramDefinition.ArgName);

        if (string.IsNullOrEmpty(envVar))
            envVar = Environment.GetEnvironmentVariable(paramDefinition
                .ArgName
                .ToUpperInvariant()
                .Replace('-', '_'));

        if (string.IsNullOrEmpty(envVar))
            return (false, default);

        var convertedEnvVar = TypeUtil.Convert(envVar, converter);

        return (true, convertedEnvVar);
    }

    private static (bool HasValue, T? Value) TryGetParamFromConfig<T>(
        ParamDefinition paramDefinition,
        IConfiguration configuration,
        Func<string?, T?>? converter)
    {
        var configSection = configuration
            .GetSection("Params")
            .GetSection(paramDefinition.ArgName);

        var configValue = configSection.Exists()
            ? configSection.Get<T?>() ?? TypeUtil.Convert(configSection.Value, converter)
            : default;

        return (configSection.Exists(), configValue);
    }

    private static (bool HasValue, T? Value) TryGetParamFromSecrets<T>(
        ParamDefinition paramDefinition,
        ISecretsProvider[] vaultProviders,
        Func<string?, T?>? converter)
    {
        foreach (var vaultProvider in vaultProviders)
        {
            var vaultValue = vaultProvider.GetSecret(paramDefinition.ArgName);

            if (vaultValue is null)
                continue;

            var convertedVaultValue = TypeUtil.Convert(vaultValue, converter);

            return (true, convertedVaultValue);
        }

        return (false, default);
    }

    private static (bool HasValue, T? Value) TryGetParamFromConsole<T>(
        ParamDefinition paramDefinition,
        IAnsiConsole ansiConsole,
        T? defaultValue,
        Dictionary<string, object?> cache,
        Func<string?, T?>? converter)
    {
        var result = ansiConsole.Prompt(new TextPrompt<string>($"Enter value for parameter '{paramDefinition.ArgName}': ")
        {
            IsSecret = paramDefinition.IsSecret,
            Mask = '*',
            ShowDefaultValue = defaultValue is not null,
            AllowEmpty = defaultValue is not null,
        });

        if (result is not { Length: > 0 })
            return (false, default);

        var convertedResult = TypeUtil.Convert(result, converter);

        // Force cache as it is a user input
        cache[paramDefinition.Name] = convertedResult;

        return (true, convertedResult);
    }

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
}
