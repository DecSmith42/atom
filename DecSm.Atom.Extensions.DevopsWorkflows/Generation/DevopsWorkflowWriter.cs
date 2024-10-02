using System.Text.RegularExpressions;

namespace DecSm.Atom.Extensions.DevopsWorkflows.Generation;

public sealed class DevopsWorkflowWriter(
    IAtomFileSystem fileSystem,
    IBuildDefinition buildDefinition,
    BuildModel buildModel,
    ILogger<DevopsWorkflowWriter> logger
) : WorkflowFileWriter<DevopsWorkflowType>(fileSystem, logger)
{
    private readonly IAtomFileSystem _fileSystem = fileSystem;

    protected override string FileExtension => "yml";

    protected override int TabSize => 2;

    protected override AbsolutePath FileLocation => _fileSystem.AtomRootDirectory / ".devops" / "workflows";

    protected override void WriteWorkflow(WorkflowModel workflow)
    {
        WriteLine($"name: {workflow.Name}");
        WriteLine();

        var manualTrigger = workflow
            .Triggers
            .OfType<DevopsManualTrigger>()
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
                            case DevopsManualBoolInput boolInput:

                                WriteLine("type: boolean");

                                if (boolInput.DefaultValue is not null)
                                    WriteLine($"default: '{boolInput.DefaultValue.Value}'");

                                break;

                            case DevopsManualStringInput stringInput:

                                WriteLine("type: string");

                                if (stringInput.DefaultValue is not null)
                                    WriteLine($"default: '{stringInput.DefaultValue}'");

                                break;

                            case DevopsManualChoiceInput choiceInput:

                                WriteLine("type: string");

                                if (choiceInput.DefaultValue is not null)
                                    WriteLine($"default: {choiceInput.DefaultValue}");
                                else
                                    WriteLine($"default: '{choiceInput.Choices[0]}'");

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
            .OfType<DevopsPushTrigger>()
            .ToArray();

        if (pushTriggers.Length is 0)
            WriteLine("trigger: none");
        else
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

                    if (pushTrigger.IncludedTags.Count > 0 || pushTrigger.ExcludedTags.Count > 0)
                        using (WriteSection("tags:"))
                        {
                            if (pushTrigger.IncludedTags.Count > 0)
                                using (WriteSection("include:"))
                                {
                                    foreach (var tag in pushTrigger.IncludedTags)
                                        WriteLine($"- '{tag}'");
                                }

                            if (pushTrigger.ExcludedTags.Count > 0)
                                using (WriteSection("exclude:"))
                                {
                                    foreach (var tag in pushTrigger.ExcludedTags)
                                        WriteLine($"- '{tag}'");
                                }
                        }
                }
            }

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
                        .Select(d => new MatrixDimension(d.Name, d.Values))
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
                            .Select(value => Regex.Replace(value, "-+", "-"))

                            // Trim hyphens from the start and end of the dimension value
                            .Select(value => value.Trim('-'))
                            .ToArray();

                        var dimensionValueName = $"{counter++:D3}_{string.Join("_", currentDimensionValues)}";

                        using (WriteSection($"{dimensionValueName}:"))
                        {
                            for (var i = 0; i < dimensions.Length; i++)
                                WriteLine(
                                    $"{buildDefinition.ParamDefinitions[dimensionNames[i]].Attribute.ArgName}: '{dimensionValues[i][dimensionValueIndices[i]]}'");
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

            var variables = new Dictionary<string, string>();

            var targetsForConsumedVariableDeclaration = new List<TargetModel>();

            foreach (var commandStep in job.Steps.OfType<CommandWorkflowStep>())
            {
                var target = buildModel.Targets.Single(t => t.Name == commandStep.Name);
                targetsForConsumedVariableDeclaration.Add(target);

                if (UseCustomArtifactProvider.IsEnabled(workflow.Options) && !commandStep.SuppressArtifactPublishing)
                {
                    if (target.ConsumedArtifacts.Count > 0)
                        targetsForConsumedVariableDeclaration.Add(buildModel.Targets.Single(t =>
                            t.Name == nameof(IDownloadArtifact.DownloadArtifact)));

                    if (target.ProducedArtifacts.Count > 0)
                        targetsForConsumedVariableDeclaration.Add(buildModel.Targets.Single(t =>
                            t.Name == nameof(IUploadArtifact.UploadArtifact)));
                }
            }

            foreach (var consumedVariable in targetsForConsumedVariableDeclaration.SelectMany(x => x.ConsumedVariables))
            {
                var variableName = buildDefinition.ParamDefinitions[consumedVariable.VariableName].Attribute.ArgName;

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

    private void WriteStep(WorkflowModel workflow, IWorkflowStepModel step, WorkflowJobModel job)
    {
        switch (step)
        {
            case CommandWorkflowStep commandStep:

                using (WriteSection("- checkout: self"))
                    WriteLine("fetchDepth: 0");

                WriteLine();

                var commandStepTarget = buildModel.Targets.Single(t => t.Name == commandStep.Name);

                var matrixParams = job
                    .MatrixDimensions
                    .Select(dimension => buildDefinition.ParamDefinitions[dimension.Name].Attribute.ArgName)
                    .Select(name => (Name: name, Value: $"$({name})"))
                    .ToArray();

                var matrixSlice = (Name: "matrix-slice", Value: string.Join("-", matrixParams.Select(x => x.Value)));

                if (!string.IsNullOrWhiteSpace(matrixSlice.Value))
                    matrixParams = matrixParams
                        .Append(matrixSlice)
                        .ToArray();

                if (commandStepTarget.ConsumedArtifacts.Count > 0)
                {
                    foreach (var consumedArtifact in commandStepTarget.ConsumedArtifacts)
                        if (workflow
                            .Jobs
                            .SelectMany(x => x.Steps)
                            .OfType<CommandWorkflowStep>()
                            .Single(x => x.Name == consumedArtifact.TargetName)
                            .SuppressArtifactPublishing)
                            logger.LogWarning(
                                "Workflow {WorkflowName} command {CommandName} consumes artifact {ArtifactName} from target {SourceTargetName}, which has artifact publishing suppressed; this may cause the workflow to fail",
                                workflow.Name,
                                commandStep.Name,
                                consumedArtifact.ArtifactName,
                                consumedArtifact.TargetName);

                    if (UseCustomArtifactProvider.IsEnabled(workflow.Options))
                    {
                        WriteCommandStep(workflow,
                            new(nameof(IDownloadArtifact.DownloadArtifact)),
                            buildModel.Targets.Single(t => t.Name == nameof(IDownloadArtifact.DownloadArtifact)),
                            [
                                ("atom-artifacts", string.Join(",", commandStepTarget.ConsumedArtifacts.Select(x => x.ArtifactName))),
                                !string.IsNullOrWhiteSpace(matrixSlice.Value)
                                    ? matrixSlice
                                    : default,
                            ],
                            false);

                        WriteLine();
                    }
                    else
                    {
                        foreach (var artifact in commandStepTarget.ConsumedArtifacts)
                        {
                            var name = !string.IsNullOrWhiteSpace(matrixSlice.Value)
                                ? $"{artifact.ArtifactName}-{matrixSlice.Value}"
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

                WriteCommandStep(workflow, commandStep, commandStepTarget, matrixParams, true);

                if (commandStepTarget.ProducedArtifacts.Count > 0 && !commandStep.SuppressArtifactPublishing)
                {
                    if (UseCustomArtifactProvider.IsEnabled(workflow.Options))
                    {
                        WriteLine();

                        WriteCommandStep(workflow,
                            new(nameof(IUploadArtifact.UploadArtifact)),
                            buildModel.Targets.Single(t => t.Name == nameof(IUploadArtifact.UploadArtifact)),
                            [
                                ("atom-artifacts", string.Join(",", commandStepTarget.ProducedArtifacts.Select(x => x.ArtifactName))),
                                !string.IsNullOrWhiteSpace(matrixSlice.Value)
                                    ? matrixSlice
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
                            var name = !string.IsNullOrWhiteSpace(matrixSlice.Value)
                                ? $"{artifact.ArtifactName}-{matrixSlice.Value}"
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

                break;
        }
    }

    private void WriteCommandStep(
        WorkflowModel workflow,
        CommandWorkflowStep commandStep,
        TargetModel target,
        (string name, string value)[] extraParams,
        bool includeName)
    {
        var assemblyName = Assembly.GetEntryAssembly()!.GetName()
            .Name!;

        using (WriteSection($"- script: dotnet run --project {assemblyName}/{assemblyName}.csproj {commandStep.Name} --skip --headless"))
        {
            if (includeName)
                WriteLine($"name: {commandStep.Name}");

            var env = new Dictionary<string, string>();

            foreach (var consumedVariable in target.ConsumedVariables)
                env[buildDefinition.ParamDefinitions[consumedVariable.VariableName].Attribute.ArgName] =
                    $"$({buildDefinition.ParamDefinitions[consumedVariable.VariableName].Attribute.ArgName})";

            var requiredSecrets = target
                .RequiredParams
                .Select(x => buildDefinition.ParamDefinitions[x])
                .Where(x => x.Attribute.IsSecret)
                .Select(x => x)
                .ToArray();

            if (requiredSecrets.Any(x => x.Attribute.IsSecret))
                foreach (var injectedSecret in workflow.Options.OfType<WorkflowVaultSecretInjection>())
                {
                    if (injectedSecret.Value is null)
                    {
                        logger.LogWarning("Workflow {WorkflowName} command {CommandName} has a vault secret injection with a null value",
                            workflow.Name,
                            commandStep.Name);

                        continue;
                    }

                    var paramDefinition = buildDefinition.ParamDefinitions.GetValueOrDefault(injectedSecret.Value);

                    if (paramDefinition is not null)
                        env[paramDefinition.Attribute.ArgName] = $"$({paramDefinition.Attribute.ArgName.ToUpper().Replace('-', '_')})";
                }

            foreach (var requiredSecret in requiredSecrets)
            {
                var injectedSecret = workflow
                    .Options
                    .OfType<WorkflowSecretInjection>()
                    .FirstOrDefault(x => x.Param == requiredSecret.Name);

                if (injectedSecret is not null)
                    env[requiredSecret.Attribute.ArgName] = $"$({requiredSecret.Attribute.ArgName.ToUpper().Replace('-', '_')})";
            }

            var paramInjections = workflow.Options.OfType<WorkflowParamInjection>();

            foreach (var paramInjection in paramInjections)
            {
                if (!buildDefinition.ParamDefinitions.TryGetValue(paramInjection.Name, out var paramDefinition))
                {
                    logger.LogWarning(
                        "Workflow {WorkflowName} command {CommandName} has an injection for parameter {ParamName} that is not consumed by the command",
                        workflow.Name,
                        commandStep.Name,
                        paramInjection.Name);

                    continue;
                }

                env[paramDefinition.Attribute.ArgName] = paramInjection.Value;
            }

            var validEnv = env
                .Where(static x => x.Value is { Length: > 0 })
                .ToList();

            var validExtraParams = extraParams
                .Where(static x => x.value is { Length: > 0 })
                .ToList();

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
