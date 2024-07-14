namespace DecSm.Atom.Build.Model;

public sealed record BuildModel
{
    public required IReadOnlyList<TargetModel> Targets { get; init; }

    public required IReadOnlyDictionary<TargetModel, TargetState> TargetStates { get; init; }

    public TargetModel? CurrentTarget =>
        Targets.FirstOrDefault(targetModel => TargetStates[targetModel] is { Status: TargetRunState.Running });

    public TargetModel GetTarget(string name) =>
        Targets.FirstOrDefault(t => t.Name == name) ?? throw new ArgumentException($"Target '{name}' not found.");

    internal void AddOutcomeData(string key, string? value)
    {
        if (CurrentTarget is null)
            throw new InvalidOperationException("No target is currently running.");

        if (value is null)
        {
            TargetStates[CurrentTarget]
                .Data
                .Remove(key);

            return;
        }

        TargetStates[CurrentTarget]
            .Data[key] = value;
    }
}