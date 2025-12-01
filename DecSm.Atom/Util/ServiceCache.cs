namespace DecSm.Atom.Util;

[PublicAPI]
public sealed class ServiceCache<T>
    where T : notnull
{
    private readonly Dictionary<IBuildAccessor, T> _cache = [];

    public T Get(IBuildAccessor key) =>
        _cache.TryGetValue(key, out var result)
            ? result
            : typeof(IBuildDefinition).IsAssignableFrom(typeof(T))
                ? (T)key
                : key.Services.GetRequiredService<T>();

    public void Clear() =>
        _cache.Clear();
}
