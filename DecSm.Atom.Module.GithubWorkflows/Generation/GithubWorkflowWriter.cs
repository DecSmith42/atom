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

                                    var inputParamName = buildDefinition.ParamDefinitions
                                        .FirstOrDefault(x => x.Value.ArgName == input.Name)
                                        .Key;

                                    if (inputParamName is null)
                                        throw new InvalidOperationException(
                                            $"Workflow {workflow.Name} has a manual trigger input named {input.Name} that does not correspond to any parameter in the build definition");

                                    switch (input)
                                    {
                                        case ManualBoolInput boolInput:

                                            bool? defaultBoolValue = null;

                                            if (boolInput.DefaultValue.HasValue)
                                            {
                                                defaultBoolValue = boolInput.DefaultValue.Value;
                                            }
                                            else
                                            {
                                                var accessedParam = buildDefinition.AccessParam(inputParamName);

                                                switch (accessedParam)
                                                {
                                                    case bool boolParam:
                                                        defaultBoolValue = boolParam;

                                                        break;
                                                    case string stringParam:
                                                    {
                                                        if (bool.TryParse(stringParam, out var parsedBool))
                                                            defaultBoolValue = parsedBool;

                                                        break;
                                                    }
                                                }
                                            }

                                            var isBoolRequired = input.Required ?? defaultBoolValue is null
                                                ? "true"
                                                : "false";

                                            WriteLine($"required: {isBoolRequired}");

                                            WriteLine("type: boolean");

                                            if (defaultBoolValue.HasValue)
                                                WriteLine($"default: {(defaultBoolValue.Value ? "true" : "false")}");

                                            break;

                                        case ManualStringInput stringInput:

                                            var defaultStringValue = stringInput.DefaultValue is { Length: > 0 }
                                                ? stringInput.DefaultValue
                                                : buildDefinition
                                                    .AccessParam(inputParamName)
                                                    ?.ToString();

                                            var isStringRequired =
                                                input.Required ?? defaultStringValue is not { Length: > 0 }
                                                    ? "true"
                                                    : "false";

                                            WriteLine($"required: {isStringRequired}");

                                            WriteLine("type: string");

                                            if (defaultStringValue is not null)
                                                WriteLine($"default: {defaultStringValue}");

                                            break;

                                        case ManualChoiceInput choiceInput:

                                            var defaultChoiceValue = choiceInput.DefaultValue is { Length: > 0 }
                                                ? choiceInput.DefaultValue
                                                : buildDefinition
                                                    .AccessParam(inputParamName)
                                                    ?.ToString();

                                            var isChoiceRequired =
                                                input.Required ?? defaultChoiceValue is not { Length: > 0 }
                                                    ? "true"
                                                    : "false";

                                            WriteLine($"required: {isChoiceRequired}");

                                            WriteLine("type: choice");

                                            using (WriteSection("options:"))
                                            {
                                                foreach (var choice in choiceInput.Choices)
                                                    WriteLine($"- {choice}");
                                            }

                                            if (defaultChoiceValue is not null)
                                                WriteLine($"default: {defaultChoiceValue}");

                                            break;
                                    }
                                }
                        }
                }

            var releaseTriggers = workflow
                .Triggers
                .OfType<GithubReleaseTrigger>()
                .ToList();

            if (releaseTriggers.Count > 0)
            {
                using (WriteSection("release:"))
                    WriteLine($"types: [ {string.Join(", ", releaseTriggers.SelectMany(x => x.Types).Distinct())} ]");

                WriteLine();
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

                    // ReSharper disable once InvertIf
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

                    // ReSharper disable once InvertIf
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
                            $"{buildDefinition.ParamDefinitions[dimension.Name].ArgName}: [ {string.Join(", ", dimension.Values)} ]");
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

            var environmentOptions = job
                .Options
                .Concat(workflow.Options)
                .OfType<DeployToEnvironment>()
                .ToList();

            foreach (var environmentOption in environmentOptions)
                WriteLine($"environment: {environmentOption.Value}");

            var githubIfOptions = job
                .Options
                .Concat(workflow.Options)
                .OfType<GithubIf>()
                .ToList();

            foreach (var githubIfOption in githubIfOptions)
                WriteLine($"if: {githubIfOption.Value}");

            var outputs = new List<string>();

            foreach (var step in job.Steps)
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

    private void WriteStep(WorkflowModel workflow, WorkflowStepModel step, WorkflowJobModel job)
    {
        if (workflow
                .Options
                .Concat(step.Options)
                .OfType<GithubCheckoutOption>()
                .FirstOrDefault() is { } checkoutOption)
            using (WriteSection("- name: Checkout"))
            {
                WriteLine($"uses: actions/checkout@{checkoutOption.Value!.Version}");

                using (WriteSection("with:"))
                {
                    WriteLine("fetch-depth: 0");

                    if (checkoutOption.Value.Lfs)
                        WriteLine("lfs: true");

                    if (!string.IsNullOrWhiteSpace(checkoutOption.Value.Submodules))
                        WriteLine($"submodules: {checkoutOption.Value.Submodules}");
                }
            }
        else
            using (WriteSection("- name: Checkout"))
            {
                WriteLine("uses: actions/checkout@v4");

                using (WriteSection("with:"))
                    WriteLine("fetch-depth: 0");
            }

        var commandStepTarget = buildModel.GetTarget(step.Name);

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

        var setupDotnetSteps = workflow
            .Options
            .Concat(step.Options)
            .OfType<SetupDotnetStep>()
            .ToList();

        if (setupDotnetSteps.Count > 0)
            foreach (var setupDotnetStep in setupDotnetSteps)
                using (WriteSection("- uses: actions/setup-dotnet@v4"))
                {
                    if (setupDotnetStep.DotnetVersion is not { Length: > 0 })
                        continue;

                    using (WriteSection("with:"))
                    {
                        WriteLine($"dotnet-version: '{setupDotnetStep.DotnetVersion}'");

                        if (setupDotnetStep.Quality is not null)
                            WriteLine($"dotnet-quality: '{setupDotnetStep.Quality.ToString()!.ToLower()}'");
                    }
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

            using (WriteSection("- name: Install atom tool"))
            {
                WriteLine("run: dotnet tool update --global DecSm.Atom.Tool");
                WriteLine("shell: bash");
            }

            WriteLine();

            using (WriteSection("- name: Setup NuGet"))
            {
                using (WriteSection("run: |"))
                {
                    foreach (var feedToAdd in feedsToAdd)
                        WriteLine($"  atom nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\"");
                }

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
                    .Single(x => x.Name == consumedArtifact.TargetName)
                    .SuppressArtifactPublishing)
                    logger.LogWarning(
                        "Workflow {WorkflowName} command {CommandName} consumes artifact {ArtifactName} from target {SourceTargetName}, which has artifact publishing suppressed; this may cause the workflow to fail",
                        workflow.Name,
                        step.Name,
                        consumedArtifact.ArtifactName,
                        consumedArtifact.TargetName);

            if (UseCustomArtifactProvider.IsEnabled(workflow.Options))
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
            else
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

        WriteLine();
        WriteCommandStep(workflow, step, commandStepTarget, matrixParams, true);

        // ReSharper disable once InvertIf
        if (commandStepTarget.ProducedArtifacts.Count > 0 && !step.SuppressArtifactPublishing)
        {
            if (UseCustomArtifactProvider.IsEnabled(workflow.Options))
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
            else
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

    private void WriteCommandStep(
        WorkflowModel workflow,
        WorkflowStepModel workflowStep,
        TargetModel target,
        (string name, string value)[] extraParams,
        bool includeId)
    {
        var projectName = _fileSystem.ProjectName;

        using (WriteSection($"- name: {workflowStep.Name}"))
        {
            if (includeId)
                WriteLine($"id: {workflowStep.Name}");

            WriteLine(
                $"run: dotnet run --project {projectName}/{projectName}.csproj {workflowStep.Name} --skip --headless");

            var env = new Dictionary<string, string>();

            foreach (var githubManualTrigger in workflow.Triggers.OfType<ManualTrigger>())
            {
                if (githubManualTrigger.Inputs is null or [])
                    continue;

                foreach (var input in githubManualTrigger.Inputs.Where(i => target
                             .Params
                             .Select(p => p.Param.ArgName)
                             .Any(p => p == i.Name)))
                    env[input.Name] = $"${{{{ inputs.{input.Name} }}}}";
            }

            foreach (var consumedVariable in target.ConsumedVariables)
                env[buildDefinition.ParamDefinitions[consumedVariable.VariableName].ArgName] =
                    $"${{{{ needs.{consumedVariable.TargetName}.outputs.{buildDefinition.ParamDefinitions[consumedVariable.VariableName].ArgName} }}}}";

            var requiredSecrets = target
                .Params
                .Where(x => x.Param.IsSecret)
                .Select(x => x)
                .ToArray();

            if (requiredSecrets.Any(x => x.Param.IsSecret))
            {
                foreach (var injectedSecret in workflow.Options.OfType<WorkflowSecretsSecretInjection>())
                {
                    if (injectedSecret.Value is null)
                    {
                        logger.LogWarning(
                            "Workflow {WorkflowName} command {CommandName} has a secret injection with a null value",
                            workflow.Name,
                            workflowStep.Name);

                        continue;
                    }

                    var paramDefinition = buildDefinition.ParamDefinitions.GetValueOrDefault(injectedSecret.Value);

                    if (paramDefinition is not null)
                        env[paramDefinition.ArgName] =
                            $"${{{{ secrets.{paramDefinition.ArgName.ToUpper().Replace('-', '_')} }}}}";
                }

                foreach (var injectedEvVar in workflow.Options.OfType<WorkflowSecretsEnvironmentInjection>())
                {
                    if (injectedEvVar.Value is null)
                    {
                        logger.LogWarning(
                            "Workflow {WorkflowName} command {CommandName} has a secret provider environment variable injection with a null value",
                            workflow.Name,
                            workflowStep.Name);

                        continue;
                    }

                    var paramDefinition = buildDefinition.ParamDefinitions.GetValueOrDefault(injectedEvVar.Value);

                    if (paramDefinition is not null)
                        env[paramDefinition.ArgName] =
                            $"${{{{ vars.{paramDefinition.ArgName.ToUpper().Replace('-', '_')} }}}}";
                }
            }

            foreach (var requiredSecret in requiredSecrets)
            {
                var injectedSecret = workflow
                    .Options
                    .Concat(workflowStep.Options)
                    .OfType<WorkflowSecretInjection>()
                    .FirstOrDefault(x => x.Value == requiredSecret.Param.Name);

                if (injectedSecret is not null)
                    env[requiredSecret.Param.ArgName] =
                        $"${{{{ secrets.{requiredSecret.Param.ArgName.ToUpper().Replace('-', '_')} }}}}";
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

                env[paramDefinition.ArgName] = $"${{{{ vars.{paramDefinition.ArgName.ToUpper().Replace('-', '_')} }}}}";
            }

            foreach (var paramInjection in paramInjections)
            {
                if (!buildDefinition.ParamDefinitions.TryGetValue(paramInjection.Name, out var paramDefinition))
                {
                    logger.LogWarning(
                        "Workflow {WorkflowName} command {CommandName} has an injection for parameter {ParamName} that is not consumed by the command",
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
