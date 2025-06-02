namespace DecSm.Atom.Util;

/// <summary>
///     Provides static access to service instances registered in the dependency injection container.
///     This class acts as a service locator pattern implementation, allowing components to access
///     services without explicit dependency injection, particularly useful in scenarios where
///     traditional constructor injection is not feasible.
/// </summary>
/// <typeparam name="T">
///     The type of service to access. Must be a non-null reference type.
///     This type should typically be an interface or class that represents a service
///     registered in the dependency injection container.
/// </typeparam>
/// <remarks>
///     <para>
///         This class is designed to work in conjunction with the <see cref="ServiceAccessorExtensions" />
///         methods to automatically populate the <see cref="Service" /> property when services are
///         registered in the DI container.
///     </para>
///     <para>
///         <b>Warning:</b> The service will not be available until it has been resolved from the DI container.
///         Make sure the service is registered and resolved before attempting to access it statically.
///     </para>
///     <para>
///         <b>Note:</b> Using static service access can make code harder to test and
///         can introduce hidden dependencies. Use this pattern sparingly and prefer constructor
///         injection when possible.
///     </para>
///     <para>
///         Common use cases include:
///         <list type="bullet">
///             <item>
///                 <description>Logging from static contexts where DI is not available</description>
///             </item>
///             <item>
///                 <description>Utility classes that need service access but cannot use DI</description>
///             </item>
///         </list>
///     </para>
/// </remarks>
/// <example>
///     <code>
/// // Register a service with static accessor
/// services.AddSingletonWithStaticAccessor&lt;IMyService, MyService&gt;();
/// 
/// // Access the service statically
/// var result = ServiceStaticAccessor&lt;IMyService&gt;.Service?.DoSomething();
/// </code>
/// </example>
/// <seealso cref="ServiceAccessorExtensions" />
public static class ServiceStaticAccessor<T>
    where T : notnull
{
    public static T? Service { get; set; }
}

/// <summary>
///     Provides extension methods for registering services with static accessor functionality
///     in the dependency injection container.
/// </summary>
/// <remarks>
///     <para>
///         These extension methods enable the registration of services that can be accessed statically
///         through <see cref="ServiceStaticAccessor{T}" />. The methods ensure that the static accessor
///         is populated when the service is resolved from the DI container.
///     </para>
///     <para>
///         The implementation uses keyed services internally to avoid circular dependencies and
///         ensure proper service resolution.
///     </para>
/// </remarks>
internal static class ServiceAccessorExtensions
{
    /// <summary>
    ///     Registers a singleton service with static accessor functionality using the implementation type
    ///     as both the service type and implementation type.
    /// </summary>
    /// <typeparam name="TImplementation">
    ///     The type of the implementation to register. Must be a class with public constructors.
    ///     This type will be used as both the service interface and the concrete implementation.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <remarks>
    ///     <para>
    ///         This method registers the service as a singleton and ensures that the
    ///         <see cref="ServiceStaticAccessor{T}.Service" /> property is populated when the service
    ///         is first resolved from the container.
    ///     </para>
    ///     <para>
    ///         The service is registered using both keyed and regular service registration to enable
    ///         static access without creating circular dependencies.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Register a concrete class for static access
    /// services.AddSingletonWithStaticAccessor&lt;ReportService&gt;();
    /// 
    /// // Later access the service
    /// ServiceStaticAccessor&lt;ReportService&gt;.Service?.AddReportData(data, command);
    /// </code>
    /// </example>
    public static void AddSingletonWithStaticAccessor<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services)
        where TImplementation : class =>
        services
            .AddKeyedSingleton<TImplementation>("StaticAccess")
            .AddSingleton<TImplementation>(x =>
                ServiceStaticAccessor<TImplementation>.Service = x.GetRequiredKeyedService<TImplementation>("StaticAccess"));

    /// <summary>
    ///     Registers a singleton service with static accessor functionality using separate service
    ///     and implementation types.
    /// </summary>
    /// <typeparam name="TService">
    ///     The type of the service interface to register. Must be a class.
    /// </typeparam>
    /// <typeparam name="TImplementation">
    ///     The type of the implementation. Must be a class with public constructors that implements <typeparamref name="TService" />.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <remarks>
    ///     <para>
    ///         This overload allows registration of services using interface/implementation separation,
    ///         which is the recommended approach for dependency injection. The static accessor will
    ///         provide access to the service through the <typeparamref name="TService" /> interface.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Register with interface separation
    /// services.AddSingletonWithStaticAccessor&lt;IParamService, ParamService&gt;();
    /// 
    /// // Access through the interface
    /// var masked = ServiceStaticAccessor&lt;IParamService&gt;.Service?.MaskMatchingSecrets(text);
    /// </code>
    /// </example>
    public static void AddSingletonWithStaticAccessor<TService,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService =>
        services
            .AddKeyedSingleton<TService, TImplementation>("StaticAccess")
            .AddSingleton<TService>(x => ServiceStaticAccessor<TService>.Service = x.GetRequiredKeyedService<TService>("StaticAccess"));

    /// <summary>
    ///     Registers a singleton service with static accessor functionality using a factory function.
    /// </summary>
    /// <typeparam name="TService">
    ///     The type of the service to register. Must be a class.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <param name="implementationFactory">
    ///     The factory function that creates the service instance. The function receives an
    ///     <see cref="IServiceProvider" /> and an optional key object.
    /// </param>
    /// <returns>The <see cref="IServiceCollection" /> for method chaining.</returns>
    /// <remarks>
    ///     <para>
    ///         This overload provides maximum flexibility by allowing custom service creation logic
    ///         through a factory function. This is useful when the service requires complex initialization
    ///         or depends on runtime configuration.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Register with custom factory
    /// services.AddSingletonWithStaticAccessor&lt;IAnsiConsole&gt;((provider, key) =>
    /// {
    ///     var console = AnsiConsole.Create(new AnsiConsoleSettings
    ///     {
    ///         ColorSystem = ColorSystemSupport.Detect()
    ///     });
    ///     return console;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddSingletonWithStaticAccessor<TService>(
        this IServiceCollection services,
        Func<IServiceProvider, object?, TService> implementationFactory)
        where TService : class =>
        services
            .AddKeyedSingleton("StaticAccess", implementationFactory)
            .AddSingleton<TService>(x => ServiceStaticAccessor<TService>.Service = x.GetRequiredKeyedService<TService>("StaticAccess"));
}
