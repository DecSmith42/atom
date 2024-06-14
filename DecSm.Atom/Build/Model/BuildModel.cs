namespace DecSm.Atom.Build.Model;

public sealed class BuildModel
{
    public required IReadOnlyList<TargetModel> Targets { get; init; }

    public required IReadOnlyDictionary<TargetModel, TargetState> TargetStates { get; init; }

    public TargetModel? CurrentTarget => Targets.FirstOrDefault(t => TargetStates[t] is { Status: TargetRunState.Running });

    public TargetModel GetTarget(string name) =>
        Targets.FirstOrDefault(t => t.Name == name) ?? throw new ArgumentException($"Target '{name}' not found.");
}