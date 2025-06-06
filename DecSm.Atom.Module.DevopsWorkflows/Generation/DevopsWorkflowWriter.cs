namespace DecSm.Atom.Module.DevopsWorkflows.Generation;

internal sealed partial class DevopsWorkflowWriter(
    IAtomFileSystem fileSystem,
    IBuildDefinition buildDefinition,
    BuildModel buildModel,
    ILogger<DevopsWorkflowWriter> logger
) : WorkflowFileWriter<DevopsWorkflowType>(fileSystem, logger)
{
    private readonly IAtomFileSystem _fileSystem = fileSystem;

    protected override string FileExtension => "yml";

    protected override int TabSize => 2;

    protected override RootedPath FileLocation => _fileSystem.AtomRootDirectory / ".devops" / "workflows";

    [GeneratedRegex("-+")]
    private static partial Regex HyphenReductionRegex();

    protected override void WriteWorkflow(WorkflowModel workflow)
    {
        WriteLine($"name: {workflow.Name}");
        WriteLine();

        var manualTrigger = workflow
            .Triggers
            .OfType<ManualTrigger>()
            .FirstOrDefault();

        if (manualTrigger is { Inputs.Count: > 0 })
            using (WriteSection("parameters:"))
            {
                foreach (var input in manualTrigger.Inputs)
                    using (WriteSection($"- name: {input.Name}"))
                    {
                        WriteLine($"displayName: '{input.Name} | {input.Description}'");

                        switch (input)
                        {
                            case ManualBoolInput boolInput:

                                WriteLine("type: boolean");

                                if (boolInput.DefaultValue is not null)
                                    WriteLine($"default: '{boolInput.DefaultValue.Value}'");

                                break;

                            case ManualStringInput stringInput:

                                WriteLine("type: string");

                                if (stringInput.DefaultValue is not null)
                                    WriteLine($"default: '{stringInput.DefaultValue}'");

                                break;

                            case ManualChoiceInput choiceInput:

                                WriteLine("type: string");

                                WriteLine(choiceInput.DefaultValue is not null
                                    ? $"default: {choiceInput.DefaultValue}"
                                    : $"default: '{choiceInput.Choices[0]}'");

                                using (WriteSection("values:"))
                                {
                                    foreach (var choice in choiceInput.Choices)
                                        WriteLine($"- '{choice}'");
                                }

                                break;
                        }
                    }
            }

        // TODO: Variables

        var variableGroups = workflow
            .Options
            .OfType<DevopsVariableGroup>()
            .ToArray();

        if (variableGroups.Length > 0)
            using (WriteSection("variables:"))
            {
                foreach (var variableGroup in variableGroups)
                    WriteLine($"- group: {variableGroup.Name}");
            }

        var pushTriggers = workflow
            .Triggers
            .OfType<GitPushTrigger>()
            .ToArray();

        if (pushTriggers.Length > 0)
            using (WriteSection("trigger:"))
            {
                foreach (var pushTrigger in pushTriggers)
                {
                    using (WriteSection("branches:"))
                    {
                        if (pushTrigger.IncludedBranches.Count > 0)
                            using (WriteSection("include:"))
                            {
                                foreach (var branch in pushTrigger.IncludedBranches)
                                    WriteLine($"- '{branch}'");
                            }

                        if (pushTrigger.ExcludedBranches.Count > 0)
                            using (WriteSection("exclude:"))
                            {
                                foreach (var branch in pushTrigger.ExcludedBranches)
                                    WriteLine($"- '{branch}'");
                            }
                    }

                    if (pushTrigger.IncludedPaths.Count > 0 || pushTrigger.ExcludedPaths.Count > 0)
                        using (WriteSection("paths:"))
                        {
                            if (pushTrigger.IncludedPaths.Count > 0)
                                using (WriteSection("include:"))
                                {
                                    foreach (var path in pushTrigger.IncludedPaths)
                                        WriteLine($"- '{path}'");
                                }

                            if (pushTrigger.ExcludedPaths.Count > 0)
                                using (WriteSection("exclude:"))
                                {
                                    foreach (var path in pushTrigger.ExcludedPaths)
                                        WriteLine($"- '{path}'");
                                }
                        }

                    // ReSharper disable once InvertIf
                    if (pushTrigger.IncludedTags.Count > 0 || pushTrigger.ExcludedTags.Count > 0)
                        using (WriteSection("tags:"))
                        {
                            if (pushTrigger.IncludedTags.Count > 0)
                                using (WriteSection("include:"))
                                {
                                    foreach (var tag in pushTrigger.IncludedTags)
                                        WriteLine($"- '{tag}'");
                                }

                            // ReSharper disable once InvertIf
                            if (pushTrigger.ExcludedTags.Count > 0)
                                using (WriteSection("exclude:"))
                                {
                                    foreach (var tag in pushTrigger.ExcludedTags)
                                        WriteLine($"- '{tag}'");
                                }
                        }
                }
            }

        if (manualTrigger is null && pushTriggers.Length is 0)
            WriteLine("trigger: none");

        WriteLine();

        using (WriteSection("jobs:"))
        {
            foreach (var job in workflow.Jobs)
            {
                WriteLine();
                WriteJob(workflow, job);
            }
        }
    }

    private void WriteJob(WorkflowModel workflow, WorkflowJobModel job)
    {
        using (WriteSection($"- job: {job.Name}"))
        {
            var jobRequirementNames = job.JobDependencies;

            if (jobRequirementNames.Count > 0)
                WriteLine($"dependsOn: [ {string.Join(", ", jobRequirementNames)} ]");

            if (job.MatrixDimensions.Count > 0)
                using (WriteSection("strategy:"))
                using (WriteSection("matrix:"))
                {
                    var dimensions = job
                        .MatrixDimensions
                        .Select(d => new MatrixDimension(d.Name)
                        {
                            Values = d.Values,
                        })
                        .ToArray();

                    var dimensionValues = dimensions
                        .Select(d => d.Values)
                        .ToArray();

                    var dimensionNames = dimensions
                        .Select(d => d.Name)
                        .ToArray();

                    var dimensionValueCounts = dimensionValues
                        .Select(d => d.Count)
                        .ToArray();

                    var dimensionValueIndices = new int[dimensionValues.Length];

                    var counter = 1;

                    while (true)
                    {
                        // Compute the current dimension values based on the indices
                        var currentDimensionValues = dimensionValueIndices
                            .Select((index, i) => dimensionValues[i][index])

                            // Replace any invalid characters in the dimension value with a hyphen
                            .Select(value => new string(value
                                .Select(c => char.IsLetterOrDigit(c)
                                    ? c
                                    : '-')
                                .ToArray()))

                            // Replace multiple hyphens with a single hyphen
                            .Select(value => HyphenReductionRegex()
                                .Replace(value, "-"))

                            // Trim hyphens from the start and end of the dimension value
                            .Select(value => value.Trim('-'))
                            .ToArray();

                        var dimensionValueName = $"{counter++:D3}_{string.Join("_", currentDimensionValues)}";

                        using (WriteSection($"{dimensionValueName}:"))
                        {
                            for (var i = 0; i < dimensions.Length; i++)
                                WriteLine(
                                    $"{buildDefinition.ParamDefinitions[dimensionNames[i]].ArgName}: '{dimensionValues[i][dimensionValueIndices[i]]}'");
                        }

                        var dimensionIndex = 0;

                        while (dimensionIndex < dimensionValues.Length)
                        {
                            if (dimensionValueIndices[dimensionIndex] < dimensionValueCounts[dimensionIndex] - 1)
                            {
                                dimensionValueIndices[dimensionIndex]++;

                                break;
                            }

                            dimensionValueIndices[dimensionIndex] = 0;
                            dimensionIndex++;
                        }

                        if (dimensionIndex == dimensionValues.Length)
                            break;
                    }
                }

            var poolOption = job
                                 .Options
                                 .Concat(workflow.Options)
                                 .OfType<DevopsPool>()
                                 .FirstOrDefault() ??
                             DevopsPool.UbuntuLatest;

            using (WriteSection("pool:"))
            {
                if (poolOption.Hosted is { Length: > 0 })
                    WriteLine($"vmImage: {poolOption.Hosted}");
                else if (poolOption.Name is { Length: > 0 })
                    WriteLine($"name: {poolOption.Name}");

                if (poolOption.Demands.Count > 0)
                    using (WriteSection("demands:"))
                    {
                        foreach (var demand in poolOption.Demands)
                            WriteLine($"- {demand}");
                    }
            }

            var environmentOption = job
                .Options
                .Concat(workflow.Options)
                .OfType<DeployToEnvironment>()
                .FirstOrDefault();

            if (environmentOption is not null)
                using (WriteSection("environment:"))
                    WriteLine($"name: {environmentOption.Value}");

            var variables = new Dictionary<string, string>();

            var targetsForConsumedVariableDeclaration = new List<TargetModel>();

            foreach (var commandStep in job.Steps)
            {
                var target = buildModel.GetTarget(commandStep.Name);
                targetsForConsumedVariableDeclaration.Add(target);

                if (!UseCustomArtifactProvider.IsEnabled(workflow.Options) || commandStep.SuppressArtifactPublishing)
                    continue;

                if (target.ConsumedArtifacts.Count > 0)
                    targetsForConsumedVariableDeclaration.Add(buildModel.GetTarget(nameof(IRetrieveArtifact.RetrieveArtifact)));

                if (target.ProducedArtifacts.Count > 0)
                    targetsForConsumedVariableDeclaration.Add(buildModel.GetTarget(nameof(IStoreArtifact.StoreArtifact)));
            }

            foreach (var consumedVariable in targetsForConsumedVariableDeclaration.SelectMany(x => x.ConsumedVariables))
            {
                var variableName = buildDefinition.ParamDefinitions[consumedVariable.VariableName].ArgName;

                variables[variableName] =
                    $"$[ dependencies.{consumedVariable.TargetName}.outputs['{consumedVariable.TargetName}.{variableName}'] ]";
            }

            if (variables.Count > 0)
                using (WriteSection("variables:"))
                {
                    foreach (var (name, value) in variables)
                        WriteLine($"{name}: {value}");
                }

            using (WriteSection("steps:"))
            {
                foreach (var step in job.Steps)
                {
                    WriteLine();

                    WriteStep(workflow, step, job);
                }
            }
        }
    }

    private void WriteStep(WorkflowModel workflow, WorkflowStepModel step, WorkflowJobModel job)
    {
        using (WriteSection("- checkout: self"))
            WriteLine("fetchDepth: 0");

        WriteLine();

        var commandStepTarget = buildModel.GetTarget(step.Name);

        var matrixParams = job
            .MatrixDimensions
            .Select(dimension => buildDefinition.ParamDefinitions[dimension.Name].ArgName)
            .Select(name => (Name: name, Value: $"$({name})"))
            .ToArray();

        var buildSlice = (Name: "build-slice", Value: string.Join("-", matrixParams.Select(x => x.Value)));

        if (!string.IsNullOrWhiteSpace(buildSlice.Value))
            matrixParams = matrixParams
                .Append(buildSlice)
                .ToArray();

        var setupDotnetSteps = workflow
            .Options
            .Concat(step.Options)
            .OfType<SetupDotnetStep>()
            .ToList();

        if (setupDotnetSteps.Count > 0)
            foreach (var setupDotnetStep in setupDotnetSteps)
                using (WriteSection("- task: UseDotNet@2"))
                {
                    if (setupDotnetStep.DotnetVersion is not { Length: > 0 })
                        continue;

                    using (WriteSection("inputs:"))
                        WriteLine($"version: '{setupDotnetStep.DotnetVersion}'\n");
                }

        var setupNugetSteps = workflow
            .Options
            .Concat(step.Options)
            .OfType<AddNugetFeedsStep>()
            .ToList();

        if (setupNugetSteps.Count > 0)
        {
            var feedsToAdd = setupNugetSteps
                .SelectMany(x => x.FeedsToAdd)
                .DistinctBy(x => x.FeedName)
                .ToList();

            // TODO: Remove preview flag once v1.0.0 is released
            using (WriteSection("- script: dotnet tool update --global DecSm.Atom.Tool --prerelease"))
                WriteLine("displayName: 'Install atom tool'");

            WriteLine();

            using (WriteSection("- script: |"))
            {
                foreach (var feedToAdd in feedsToAdd)
                    WriteLine($"  atom nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\"");

                WriteLine("displayName: 'Setup NuGet'");

                using (WriteSection("env:"))
                {
                    foreach (var feedToAdd in feedsToAdd)
                        WriteLine($"{AddNugetFeedsStep.GetEnvVarNameForFeed(feedToAdd.FeedName)}: $({feedToAdd.SecretName})");
                }

                WriteLine();
            }
        }

        if (commandStepTarget.ConsumedArtifacts.Count > 0)
        {
            foreach (var consumedArtifact in commandStepTarget.ConsumedArtifacts)
                if (workflow
                    .Jobs
                    .SelectMany(x => x.Steps)
                    .Single(x => x.Name == consumedArtifact.TargetName)
                    .SuppressArtifactPublishing)
                    logger.LogWarning(
                        "Workflow {WorkflowName} command {CommandName} consumes artifact {ArtifactName} from target {SourceTargetName}, which has artifact publishing suppressed; this may cause the workflow to fail",
                        workflow.Name,
                        step.Name,
                        consumedArtifact.ArtifactName,
                        consumedArtifact.TargetName);

            if (UseCustomArtifactProvider.IsEnabled(workflow.Options))
            {
                WriteCommandStep(workflow,
                    new(nameof(IRetrieveArtifact.RetrieveArtifact)),
                    buildModel.GetTarget(nameof(IRetrieveArtifact.RetrieveArtifact)),
                    [
                        ("atom-artifacts", string.Join(",", commandStepTarget.ConsumedArtifacts.Select(x => x.ArtifactName))),
                        !string.IsNullOrWhiteSpace(buildSlice.Value)
                            ? buildSlice
                            : default,
                    ],
                    false);

                WriteLine();
            }
            else
            {
                foreach (var artifact in commandStepTarget.ConsumedArtifacts)
                {
                    var name = !string.IsNullOrWhiteSpace(buildSlice.Value)
                        ? $"{artifact.ArtifactName}-{buildSlice.Value}"
                        : artifact.ArtifactName;

                    using (WriteSection("- task: DownloadPipelineArtifact@2"))
                    {
                        WriteLine($"displayName: {name}");

                        using (WriteSection("inputs:"))
                        {
                            WriteLine($"artifact: {name}");
                            WriteLine($"path: \"{Devops.PipelineArtifactDirectory}/{artifact.ArtifactName}\"");
                        }
                    }

                    WriteLine();
                }
            }
        }

        WriteCommandStep(workflow, step, commandStepTarget, matrixParams, true);

        // ReSharper disable once InvertIf
        if (commandStepTarget.ProducedArtifacts.Count > 0 && !step.SuppressArtifactPublishing)
        {
            if (UseCustomArtifactProvider.IsEnabled(workflow.Options))
            {
                WriteLine();

                WriteCommandStep(workflow,
                    new(nameof(IStoreArtifact.StoreArtifact)),
                    buildModel.GetTarget(nameof(IStoreArtifact.StoreArtifact)),
                    [
                        ("atom-artifacts", string.Join(",", commandStepTarget.ProducedArtifacts.Select(x => x.ArtifactName))),
                        !string.IsNullOrWhiteSpace(buildSlice.Value)
                            ? buildSlice
                            : default,
                    ],
                    false);
            }
            else
            {
                if (commandStepTarget.ProducedArtifacts.Count > 0)
                    WriteLine();

                foreach (var artifact in commandStepTarget.ProducedArtifacts)
                {
                    var name = !string.IsNullOrWhiteSpace(buildSlice.Value)
                        ? $"{artifact.ArtifactName}-{buildSlice.Value}"
                        : artifact.ArtifactName;

                    using (WriteSection("- task: PublishPipelineArtifact@1"))
                    {
                        WriteLine($"displayName: {name}");

                        using (WriteSection("inputs:"))
                        {
                            WriteLine($"artifactName: {name}");
                            WriteLine($"targetPath: \"{Devops.PipelinePublishDirectory}/{artifact.ArtifactName}\"");
                        }
                    }

                    WriteLine();
                }
            }
        }
    }

    private void WriteCommandStep(
        WorkflowModel workflow,
        WorkflowStepModel workflowStep,
        TargetModel target,
        (string name, string value)[] extraParams,
        bool includeName)
    {
        var projectName = _fileSystem.ProjectName;

        using (WriteSection($"- script: dotnet run --project {projectName}/{projectName}.csproj {workflowStep.Name} --skip --headless"))
        {
            if (includeName)
                WriteLine($"name: {workflowStep.Name}");

            var env = new Dictionary<string, string>();

            foreach (var githubManualTrigger in workflow.Triggers.OfType<ManualTrigger>())
            {
                if (githubManualTrigger.Inputs is null or [])
                    continue;

                foreach (var input in githubManualTrigger.Inputs.Where(i => target
                             .RequiredParams
                             .Select(p => p.ArgName)
                             .Any(p => p == i.Name)))
                    env[input.Name] = $"${{{{ parameters.{input.Name} }}}}";
            }

            foreach (var consumedVariable in target.ConsumedVariables)
                env[buildDefinition.ParamDefinitions[consumedVariable.VariableName].ArgName] =
                    $"$({buildDefinition.ParamDefinitions[consumedVariable.VariableName].ArgName})";

            var requiredSecrets = target
                .RequiredParams
                .Where(x => x.IsSecret)
                .Select(x => x)
                .ToArray();

            if (requiredSecrets.Any(x => x.IsSecret))
            {
                foreach (var injectedSecret in workflow.Options.OfType<WorkflowSecretsSecretInjection>())
                {
                    if (injectedSecret.Value is null)
                    {
                        logger.LogWarning("Workflow {WorkflowName} command {CommandName} has a secret injection with a null value",
                            workflow.Name,
                            workflowStep.Name);

                        continue;
                    }

                    var paramDefinition = buildDefinition.ParamDefinitions.GetValueOrDefault(injectedSecret.Value);

                    if (paramDefinition is not null)
                        env[paramDefinition.ArgName] = $"$({paramDefinition.ArgName.ToUpper().Replace('-', '_')})";
                }

                foreach (var injectedEnvVar in workflow.Options.OfType<WorkflowSecretsEnvironmentInjection>())
                {
                    if (injectedEnvVar.Value is null)
                    {
                        logger.LogWarning(
                            "Workflow {WorkflowName} command {CommandName} has a secret environment variable injection with a null value",
                            workflow.Name,
                            workflowStep.Name);

                        continue;
                    }

                    var paramDefinition = buildDefinition.ParamDefinitions.GetValueOrDefault(injectedEnvVar.Value);

                    if (paramDefinition is not null)
                        env[paramDefinition.ArgName] = $"$({paramDefinition.ArgName.ToUpper().Replace('-', '_')})";
                }
            }

            foreach (var requiredSecret in requiredSecrets)
            {
                var injectedSecret = workflow
                    .Options
                    .Concat(workflowStep.Options)
                    .OfType<WorkflowSecretInjection>()
                    .FirstOrDefault(x => x.Value == requiredSecret.Name);

                if (injectedSecret is not null)
                    env[requiredSecret.ArgName] = $"$({requiredSecret.ArgName.ToUpper().Replace('-', '_')})";
            }

            var environmentInjections = workflow.Options.OfType<WorkflowEnvironmentInjection>();
            var paramInjections = workflow.Options.OfType<WorkflowParamInjection>();
            environmentInjections = environmentInjections.Where(e => paramInjections.All(p => p.Name != e.Value));

            foreach (var environmentInjection in environmentInjections.Where(e => e.Value is not null))
            {
                if (!buildDefinition.ParamDefinitions.TryGetValue(environmentInjection.Value!, out var paramDefinition))
                {
                    logger.LogWarning(
                        "Workflow {WorkflowName} command {CommandName} has an injection for parameter {ParamName} that does not exist",
                        workflow.Name,
                        workflowStep.Name,
                        environmentInjection.Value);

                    continue;
                }

                env[paramDefinition.ArgName] = $"$({paramDefinition.ArgName.ToUpper().Replace('-', '_')})";
            }

            foreach (var paramInjection in paramInjections)
            {
                if (!buildDefinition.ParamDefinitions.TryGetValue(paramInjection.Name, out var paramDefinition))
                {
                    logger.LogWarning(
                        "Workflow {WorkflowName} command {CommandName} has an injection for parameter {ParamName} that does not exist",
                        workflow.Name,
                        workflowStep.Name,
                        paramInjection.Name);

                    continue;
                }

                env[paramDefinition.ArgName] = paramInjection.Value;
            }

            var validEnv = env
                .Where(static x => x.Value is { Length: > 0 })
                .ToList();

            var validExtraParams = extraParams
                .Where(static x => x.value is { Length: > 0 })
                .ToList();

            // ReSharper disable once InvertIf
            if (validEnv.Count > 0 || validExtraParams.Count > 0)
                using (WriteSection("env:"))
                {
                    foreach (var (key, value) in validEnv)
                        WriteLine($"{key}: {value}");

                    foreach (var (key, value) in validExtraParams)
                        WriteLine($"{key}: {value}");
                }
        }
    }
}
