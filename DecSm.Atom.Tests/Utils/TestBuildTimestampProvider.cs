namespace DecSm.Atom.Tests.Utils;

public sealed class TestBuildTimestampProvider : IBuildTimestampProvider
{
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}
