namespace DecSm.Atom.Util;

internal static class ServiceAccessor<T>
    where T : notnull
{
    public static T? Service { get; set; }
}

internal static class ServiceAccessorExtensions
{
    public static void AddAccessedSingleton<TService,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService =>
        services
            .AddSingleton<TImplementation>()
            .AddSingleton<TService>(x => ServiceAccessor<TService>.Service = x.GetRequiredService<TImplementation>());
}
