namespace DecSm.Atom.Module.GithubWorkflows.Generation;

internal sealed class GithubWorkflowWriter(
    IAtomFileSystem fileSystem,
    IBuildDefinition buildDefinition,
    BuildModel buildModel,
    ILogger<GithubWorkflowWriter> logger
) : WorkflowFileWriter<GithubWorkflowType>(fileSystem, logger)
{
    private readonly IAtomFileSystem _fileSystem = fileSystem;

    protected override string FileExtension => "yml";

    protected override int TabSize => 2;

    protected override RootedPath FileLocation => _fileSystem.AtomRootDirectory / ".github" / "workflows";

    protected override void WriteWorkflow(WorkflowModel workflow)
    {
        WriteLine($"name: {workflow.Name}");
        WriteLine();

        using (WriteSection("on:"))
        {
            var manualTrigger = workflow
                .Triggers
                .OfType<ManualTrigger>()
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
                                    WriteLine($"required: {input.Required.ToString().ToLower()}");

                                    switch (input)
                                    {
                                        case ManualBoolInput boolInput:

                                            WriteLine("type: boolean");

                                            if (boolInput.DefaultValue is not null)
                                                WriteLine($"default: {boolInput.DefaultValue.ToString()?.ToLower()}");

                                            break;

                                        case ManualStringInput stringInput:

                                            WriteLine("type: string");

                                            if (stringInput.DefaultValue is not null)
                                                WriteLine($"default: {stringInput.DefaultValue}");

                                            break;

                                        case ManualChoiceInput choiceInput:

                                            WriteLine("type: choice");

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

            foreach (var pullRequestTrigger in workflow.Triggers.OfType<GitPullRequestTrigger>())
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

            foreach (var pushTrigger in workflow.Triggers.OfType<GitPushTrigger>())
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
                        WriteLine($"{buildDefinition.ParamDefinitions[dimension.Name].ArgName}: [ {string.Join(", ", dimension.Values)} ]");
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

            foreach (var step in job.Steps.OfType<WorkflowCommandModel>())
                outputs.AddRange(buildModel.GetTarget(step.Name)
                    .ProducedVariables);

            if (outputs.Count > 0)
                using (WriteSection("outputs:"))
                {
                    foreach (var output in outputs)
                        WriteLine(
                            $"{buildDefinition.ParamDefinitions[output].ArgName}: ${{{{ steps.{job.Name}.outputs.{buildDefinition.ParamDefinitions[output].ArgName} }}}}");
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

    private void WriteStep(WorkflowModel workflow, IWorkflowTargetModel step, WorkflowJobModel job)
    {
        switch (step)
        {
            case WorkflowCommandModel commandStep:

                using (WriteSection("- name: Checkout"))
                {
                    WriteLine("uses: actions/checkout@v4");

                    using (WriteSection("with:"))
                        WriteLine("fetch-depth: 0");
                }

                var commandStepTarget = buildModel.GetTarget(commandStep.Name);

                var matrixParams = job
                    .MatrixDimensions
                    .Select(dimension => buildDefinition.ParamDefinitions[dimension.Name].ArgName)
                    .Select(name => (Name: name, Value: $"${{{{ matrix.{name} }}}}"))
                    .ToArray();

                var buildSlice = (Name: "build-slice", Value: string.Join("-", matrixParams.Select(x => x.Value)));

                if (!string.IsNullOrWhiteSpace(buildSlice.Value))
                    matrixParams = matrixParams
                        .Append(buildSlice)
                        .ToArray();

                var setupNugetSteps = workflow
                    .Options
                    .Concat(commandStep.Options)
                    .OfType<AddNugetFeedsStep>()
                    .ToList();

                if (setupNugetSteps.Count > 0)
                {
                    var feedsToAdd = setupNugetSteps
                        .SelectMany(x => x.FeedsToAdd)
                        .DistinctBy(x => x.FeedName)
                        .ToList();

                    // TODO: Remove preview flag once v1.0.0 is released
                    using (WriteSection("- name: Install atom tool"))
                    {
                        WriteLine("run: dotnet tool update --global DecSm.Atom.Tool --prerelease");
                        WriteLine("shell: bash");
                    }

                    WriteLine();

                    using (WriteSection("- name: Setup NuGet"))
                    {
                        using (WriteSection("run: |"))
                            foreach (var feedToAdd in feedsToAdd)
                                WriteLine($"  atom nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\"");

                        WriteLine("shell: bash");

                        using (WriteSection("env:"))
                        {
                            foreach (var feedToAdd in feedsToAdd)
                                WriteLine(
                                    $$$"""{{{AddNugetFeedsStep.GetEnvVarNameForFeed(feedToAdd.FeedName)}}}: ${{ secrets.{{{feedToAdd.SecretName}}} }}""");
                        }
                    }
                }

                if (commandStepTarget.ConsumedArtifacts.Count > 0)
                {
                    foreach (var consumedArtifact in commandStepTarget.ConsumedArtifacts)
                        if (workflow
                            .Jobs
                            .SelectMany(x => x.Steps)
                            .OfType<WorkflowCommandModel>()
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
                        foreach (var slice in commandStepTarget.ConsumedArtifacts.GroupBy(a => a.BuildSlice))
                        {
                            WriteLine();

                            WriteCommandStep(workflow,
                                new(nameof(IRetrieveArtifact.RetrieveArtifact)),
                                buildModel.GetTarget(nameof(IRetrieveArtifact.RetrieveArtifact)),
                                [
                                    ("atom-artifacts", string.Join(",",
                                        slice
                                            .AsEnumerable()
                                            .Select(x => x.ArtifactName))),
                                    slice.Key is { Length: > 0 }
                                        ? (Name: "build-slice", Value: slice.Key)
                                        : !string.IsNullOrWhiteSpace(buildSlice.Value)
                                            ? buildSlice
                                            : default,
                                ],
                                false);
                        }
                    }
                    else
                    {
                        foreach (var artifact in commandStepTarget.ConsumedArtifacts)
                        {
                            WriteLine();

                            using (WriteSection($"- name: Download {artifact.ArtifactName}"))
                            {
                                WriteLine("uses: actions/download-artifact@v4");

                                using (WriteSection("with:"))
                                {
                                    WriteLine(artifact.BuildSlice is { Length: > 0 }
                                        ? $"name: {artifact.ArtifactName}-{artifact.BuildSlice}"
                                        : !string.IsNullOrWhiteSpace(buildSlice.Value)
                                            ? $"name: {artifact.ArtifactName}-{buildSlice.Value}"
                                            : $"name: {artifact.ArtifactName}");

                                    WriteLine($"path: \"{Github.PipelineArtifactDirectory}/{artifact.ArtifactName}\"");
                                }
                            }
                        }
                    }
                }

                WriteLine();
                WriteCommandStep(workflow, commandStep, commandStepTarget, matrixParams, true);

                if (commandStepTarget.ProducedArtifacts.Count > 0 && !commandStep.SuppressArtifactPublishing)
                {
                    if (UseCustomArtifactProvider.IsEnabled(workflow.Options))
                    {
                        foreach (var slice in commandStepTarget.ProducedArtifacts.GroupBy(a => a.BuildSlice))
                        {
                            WriteLine();

                            WriteCommandStep(workflow,
                                new(nameof(IStoreArtifact.StoreArtifact)),
                                buildModel.GetTarget(nameof(IStoreArtifact.StoreArtifact)),
                                [
                                    ("atom-artifacts", string.Join(",",
                                        slice
                                            .AsEnumerable()
                                            .Select(x => x.ArtifactName))),
                                    slice.Key is { Length: > 0 }
                                        ? (Name: "build-slice", Value: slice.Key)
                                        : !string.IsNullOrWhiteSpace(buildSlice.Value)
                                            ? buildSlice
                                            : default,
                                ],
                                false);
                        }
                    }
                    else
                    {
                        foreach (var artifact in commandStepTarget.ProducedArtifacts)
                        {
                            WriteLine();

                            using (WriteSection($"- name: Upload {artifact.ArtifactName}"))
                            {
                                WriteLine("uses: actions/upload-artifact@v4");

                                using (WriteSection("with:"))
                                {
                                    WriteLine(artifact.BuildSlice is { Length: > 0 }
                                        ? $"name: {artifact.ArtifactName}-{artifact.BuildSlice}"
                                        : !string.IsNullOrWhiteSpace(buildSlice.Value)
                                            ? $"name: {artifact.ArtifactName}-{buildSlice.Value}"
                                            : $"name: {artifact.ArtifactName}");

                                    WriteLine($"path: \"{Github.PipelinePublishDirectory}/{artifact.ArtifactName}\"");
                                }
                            }
                        }
                    }
                }

                break;
        }
    }

    private void WriteCommandStep(
        WorkflowModel workflow,
        WorkflowCommandModel workflowCommandStep,
        TargetModel target,
        (string name, string value)[] extraParams,
        bool includeId)
    {
        var projectName = _fileSystem.ProjectName;

        using (WriteSection($"- name: {workflowCommandStep.Name}"))
        {
            if (includeId)
                WriteLine($"id: {workflowCommandStep.Name}");

            WriteLine($"run: dotnet run --project {projectName}/{projectName}.csproj {workflowCommandStep.Name} --skip --headless");

            var env = new Dictionary<string, string>();

            foreach (var githubManualTrigger in workflow.Triggers.OfType<ManualTrigger>())
            {
                if (githubManualTrigger.Inputs is null or [])
                    continue;

                foreach (var input in githubManualTrigger.Inputs.Where(i => target
                             .RequiredParams
                             .Select(p => p.ArgName)
                             .Any(p => p == i.Name)))
                    env[input.Name] = $"${{{{ inputs.{input.Name} }}}}";
            }

            foreach (var consumedVariable in target.ConsumedVariables)
                env[buildDefinition.ParamDefinitions[consumedVariable.VariableName].ArgName] =
                    $"${{{{ needs.{consumedVariable.TargetName}.outputs.{buildDefinition.ParamDefinitions[consumedVariable.VariableName].ArgName} }}}}";

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
                            workflowCommandStep.Name);

                        continue;
                    }

                    var paramDefinition = buildDefinition.ParamDefinitions.GetValueOrDefault(injectedSecret.Value);

                    if (paramDefinition is not null)
                        env[paramDefinition.ArgName] = $"${{{{ secrets.{paramDefinition.ArgName.ToUpper().Replace('-', '_')} }}}}";
                }

                foreach (var injectedEvVar in workflow.Options.OfType<WorkflowSecretsEnvironmentInjection>())
                {
                    if (injectedEvVar.Value is null)
                    {
                        logger.LogWarning(
                            "Workflow {WorkflowName} command {CommandName} has a secret provider environment variable injection with a null value",
                            workflow.Name,
                            workflowCommandStep.Name);

                        continue;
                    }

                    var paramDefinition = buildDefinition.ParamDefinitions.GetValueOrDefault(injectedEvVar.Value);

                    if (paramDefinition is not null)
                        env[paramDefinition.ArgName] = $"${{{{ vars.{paramDefinition.ArgName.ToUpper().Replace('-', '_')} }}}}";
                }
            }

            foreach (var requiredSecret in requiredSecrets)
            {
                var injectedSecret = workflow
                    .Options
                    .Concat(workflowCommandStep.Options)
                    .OfType<WorkflowSecretInjection>()
                    .FirstOrDefault(x => x.Value == requiredSecret.Name);

                if (injectedSecret is not null)
                    env[requiredSecret.ArgName] = $"${{{{ secrets.{requiredSecret.ArgName.ToUpper().Replace('-', '_')} }}}}";
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
                        workflowCommandStep.Name,
                        environmentInjection.Value);

                    continue;
                }

                env[paramDefinition.ArgName] = $"${{{{ vars.{paramDefinition.ArgName.ToUpper().Replace('-', '_')} }}}}";
            }

            foreach (var paramInjection in paramInjections)
            {
                if (!buildDefinition.ParamDefinitions.TryGetValue(paramInjection.Name, out var paramDefinition))
                {
                    logger.LogWarning(
                        "Workflow {WorkflowName} command {CommandName} has an injection for parameter {ParamName} that is not consumed by the command",
                        workflow.Name,
                        workflowCommandStep.Name,
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
