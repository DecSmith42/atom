namespace DecSm.Atom.Module.DevopsWorkflows.Generation.Options;

public sealed record DevopsPool : IWorkflowOption
{
    public IReadOnlyList<string> Demands { get; init; } = [];

    public string? Name { get; init; }

    public string? Hosted { get; init; }

    public static DevopsPool WindowsLatest { get; } = new()
    {
        Hosted = "windows-latest",
    };

    public static DevopsPool UbuntuLatest { get; } = new()
    {
        Hosted = "ubuntu-latest",
    };

    public static DevopsPool MacOsLatest { get; } = new()
    {
        Hosted = "macOS-latest",
    };

    public static DevopsPool SetByMatrix { get; } = new()
    {
        Hosted = "$(job-runs-on)",
    };
}
