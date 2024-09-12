namespace DecSm.Atom.Build.Model;

public sealed record TargetModel(string Name, string? Description, bool IsHidden)
{
    public required IReadOnlyList<Func<Task>> Tasks { get; init; }

    public required IReadOnlyList<string> RequiredParams { get; init; }

    public required IReadOnlyList<ConsumedArtifact> ConsumedArtifacts { get; init; }

    public required IReadOnlyList<ProducedArtifact> ProducedArtifacts { get; init; }

    public required IReadOnlyList<ConsumedVariable> ConsumedVariables { get; init; }

    public required IReadOnlyList<string> ProducedVariables { get; init; }

    public required IReadOnlyList<TargetModel> Dependencies { get; init; }
}
