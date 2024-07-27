namespace DecSm.Atom.Build.Definition;

public sealed class TargetDefinition
{
    public string Name { get; init; } = string.Empty;

    public string? Description { get; private set; }

    public List<Func<Task>> Tasks { get; private set; } = [];

    public List<string> Dependencies { get; private set; } = [];

    public List<string> RequiredParams { get; private set; } = [];

    public List<ConsumedArtifact> ConsumedArtifacts { get; private set; } = [];

    public List<ProducedArtifact> ProducedArtifacts { get; private set; } = [];

    public List<ConsumedVariable> ConsumedVariables { get; private set; } = [];

    public List<string> ProducedVariables { get; private set; } = [];

    private List<(Func<IBuildDefinition, Target> GetExtension, bool RunExtensionAfter)> Extensions { get; } = [];

    public TargetDefinition Extends<T>(Func<T, Target> x, bool runExtensionAfter = false)
        where T : IBuildDefinition
    {
        Extensions.Insert(0, (d => x((T)d), runExtensionAfter));

        return this;
    }

    internal TargetDefinition ApplyExtensions(IBuildDefinition buildDefinition)
    {
        foreach (var extension in Extensions)
        {
            var targetToExtend = extension
                .GetExtension(buildDefinition)(new()
                {
                    Name = Name,
                })
                .ApplyExtensions(buildDefinition);

            if (extension.RunExtensionAfter)
            {
                Tasks.AddRange(targetToExtend.Tasks);
                Dependencies.AddRange(targetToExtend.Dependencies);
                RequiredParams.AddRange(targetToExtend.RequiredParams);
                ConsumedArtifacts.AddRange(targetToExtend.ConsumedArtifacts);
                ProducedArtifacts.AddRange(targetToExtend.ProducedArtifacts);
                ConsumedVariables.AddRange(targetToExtend.ConsumedVariables);
                ProducedVariables.AddRange(targetToExtend.ProducedVariables);
            }
            else
            {
                Tasks = targetToExtend
                    .Tasks
                    .Concat(Tasks)
                    .ToList();

                Dependencies = targetToExtend
                    .Dependencies
                    .Concat(Dependencies)
                    .ToList();

                RequiredParams = targetToExtend
                    .RequiredParams
                    .Concat(RequiredParams)
                    .ToList();

                ConsumedArtifacts = targetToExtend
                    .ConsumedArtifacts
                    .Concat(ConsumedArtifacts)
                    .ToList();

                ProducedArtifacts = targetToExtend
                    .ProducedArtifacts
                    .Concat(ProducedArtifacts)
                    .ToList();

                ConsumedVariables = targetToExtend
                    .ConsumedVariables
                    .Concat(ConsumedVariables)
                    .ToList();

                ProducedVariables = targetToExtend
                    .ProducedVariables
                    .Concat(ProducedVariables)
                    .ToList();
            }
        }

        return this;
    }

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