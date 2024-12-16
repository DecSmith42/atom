namespace DecSm.Atom.Build.Definition;

[PublicAPI]
public sealed class TargetDefinition
{
    public string Name { get; init; } = string.Empty;

    public string? Description { get; private set; }

    public bool Hidden { get; private set; }

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

    public TargetDefinition IsHidden()
    {
        Hidden = true;

        return this;
    }

    public TargetDefinition Executes(Func<Task> task)
    {
        if (Tasks.Any(x => x() == task()))
            return this;

        Tasks.Add(task);

        return this;
    }

    public TargetDefinition DependsOn(string commandName)
    {
        Dependencies.Add(commandName);

        return this;
    }

    public TargetDefinition DependsOn(CommandDefinition command)
    {
        Dependencies.Add(command.Name);

        return this;
    }

    public TargetDefinition RequiresParam(
        [SuppressMessage("Roslynator", "RCS1163:Unused parameter")] string? param,
        [CallerArgumentExpression("param")] string _ = null!)
    {
        if (_.StartsWith("nameof("))
            RequiredParams.Add(_[7..^1]);
        else
            RequiredParams.Add(_
                .Split('.')
                .Last());

        return this;
    }

    public TargetDefinition ProducesArtifact(string artifactName, string? buildSlice = null)
    {
        ProducedArtifacts.Add(new(artifactName, buildSlice));

        return this;
    }

    public TargetDefinition ConsumesArtifact(string commandName, string artifactName, string? buildSlice = null)
    {
        ConsumedArtifacts.Add(new(commandName, artifactName, buildSlice));

        return this;
    }

    public TargetDefinition ConsumesArtifact(CommandDefinition command, string artifactName, string? buildSlice = null)
    {
        ConsumedArtifacts.Add(new(command.Name, artifactName, buildSlice));

        return this;
    }

    public TargetDefinition ProducesVariable(string variableName)
    {
        ProducedVariables.Add(variableName);

        return this;
    }

    public TargetDefinition ProducesVariable(ParamDefinition parameter)
    {
        ProducedVariables.Add(parameter.ArgName);

        return this;
    }

    public TargetDefinition ConsumesVariable(string commandName, string outputName)
    {
        ConsumedVariables.Add(new(commandName, outputName));

        return this;
    }

    public TargetDefinition ConsumesVariable(CommandDefinition command, string outputName)
    {
        ConsumedVariables.Add(new(command.Name, outputName));

        return this;
    }

    public TargetDefinition ConsumesVariable(CommandDefinition command, ParamDefinition parameter)
    {
        ConsumedVariables.Add(new(command.Name, parameter.ArgName));

        return this;
    }
}
