namespace DecSm.Atom.Build;

public sealed class AtomBuildIdProvider : IBuildIdProvider
{
    private static readonly DateTimeOffset BuildIdOffset = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private int? _buildId;

    public int BuildId => _buildId ??= int.Parse($"{(DateTime.UtcNow - BuildIdOffset).Seconds}");
}