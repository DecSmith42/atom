namespace DecSm.Atom.Util;

internal static class ServiceStaticAccessor<T>
    where T : notnull
{
    public static T? Service { get; set; }
}

internal static class ServiceAccessorExtensions
{
    public static void AddSingletonWithStaticAccessor<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services)
        where TImplementation : class =>
        services
            .AddKeyedSingleton<TImplementation>("StaticAccess")
            .AddSingleton<TImplementation>(x =>
                ServiceStaticAccessor<TImplementation>.Service = x.GetRequiredKeyedService<TImplementation>("StaticAccess"));

    public static void AddSingletonWithStaticAccessor<TService,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService =>
        services
            .AddKeyedSingleton<TImplementation>("StaticAccess")
            .AddSingleton<TService>(x =>
                ServiceStaticAccessor<TService>.Service = x.GetRequiredKeyedService<TImplementation>("StaticAccess"));
}
