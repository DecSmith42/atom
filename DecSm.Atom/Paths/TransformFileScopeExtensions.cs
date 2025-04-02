namespace DecSm.Atom.Paths;

[PublicAPI]
public static class TransformFileScopeExtensions
{
    /// <inheritdoc cref="TransformFileScope.AddAsync" />
    public static async Task<TransformMultiFileScope> AddAsync(
        this Task<TransformMultiFileScope> scopeTask,
        Func<string, string> transform) =>
        await (await scopeTask).AddAsync(transform);

    /// <inheritdoc cref="TransformFileScope.AddAsync" />
    public static async Task<TransformFileScope> AddAsync(this Task<TransformFileScope> scopeTask, Func<string, string> transform) =>
        await (await scopeTask).AddAsync(transform);
}
