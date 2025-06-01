namespace DecSm.Atom.Build;

public interface IBuildAccessor
{
    /// <summary>
    ///     The service provider used to resolve services required by the build definition.
    /// </summary>
    IServiceProvider Services { get; }

    protected ILogger Logger =>
        GetService<ILoggerFactory>()
            .CreateLogger(GetType());

    protected IAtomFileSystem FileSystem => GetService<IAtomFileSystem>();

    /// <summary>
    ///     Shortcut for getting a service from the <see cref="Services" /> service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve.</typeparam>
    /// <returns>The service instance.</returns>
    T GetService<T>()
        where T : notnull =>
        typeof(T).GetInterface(nameof(IBuildDefinition)) != null
            ? (T)this
            : Services.GetRequiredService<T>();

    /// <summary>
    ///     Retrieves all registered services of the specified type from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of the services to retrieve.</typeparam>
    /// <returns>A collection of instances of the specified type.</returns>
    IEnumerable<T> GetServices<T>()
        where T : notnull =>
        typeof(T).GetInterface(nameof(IBuildDefinition)) != null
            ? [(T)this]
            : Services.GetServices<T>();

    /// <summary>
    ///     Retrieves a parameter value using the specified expression, with an optional default value and converter.
    /// </summary>
    /// <typeparam name="T">The type of the parameter value.</typeparam>
    /// <param name="parameterExpression">An expression identifying the parameter.</param>
    /// <param name="defaultValue">The default value to return if the parameter is not set.</param>
    /// <param name="converter">An optional function to convert the parameter value from a string.</param>
    /// <returns>The parameter value, or the default value if not set.</returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    T? GetParam<T>(Expression<Func<T?>> parameterExpression, T? defaultValue = default, Func<string?, T?>? converter = null) =>
        Services
            .GetRequiredService<IParamService>()
            .GetParam(parameterExpression, defaultValue, converter);
}
