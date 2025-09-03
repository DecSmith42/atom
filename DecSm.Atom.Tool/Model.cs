namespace DecSm.Atom.Tool;

internal static class Model
{
    public static readonly Argument<string[]> RunArgs = new("runArgs")
    {
        DefaultValueFactory = _ => [],
    };

    public static readonly Option<string> ProjectOption = new("--project", "-p")
    {
        Description = "The project to run.",
    };

    public static readonly Option<string> NameOption = new("--name", "-n")
    {
        Description = "The name of the feed to add/remove.",
    };

    public static readonly Option<string> UrlOption = new("--url", "-u")
    {
        Description = "The url of the feed to add/remove.",
    };

    private static readonly Command RunCommand = new Command("run")
    {
        RunArgs,
        ProjectOption,
    }.WithAction(RunHandler.Handle);

    private static readonly Command NugetAddCommand = new Command("nuget-add")
    {
        NameOption,
        UrlOption,
    }.WithAction(NugetAddHandler.Handle);

    public static readonly RootCommand RootCommand = new RootCommand
    {
        RunArgs,
        ProjectOption,
        RunCommand,
        NugetAddCommand,
    }.WithAction(RunHandler.Handle);
}
