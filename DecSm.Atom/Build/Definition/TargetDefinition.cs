namespace DecSm.Atom.Build.Definition;

public sealed class TargetDefinition
{
    public string Name { get; init; } = string.Empty;

    public string? Description { get; set; }

    public List<Func<Task>> Tasks { get; } = [];

    public List<string> Dependencies { get; } = [];

    public List<string> RequiredParams { get; } = [];

    public List<ConsumedArtifact> ConsumedArtifacts { get; } = [];

    public List<ProducedArtifact> ProducedArtifacts { get; } = [];

    public List<ConsumedVariable> ConsumedVariables { get; } = [];

    public List<string> ProducedVariables { get; } = [];

    public TargetDefinition WithDescription(string description)
    {
        Description = description;

        return this;
    }

    public TargetDefinition Executes(Func<Task> task)
    {
        if (Tasks.Any(x => x() == task()))
            return this;

        Tasks.Add(task);

        return this;
    }

    public TargetDefinition DependsOn<T>()
    {
        var targets = typeof(T)
            .GetProperties()
            .Where(x => x.PropertyType == typeof(Target))
            .ToList();

        if (targets.Count != 1)
            throw new InvalidOperationException($"Type '{typeof(T).Name}' must have exactly one property of type 'Target'.");

        Dependencies.Add(targets[0].Name);

        return this;
    }

    public TargetDefinition RequiresParam(string? param, [CallerArgumentExpression("param")] string _ = null!)
    {
        RequiredParams.Add(_
            .Split('.')
            .Last());

        return this;
    }

    public TargetDefinition ProducesArtifact(string artifactName)
    {
        ProducedArtifacts.Add(new(artifactName));

        return this;
    }

    public TargetDefinition ConsumesArtifact<T>(string artifactName)
        where T : IBuildDefinition
    {
        var name = typeof(T).Name;

        if (name.Length > 1 && name.StartsWith('I') && char.IsUpper(name[1]))
            name = name[1..];

        ConsumedArtifacts.Add(new(name, artifactName));

        return this;
    }

    public TargetDefinition ProducesVariable(string variableName)
    {
        ProducedVariables.Add(variableName);

        return this;
    }

    public TargetDefinition ConsumesVariable<T>(string outputName)
        where T : IBuildDefinition
    {
        var name = typeof(T).Name;

        if (name.Length > 1 && name.StartsWith('I') && char.IsUpper(name[1]))
            name = name[1..];

        ConsumedVariables.Add(new(name, outputName));

        return this;
    }
}