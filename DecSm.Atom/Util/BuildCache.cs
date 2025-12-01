namespace DecSm.Atom.Util;

[PublicAPI]
public sealed class BuildCache<T>
{
    private readonly Dictionary<IBuildAccessor, T> _cache = [];

    public bool TryGetValue(IBuildAccessor key, [MaybeNullWhen(false)] out T result) =>
        _cache.TryGetValue(key, out result);

    [return: NotNullIfNotNull("value")]
    public T Set(IBuildAccessor key, T value) =>
        _cache[key] = value;

    public void Clear() =>
        _cache.Clear();
}
