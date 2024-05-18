namespace DecSm.Atom.Build.Model;

public sealed record ExecutableTarget(TargetDefinition TargetDefinition)
{
    public List<ExecutableTarget> Dependencies { get; } = [];
    public List<ExecutableTarget> Dependents { get; } = [];
}