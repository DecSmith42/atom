namespace DecSm.Atom.Build.Model;

public sealed record TargetModel(string Name)
{
    public IReadOnlyList<Func<Task>> Tasks { get; init; } = [];

    public IReadOnlyList<string> RequiredParams { get; init; } = [];

    public IReadOnlyList<ConsumedArtifact> ConsumedArtifacts { get; init; } = [];

    public IReadOnlyList<ProducedArtifact> ProducedArtifacts { get; init; } = [];

    public IReadOnlyList<ConsumedVariable> ConsumedVariables { get; init; } = [];

    public IReadOnlyList<string> ProducedVariables { get; init; } = [];

    public List<TargetModel> Dependencies { get; init; } = [];
}