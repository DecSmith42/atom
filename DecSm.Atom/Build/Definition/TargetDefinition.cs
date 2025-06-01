namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Defines a target that can be modeled as <see cref="TargetModel" /> and executed.
/// </summary>
[PublicAPI]
public sealed class TargetDefinition
{
    /// <summary>
    ///     The name of the target. Target names must be unique.
    ///     Automatically set when using the <see cref="Target" /> delegate.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The description of the target.
    ///     Not required but highly recommended to provide a description for each target.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    ///     If true, the target will not appear in the help output unless -[-v]erbose is used.
    /// </summary>
    public bool Hidden { get; private set; }

    /// <summary>
    ///     The code that will be executed when the target is invoked.
    /// </summary>
    /// <remarks>
    ///     Tasks will be executed in the order of the list.
    /// </remarks>
    public List<Func<Task>> Tasks { get; private set; } = [];

    /// <summary>
    ///     Names of targets that must be executed before this target.
    /// </summary>
    public List<string> Dependencies { get; private set; } = [];

    /// <summary>
    ///     Names of parameters that must be provided when invoking the target.
    /// </summary>
    public List<string> RequiredParams { get; private set; } = [];

    /// <summary>
    ///     Artifacts that must be produced by other targets before this target can be executed.
    /// </summary>
    /// <remarks>
    ///     By default, Atom will automatically acquire the artifacts from the workflow host or a custom artifact provider.
    /// </remarks>
    public List<ConsumedArtifact> ConsumedArtifacts { get; private set; } = [];

    /// <summary>
    ///     Artifacts that will be produced by this target. Can be used as <see cref="ConsumedArtifacts" /> in other targets.
    /// </summary>
    /// <remarks>
    ///     By default, Atom will automatically publish the artifacts to the workflow host or a custom artifact provider.
    /// </remarks>
    public List<ProducedArtifact> ProducedArtifacts { get; private set; } = [];

    /// <summary>
    ///     Variables that must be produced by other targets (see <see cref="ProducedVariables" />) before this target can be executed.
    /// </summary>
    /// <remarks>
    ///     By default, Atom will automatically acquire the variables from the workflow host or a custom variable provider.
    /// </remarks>
    public List<ConsumedVariable> ConsumedVariables { get; private set; } = [];

    /// <summary>
    ///     Variables that will be produced by this target. Can be used as <see cref="ConsumedVariables" /> in other targets.
    /// </summary>
    /// <remarks>
    ///     By default, Atom will automatically publish the variables to the workflow host or a custom variable provider.
    /// </remarks>
    public List<string> ProducedVariables { get; private set; } = [];

    private List<(Func<IBuildDefinition, Target> GetExtension, bool RunExtensionAfter)> Extensions { get; } = [];

    /// <summary>
    ///     Marks this target as an extension of another (the 'base'), allowing it to inherit the tasks, dependencies, and other properties of the
    ///     base.
    /// </summary>
    /// <param name="targetToExtend">The base target to extend.</param>
    /// <param name="runExtensionAfter">If true, the tasks of this target will be executed after the inherited tasks of the base target.</param>
    /// <typeparam name="T">The type of the base target.</typeparam>
    /// <returns>This target definition.</returns>
    /// <remarks>
    ///     This affects the properties and the execution of this target, not the target being extended.
    /// </remarks>
    public TargetDefinition Extends<T>(Func<T, Target> targetToExtend, bool runExtensionAfter = false)
        where T : IBuildDefinition
    {
        Extensions.Insert(0, (d => targetToExtend((T)d), runExtensionAfter));

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

    /// <summary>
    ///     Attaches a descriptive text to the target, describing its purpose for clarity.
    /// </summary>
    /// <param name="description">Human-readable text describing the target.</param>
    /// <returns>The updated <see cref="TargetDefinition" /> instance for fluent API chaining.</returns>
    public TargetDefinition WithDescription(string description)
    {
        Description = description;

        return this;
    }

    /// <summary>
    ///     Marks the target as hidden, making it unlisted from main documentation or help utilities.
    /// </summary>
    /// <returns>The updated <see cref="TargetDefinition" /> instance for fluent API chaining.</returns>
    public TargetDefinition IsHidden()
    {
        Hidden = true;

        return this;
    }

    /// <summary>
    ///     Adds an asynchronous task to execute when the build target runs.
    /// </summary>
    /// <param name="task">The async task to execute.</param>
    /// <returns>The updated <see cref="TargetDefinition" /> instance for fluent API chaining.</returns>
    public TargetDefinition Executes(Func<Task> task)
    {
        if (Tasks.Any(x => x() == task()))
            return this;

        Tasks.Add(task);

        return this;
    }

    /// <summary>
    ///     Adds a dependency to another target by name, indicating prerequisite tasks to complete first.
    /// </summary>
    /// <param name="commandName">The name of the target upon which this target depends.</param>
    /// <returns>The updated <see cref="TargetDefinition" /> instance for fluent API chaining.</returns>
    public TargetDefinition DependsOn(string commandName)
    {
        Dependencies.Add(commandName);

        return this;
    }

    /// <summary>
    ///     Adds a dependency on another defined command target.
    /// </summary>
    /// <param name="workflowTarget">The target or command instance defining the dependency.</param>
    /// <returns>The updated <see cref="TargetDefinition" /> instance for fluent API chaining.</returns>
    public TargetDefinition DependsOn(WorkflowTargetDefinition workflowTarget)
    {
        Dependencies.Add(workflowTarget.Name);

        return this;
    }

    /// <summary>
    ///     Specifies a parameter required by this target before execution can proceed.
    /// </summary>
    /// <param name="paramName">The parameter name required by this target.</param>
    /// <returns>The updated <see cref="TargetDefinition" /> instance for fluent API chaining.</returns>
    /// <example>
    ///     You can pass in the `nameof` expression of the parameter, which will be automatically resolved to the parameter name.
    ///     Or you can pass the parameter name as a string.
    ///     <code>
    ///     string Param1 => GetParam(() => Param1, "default value");
    ///     string Param2 => GetParam(() => Param2, "default value");
    ///     Target MyTarget => d => d
    ///         .RequiresParam(nameof(Param1))
    ///         .RequiresParam("Param2")
    ///         .Executes(() => { ... });
    /// </code>
    /// </example>
    public TargetDefinition RequiresParam(string paramName)
    {
        RequiredParams.Add(paramName);

        return this;
    }

    /// <summary>
    ///     Specifies that this target requires the provided parameters to be defined for execution.
    /// </summary>
    /// <param name="paramNames">An array of parameter names that are required by the target.</param>
    /// <returns>This target definition.</returns>
    public TargetDefinition RequiresParams(params IEnumerable<string> paramNames)
    {
        RequiredParams.AddRange(paramNames);

        return this;
    }

    /// <summary>
    ///     Adds an artifact to the list of artifacts produced by the target.
    /// </summary>
    /// <param name="artifactName">The name of the artifact being produced.</param>
    /// <param name="buildSlice">An optional build slice associated with the produced artifact.</param>
    /// <returns>The current target definition.</returns>
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

    public TargetDefinition ConsumesArtifact(WorkflowTargetDefinition workflowTarget, string artifactName, string? buildSlice = null)
    {
        ConsumedArtifacts.Add(new(workflowTarget.Name, artifactName, buildSlice));

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

    public TargetDefinition ConsumesVariable(WorkflowTargetDefinition workflowTarget, string outputName)
    {
        ConsumedVariables.Add(new(workflowTarget.Name, outputName));

        return this;
    }

    public TargetDefinition ConsumesVariable(WorkflowTargetDefinition workflowTarget, ParamDefinition parameter)
    {
        ConsumedVariables.Add(new(workflowTarget.Name, parameter.ArgName));

        return this;
    }
}
