namespace DecSm.Atom.Build;

/// <summary>
///     Provides base functionality for accessing services and parameters within the Atom build system.
///     This interface serves as a foundation for target definitions and helpers.
/// </summary>
/// <remarks>
///     The interface includes several protected properties and methods that are commonly needed across
///     build definitions:
///     <list type="bullet">
///         <item>Service resolution with special handling for IBuildDefinition types</item>
///         <item>Logging through ILogger with automatic type-based logger creation</item>
///         <item>File system access through IAtomFileSystem</item>
///         <item>Parameter retrieval with support for expressions, default values, and custom converters</item>
///     </list>
/// </remarks>
[PublicAPI]
public interface IBuildAccessor
{
    /// <summary>
    ///     The service provider used to resolve services required by the build definition.
    /// </summary>
    IServiceProvider Services { get; }

    /// <summary>
    ///     Gets a logger instance for the implementing type.
    ///     The logger is automatically configured with the type name of the implementing class.
    /// </summary>
    protected ILogger Logger =>
        GetService<ILoggerFactory>()
            .CreateLogger(GetType());

    /// <summary>
    ///     Gets the Atom file system service for file and directory operations.
    ///     Provides access to build-specific directories and file system abstractions.
    /// </summary>
    protected IAtomFileSystem FileSystem => GetService<IAtomFileSystem>();

    /// <summary>
    ///     Shortcut for getting a service from the <see cref="Services" /> service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve.</typeparam>
    /// <returns>The service instance.</returns>
    /// <remarks>
    ///     Special behavior: If the requested type T implements IBuildDefinition, this method
    ///     returns the current instance (this) cast to type T, rather than resolving from the service provider.
    ///     This enables build definitions to access their own interface methods through dependency injection patterns.
    /// </remarks>
    protected T GetService<T>()
        where T : notnull =>
        typeof(T).GetInterface(nameof(IBuildDefinition)) != null
            ? (T)this
            : Services.GetRequiredService<T>();

    /// <summary>
    ///     Retrieves all registered services of the specified type from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of the services to retrieve.</typeparam>
    /// <returns>A collection of instances of the specified type.</returns>
    /// <remarks>
    ///     Special behavior: If the requested type T implements IBuildDefinition, this method
    ///     returns a collection containing only the current instance (this) cast to type T,
    ///     rather than resolving from the service provider.
    /// </remarks>
    protected IEnumerable<T> GetServices<T>()
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
    /// <remarks>
    ///     This method uses expression trees to extract the parameter name from the provided lambda expression.
    ///     The parameter name is derived from the member access expression, allowing for strongly-typed
    ///     parameter access while maintaining compile-time safety.
    ///     The parameter resolution follows a priority order defined by the parameter's sources configuration,
    ///     typically checking command line arguments, environment variables, configuration files, and secrets.
    /// </remarks>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    protected T? GetParam<T>(Expression<Func<T?>> parameterExpression, T? defaultValue = default, Func<string?, T?>? converter = null) =>
        Services
            .GetRequiredService<IParamService>()
            .GetParam(parameterExpression, defaultValue, converter);
}
