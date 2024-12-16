namespace DecSm.Atom.Paths;

[PublicAPI]
public static class AtomPaths
{
    public const string Root = "Root";
    public const string Artifacts = "Artifacts";
    public const string Publish = "Publish";
    public const string Temp = "Temp";

    public static void ProvidePath(
        this IServiceCollection services,
        Func<string, Func<string, RootedPath>, RootedPath?> locate,
        int priority = 1) =>
        services.AddSingleton<IPathProvider>(new PathProvider
        {
            Priority = priority,
            Locator = locate,
        });
}
