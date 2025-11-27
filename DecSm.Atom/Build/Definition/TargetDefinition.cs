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
    public List<Func<CancellationToken, Task>> Tasks { get; private set; } = [];

    /// <summary>
    ///     Names of targets that must be executed before this target.
    /// </summary>
    public List<string> Dependencies { get; private set; } = [];

    /// <summary>
    ///     Names of parameters that may/must be provided when invoking the target.
    /// </summary>
    public List<DefinedParam> Params { get; private set; } = [];

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
    ///     Variables that must be produced by other targets (see <see cref="ProducedVariables" />) before this target can be
    ///     executed.
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
    ///     Marks this target as an extension of another (the 'base'), allowing it to inherit the tasks, dependencies, and
    ///     other properties of the
    ///     base.
    /// </summary>
    /// <param name="targetToExtend">The base target to extend.</param>
    /// <param name="runExtensionAfter">
    ///     If true, the tasks of this target will be executed after the inherited tasks of the
    ///     base target.
    /// </param>
    /// <typeparam name="T">The type of the base target.</typeparam>
    /// <returns>This target definition.</returns>
    /// <remarks>
    ///     This affects the properties and the execution of this target, not the target being extended.
    /// </remarks>
    public TargetDefinition Extends<T>(Func<T, Target> targetToExtend, bool runExtensionAfter = false)
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
                Params.AddRange(targetToExtend.Params);
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

                Params = targetToExtend
                    .Params
                    .Concat(Params)
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
    public TargetDefinition DescribedAs(string description)
    {
        Description = description;

        return this;
    }

    /// <summary>
    ///     Marks the target as hidden, making it unlisted from main documentation or help utilities.
    /// </summary>
    /// <returns>The updated <see cref="TargetDefinition" /> instance for fluent API chaining.</returns>
    public TargetDefinition IsHidden(bool hidden = true)
    {
        Hidden = hidden;

        return this;
    }

    /// <summary>
    ///     Adds an asynchronous task to execute when the build target runs.
    /// </summary>
    /// <param name="task">The async task to execute.</param>
    /// <returns>The updated <see cref="TargetDefinition" /> instance for fluent API chaining.</returns>
    public TargetDefinition Executes(Func<CancellationToken, Task> task)
    {
        Tasks.Add(task);

        return this;
    }

    /// <summary>
    ///     Adds an asynchronous task to execute when the build target runs.
    /// </summary>
    /// <param name="task">The async task to execute.</param>
    /// <returns>The updated <see cref="TargetDefinition" /> instance for fluent API chaining.</returns>
    public TargetDefinition Executes(Func<Task> task)
    {
        Tasks.Add(_ => task());

        return this;
    }

    /// <summary>
    ///     Adds an asynchronous task to execute when the build target runs.
    /// </summary>
    /// <param name="action">The synchronous action to execute.</param>
    /// <returns>The updated <see cref="TargetDefinition" /> instance for fluent API chaining.</returns>
    public TargetDefinition Executes(Action action)
    {
        Tasks.Add(_ =>
        {
            action();

            return Task.CompletedTask;
        });

        return this;
    }

    /// <summary>
    ///     Adds a dependency to another target by name, indicating prerequisite tasks to complete first.
    /// </summary>
    /// <param name="targetName">The name of the target upon which this target depends.</param>
    /// <returns>The updated <see cref="TargetDefinition" /> instance for fluent API chaining.</returns>
    public TargetDefinition DependsOn(string targetName)
    {
        Dependencies.Add(targetName);

        return this;
    }

    /// <summary>
    ///     Adds a dependency to another target by name, indicating prerequisite tasks to complete first.
    /// </summary>
    /// <param name="target">The target upon which this target depends.</param>
    /// <param name="targetName">
    ///     The name of the target upon which this target depends (automatically inferred from the
    ///     target).
    /// </param>
    /// <returns>The updated <see cref="TargetDefinition" /> instance for fluent API chaining.</returns>
    [SuppressMessage("ReSharper", "LocalizableElement")]
    public TargetDefinition DependsOn(Target target, [CallerArgumentExpression("target")] string? targetName = null)
    {
        if (string.IsNullOrWhiteSpace(targetName))
            throw new ArgumentException(
                "Unable to infer target name from argument expression. Please use DependsOn(\"TargetName\") overload.",
                nameof(target));

        Dependencies.Add(targetName);

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
    ///     Specifies that this target may use the provided parameters for execution.
    /// </summary>
    /// <param name="paramNames">An array of parameter names that may be used by the target.</param>
    /// <returns>This target definition.</returns>
    public TargetDefinition UsesParam(params IEnumerable<string> paramNames)
    {
        Params.AddRange(paramNames.Select(x => new DefinedParam(x, false)));

        return this;
    }

    /// <summary>
    ///     Specifies that this target requires the provided parameters to be defined for execution.
    /// </summary>
    /// <param name="paramNames">An array of parameter names that are required by the target.</param>
    /// <returns>This target definition.</returns>
    public TargetDefinition RequiresParam(params IEnumerable<string> paramNames)
    {
        Params.AddRange(paramNames.Select(x => new DefinedParam(x, true)));

        return this;
    }

    /// <summary>
    ///     Adds an artifact to the list of artifacts produced by the target.
    ///     This build system will automatically publish the produced artifacts to the workflow host or a custom artifact
    ///     provider,
    ///     allowing other targets to consume them as dependencies.
    /// </summary>
    /// <param name="artifactName">The name of the artifact being produced.</param>
    /// <param name="buildSlice">An optional build slice associated with the produced artifact.</param>
    /// <returns>The current target definition.</returns>
    public TargetDefinition ProducesArtifact(string artifactName, string? buildSlice = null)
    {
        ProducedArtifacts.Add(new(artifactName, buildSlice));

        return this;
    }

    /// <summary>
    ///     Adds an artifact to the list of artifacts produced by the target.
    ///     This build system will automatically publish the produced artifacts to the workflow host or a custom artifact
    ///     provider,
    ///     allowing other targets to consume them as dependencies.
    /// </summary>
    /// <param name="artifactName">The name of the artifact being produced.</param>
    /// <param name="buildSlice">An optional build slice associated with the produced artifact.</param>
    /// <returns>The current target definition.</returns>
    public TargetDefinition ProducesArtifacts(IEnumerable<string> artifactName, string? buildSlice = null)
    {
        ProducedArtifacts.AddRange(artifactName.Select(x => new ProducedArtifact(x, buildSlice)));

        return this;
    }

    /// <summary>
    ///     Adds an artifact to the list of artifacts that this target consumes, indicating a dependency on its production.
    ///     This build system will automatically acquire the consumed artifacts from the workflow host or a custom artifact
    ///     provider,
    ///     ensuring that the target has access to the necessary artifacts before execution.
    /// </summary>
    /// <param name="targetName">The name of the target that produces the artifact.</param>
    /// <param name="artifactName">The name of the artifact being consumed.</param>
    /// <param name="buildSlice">An optional build slice associated with the consumed artifact.</param>
    /// <returns>The current target definition.</returns>
    public TargetDefinition ConsumesArtifact(string targetName, string artifactName, string? buildSlice = null)
    {
        ConsumedArtifacts.Add(new(targetName, artifactName, buildSlice));

        return this;
    }

    /// <summary>
    ///     Adds multiple artifacts to the list of artifacts that this target consumes, indicating a dependency on their
    ///     production.
    ///     This build system will automatically acquire the consumed artifacts from the workflow host or a custom artifact
    ///     provider,
    ///     ensuring that the target has access to the necessary artifacts before execution.
    /// </summary>
    /// <param name="targetName">The name of the target that produces the artifact.</param>
    /// <param name="artifactName">The name of the artifact being consumed.</param>
    /// <param name="buildSlices">The build slices associated with the consumed artifacts.</param>
    /// <returns>The current target definition.</returns>
    public TargetDefinition ConsumesArtifact(string targetName, string artifactName, IEnumerable<string> buildSlices)
    {
        foreach (var buildSlice in buildSlices)
            ConsumedArtifacts.Add(new(targetName, artifactName, buildSlice));

        return this;
    }

    /// <summary>
    ///     Adds multiple artifacts to the list of artifacts that this target consumes, indicating a dependency on their
    ///     production.
    ///     This build system will automatically acquire the consumed artifacts from the workflow host or a custom artifact
    ///     provider,
    ///     ensuring that the target has access to the necessary artifacts before execution.
    /// </summary>
    /// <param name="targetName">The name of the target that produces the artifact.</param>
    /// <param name="artifactNames">The names of the artifacts being consumed.</param>
    /// <param name="buildSlice">An optional build slice associated with the consumed artifact.</param>
    /// <returns>The current target definition.</returns>
    public TargetDefinition ConsumesArtifact(
        string targetName,
        IEnumerable<string> artifactNames,
        string? buildSlice = null)
    {
        foreach (var artifactName in artifactNames)
            ConsumedArtifacts.Add(new(targetName, artifactName, buildSlice));

        return this;
    }

    /// <summary>
    ///     Adds multiple artifacts to the list of artifacts that this target consumes, indicating a dependency on their
    ///     production.
    ///     This build system will automatically acquire the consumed artifacts from the workflow host or a custom artifact
    ///     provider,
    ///     ensuring that the target has access to the necessary artifacts before execution.
    /// </summary>
    /// <param name="targetName">The name of the target that produces the artifact.</param>
    /// <param name="artifactNames">The names of the artifacts being consumed.</param>
    /// <param name="buildSlices">The build slices associated with the consumed artifacts.</param>
    /// <returns>The current target definition.</returns>
    public TargetDefinition ConsumesArtifact(
        string targetName,
        IEnumerable<string> artifactNames,
        IEnumerable<string> buildSlices)
    {
        var buildSlicesArray = buildSlices.ToArray();

        foreach (var artifactName in artifactNames)
        foreach (var buildSlice in buildSlicesArray)
            ConsumedArtifacts.Add(new(targetName, artifactName, buildSlice));

        return this;
    }

    /// <summary>
    ///     Adds the specified variable name to the list of variables that this target produces.
    /// </summary>
    /// <param name="variableName">The name of the variable that this target produces.</param>
    /// <returns>This target definition with the updated list of produced variables.</returns>
    /// <remarks>
    ///     This does not automatically write to the variable; it simply indicates that this target will produce a variable
    ///     with the given
    ///     name. The variable will need to be written using <see cref="WorkflowVariableService.WriteVariable" />.
    /// </remarks>
    public TargetDefinition ProducesVariable(string variableName)
    {
        ProducedVariables.Add(variableName);

        return this;
    }

    /// <summary>
    ///     Specifies a variable that this target consumes during execution, identified by its associated target name and
    ///     variable name.
    /// </summary>
    /// <param name="targetName">
    ///     The name of the target or command associated with the consumed variable.
    ///     The current target will be flagged as a dependant of the target named here.
    /// </param>
    /// <param name="outputName">The name of the variable being consumed by this target.</param>
    /// <returns>This target definition, allowing for method chaining.</returns>
    /// <remarks>
    ///     The build system will automatically acquire the consumed variables from the workflow host or a custom variable
    ///     provider
    ///     and inject them into matching params that are required or used by this target.
    /// </remarks>
    public TargetDefinition ConsumesVariable(string targetName, string outputName)
    {
        ConsumedVariables.Add(new(targetName, outputName));

        return this;
    }
}
