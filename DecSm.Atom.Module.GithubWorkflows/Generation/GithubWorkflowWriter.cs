namespace DecSm.Atom.Module.GithubWorkflows.Generation;

public sealed class GithubWorkflowWriter(
    IAtomFileSystem fileSystem,
    IBuildDefinition buildDefinition,
    BuildModel buildModel,
    ILogger<GithubWorkflowWriter> logger
) : WorkflowFileWriter<GithubWorkflowType>(fileSystem, logger)
{
    private readonly IAtomFileSystem _fileSystem = fileSystem;

    protected override string FileExtension => "yml";

    protected override int TabSize => 2;

    protected override AbsolutePath FileLocation => _fileSystem.AtomRootDirectory / ".github" / "workflows";

    protected override void WriteWorkflow(WorkflowModel workflow)
    {
        WriteLine($"name: {workflow.Name}");
        WriteLine();

        using (WriteSection("on:"))
        {
            var manualTrigger = workflow
                .Triggers
                .OfType<GithubManualTrigger>()
                .FirstOrDefault();

            if (manualTrigger is not null)
                using (WriteSection("workflow_dispatch:"))
                {
                    if (manualTrigger.Inputs?.Count > 0)
                        using (WriteSection("inputs:"))
                        {
                            foreach (var input in manualTrigger.Inputs)
                                using (WriteSection($"{input.Name}:"))
                                {
                                    WriteLine($"description: {input.Description}");
                                    WriteLine($"required: {input.Required}");

                                    switch (input)
                                    {
                                        case GithubManualBoolInput boolInput:

                                            WriteLine("type: bool");

                                            if (boolInput.DefaultValue is not null)
                                                WriteLine($"default: {boolInput.DefaultValue}");

                                            break;

                                        case GithubManualStringInput stringInput:

                                            WriteLine("type: string");

                                            if (stringInput.DefaultValue is not null)
                                                WriteLine($"default: {stringInput.DefaultValue}");

                                            break;

                                        case GithubManualChoiceInput choiceInput:

                                            WriteLine("type: string");

                                            using (WriteSection("options:"))
                                            {
                                                foreach (var choice in choiceInput.Choices)
                                                    WriteLine($"- {choice}");
                                            }

                                            if (choiceInput.DefaultValue is not null)
                                                WriteLine($"default: {choiceInput.DefaultValue}");

                                            break;
                                    }
                                }
                        }
                }

            foreach (var pullRequestTrigger in workflow.Triggers.OfType<GithubPullRequestTrigger>())
                using (WriteSection("pull_request:"))
                {
                    if (pullRequestTrigger.IncludedBranches.Count > 0)
                        using (WriteSection("branches:"))
                        {
                            foreach (var branch in pullRequestTrigger.IncludedBranches)
                                WriteLine($"- '{branch}'");
                        }

                    if (pullRequestTrigger.ExcludedBranches.Count > 0)
                        using (WriteSection("branches-ignore:"))
                        {
                            foreach (var branch in pullRequestTrigger.ExcludedBranches)
                                WriteLine($"- '{branch}'");
                        }

                    if (pullRequestTrigger.IncludedPaths.Count > 0)
                        using (WriteSection("paths:"))
                        {
                            foreach (var path in pullRequestTrigger.IncludedPaths)
                                WriteLine($"- '{path}'");
                        }

                    if (pullRequestTrigger.ExcludedPaths.Count > 0)
                        using (WriteSection("paths-ignore:"))
                        {
                            foreach (var path in pullRequestTrigger.ExcludedPaths)
                                WriteLine($"- '{path}'");
                        }

                    if (pullRequestTrigger.Types.Count > 0)
                        using (WriteSection("types:"))
                        {
                            foreach (var type in pullRequestTrigger.Types)
                                WriteLine($"- '{type}'");
                        }
                }

            foreach (var pushTrigger in workflow.Triggers.OfType<GithubPushTrigger>())
                using (WriteSection("push:"))
                {
                    if (pushTrigger.IncludedBranches.Count > 0)
                        using (WriteSection("branches:"))
                        {
                            foreach (var branch in pushTrigger.IncludedBranches)
                                WriteLine($"- '{branch}'");
                        }

                    if (pushTrigger.ExcludedBranches.Count > 0)
                        using (WriteSection("branches-ignore:"))
                        {
                            foreach (var branch in pushTrigger.ExcludedBranches)
                                WriteLine($"- '{branch}'");
                        }

                    if (pushTrigger.IncludedPaths.Count > 0)
                        using (WriteSection("paths:"))
                        {
                            foreach (var path in pushTrigger.IncludedPaths)
                                WriteLine($"- '{path}'");
                        }

                    if (pushTrigger.ExcludedPaths.Count > 0)
                        using (WriteSection("paths-ignore:"))
                        {
                            foreach (var path in pushTrigger.ExcludedPaths)
                                WriteLine($"- '{path}'");
                        }

                    if (pushTrigger.IncludedTags.Count > 0)
                        using (WriteSection("tags:"))
                        {
                            foreach (var tag in pushTrigger.IncludedTags)
                                WriteLine($"- '{tag}'");
                        }

                    if (pushTrigger.ExcludedTags.Count > 0)
                        using (WriteSection("tags-ignore:"))
                        {
                            foreach (var tag in pushTrigger.ExcludedTags)
                                WriteLine($"- '{tag}'");
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
        using (WriteSection($"{job.Name}:"))
        {
            var jobRequirementNames = job.JobDependencies;

            if (jobRequirementNames.Count > 0)
                WriteLine($"needs: [ {string.Join(", ", jobRequirementNames)} ]");

            if (job.MatrixDimensions.Count > 0)
                using (WriteSection("strategy:"))
                using (WriteSection("matrix:"))
                {
                    foreach (var dimension in job.MatrixDimensions)
                        WriteLine(
                            $"{buildDefinition.ParamDefinitions[dimension.Name].Attribute.ArgName}: [ {string.Join(", ", dimension.Values)} ]");
                }

            var githubPlatformOption = job
                                           .Options
                                           .Concat(workflow.Options)
                                           .OfType<GithubRunsOn>()
                                           .FirstOrDefault() ??
                                       GithubRunsOn.UbuntuLatest;

            var labelsDisplay = githubPlatformOption.Labels.Count is 1
                ? githubPlatformOption.Labels[0]
                : $"[ {string.Join(", ", githubPlatformOption.Labels)} ]";

            if (githubPlatformOption.Group is { Length: > 0 })
                using (WriteSection("runs-on:"))
                {
                    WriteLine($"group: {githubPlatformOption.Group}");
                    WriteLine($"labels: {labelsDisplay}");
                }
            else
                WriteLine($"runs-on: {labelsDisplay}");

            var outputs = new List<string>();

            foreach (var step in job.Steps.OfType<CommandWorkflowStep>())
                outputs.AddRange(buildModel.Targets.Single(t => t.Name == step.Name)
                    .ProducedVariables);

            if (outputs.Count > 0)
                using (WriteSection("outputs:"))
                {
                    foreach (var output in outputs)
                        WriteLine(
                            $"{buildDefinition.ParamDefinitions[output].Attribute.ArgName}: ${{{{ steps.{job.Name}.outputs.{buildDefinition.ParamDefinitions[output].Attribute.ArgName} }}}}");
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

                using (WriteSection("- name: Checkout"))
                {
                    WriteLine("uses: actions/checkout@v4");

                    using (WriteSection("with:"))
                        WriteLine("fetch-depth: 0");
                }

                WriteLine();

                var commandStepTarget = buildModel.Targets.Single(t => t.Name == commandStep.Name);

                var matrixParams = job
                    .MatrixDimensions
                    .Select(dimension => buildDefinition.ParamDefinitions[dimension.Name].Attribute.ArgName)
                    .Select(name => (Name: name, Value: $"${{{{ matrix.{name} }}}}"))
                    .ToArray();

                var matrixSlice = (Name: "matrix-slice", Value: string.Join("-", matrixParams.Select(x => x.Value)));

                if (!string.IsNullOrWhiteSpace(matrixSlice.Value))
                    matrixParams = matrixParams
                        .Append(matrixSlice)
                        .ToArray();

                var allOptions = job.Options.Concat(workflow.Options);

                var setupNugetSteps = allOptions
                    .OfType<AddNugetFeedsStep>()
                    .ToList();

                if (setupNugetSteps.Count > 0)
                {
                    var feedsToAdd = setupNugetSteps
                        .SelectMany(x => x.FeedsToAdd)
                        .DistinctBy(x => x.FeedName)
                        .ToList();

                    using (WriteSection("- name: Setup NuGet"))
                    {
                        using (WriteSection("run: |"))
                        {
                            // TODO: Change to acquire DecSm.Atom.Tool instead of directly calling project, once it's available
                            foreach (var feedToAdd in feedsToAdd)
                                WriteLine(
                                    $"dotnet run --project DecSm.Atom.Tools.Nuget/DecSm.Atom.Tools.Nuget.csproj -- nuget-add {feedToAdd.FeedName};{feedToAdd.FeedUrl}");
                        }

                        WriteLine("shell: bash");

                        using (WriteSection("env:"))
                        {
                            foreach (var feedToAdd in feedsToAdd)
                                WriteLine(
                                    $$$"""{{{AddNugetFeedsStep.EnvVar(feedToAdd.FeedName)}}}: ${{ secrets.{{{feedToAdd.SecretName}}} }}""");
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
                            using (WriteSection($"- name: Download {artifact.ArtifactName}"))
                            {
                                WriteLine("uses: actions/download-artifact@v4");

                                using (WriteSection("with:"))
                                {
                                    WriteLine(!string.IsNullOrWhiteSpace(matrixSlice.Value)
                                        ? $"name: {artifact.ArtifactName}-{matrixSlice.Value}"
                                        : $"name: {artifact.ArtifactName}");

                                    WriteLine($"path: \"{Github.PipelineArtifactDirectory}/{artifact.ArtifactName}\"");
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
                            using (WriteSection($"- name: Upload {artifact.ArtifactName}"))
                            {
                                WriteLine("uses: actions/upload-artifact@v4");

                                using (WriteSection("with:"))
                                {
                                    WriteLine(!string.IsNullOrWhiteSpace(matrixSlice.Value)
                                        ? $"name: {artifact.ArtifactName}-{matrixSlice.Value}"
                                        : $"name: {artifact.ArtifactName}");

                                    WriteLine($"path: \"{Github.PipelinePublishDirectory}/{artifact.ArtifactName}\"");
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
        bool includeId)
    {
        var assemblyName = Assembly.GetEntryAssembly()!.GetName()
            .Name!;

        using (WriteSection($"- name: {commandStep.Name}"))
        {
            if (includeId)
                WriteLine($"id: {commandStep.Name}");

            WriteLine($"run: dotnet run --project {assemblyName}/{assemblyName}.csproj {commandStep.Name} --skip --headless");

            var env = new Dictionary<string, string>();

            foreach (var consumedVariable in target.ConsumedVariables)
                env[buildDefinition.ParamDefinitions[consumedVariable.VariableName].Attribute.ArgName] =
                    $"${{{{ needs.{consumedVariable.TargetName}.outputs.{buildDefinition.ParamDefinitions[consumedVariable.VariableName].Attribute.ArgName} }}}}";

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
                        env[paramDefinition.Attribute.ArgName] =
                            $"${{{{ secrets.{paramDefinition.Attribute.ArgName.ToUpper().Replace('-', '_')} }}}}";
                }

            foreach (var requiredSecret in requiredSecrets)
            {
                var injectedSecret = workflow
                    .Options
                    .OfType<WorkflowSecretInjection>()
                    .FirstOrDefault(x => x.Param == requiredSecret.Name);

                if (injectedSecret is not null)
                    env[requiredSecret.Attribute.ArgName] =
                        $"${{{{ secrets.{requiredSecret.Attribute.ArgName.ToUpper().Replace('-', '_')} }}}}";
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
