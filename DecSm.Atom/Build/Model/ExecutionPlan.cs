namespace DecSm.Atom.Build.Model;

public sealed record ExecutionPlan
{
    public List<ExecutableTarget> Targets { get; set; } = [];

    public ExecutableTarget GetTarget(string name) =>
        Targets.FirstOrDefault(t => t.TargetDefinition.Name == name) ??
        throw new ArgumentException($"Target '{name}' not found.");
}