namespace DecSm.Atom.Module.DevopsWorkflows.Generation.Options;

public sealed record DevopsPool : IWorkflowOption
{
    public IReadOnlyList<string> Demands { get; init; } = [];

    public string? Name { get; init; }

    public string? Hosted { get; init; }

    public static DevopsPool WindowsLatest { get; } = new()
    {
        Hosted = IJobRunsOn.WindowsLatestTag,
    };

    public static DevopsPool UbuntuLatest { get; } = new()
    {
        Hosted = IJobRunsOn.UbuntuLatestTag,
    };

    public static DevopsPool MacOsLatest { get; } = new()
    {
        Hosted = IJobRunsOn.MacOsLatestTag,
    };

    public static DevopsPool SetByMatrix { get; } = new()
    {
        Hosted = "$(job-runs-on)",
    };
}
