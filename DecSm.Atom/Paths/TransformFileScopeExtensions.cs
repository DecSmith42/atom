namespace DecSm.Atom.Paths;

public static class TransformFileScopeExtensions
{
    public static async Task<TransformMultiFileScope> AddAsync(
        this Task<TransformMultiFileScope> scopeTask,
        Func<string, string> transform) =>
        await (await scopeTask).AddAsync(transform);

    public static async Task<TransformFileScope> AddAsync(this Task<TransformFileScope> scopeTask, Func<string, string> transform) =>
        await (await scopeTask).AddAsync(transform);
}
