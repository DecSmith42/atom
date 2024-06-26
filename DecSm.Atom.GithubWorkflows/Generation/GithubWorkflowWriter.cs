﻿namespace DecSm.Atom.GithubWorkflows.Generation;

public sealed class GithubWorkflowWriter(
    IFileSystem fileSystem,
    IBuildDefinition buildDefinition,
    BuildModel buildModel,
    ILogger<GithubWorkflowWriter> logger
) : WorkflowFileWriter<GithubWorkflowType>(fileSystem, logger)
{
    private readonly IFileSystem _fileSystem = fileSystem;

    protected override string FileExtension => "yml";

    protected override int TabSize => 2;

    protected override AbsolutePath FileLocation => _fileSystem.SolutionRoot() / ".github" / "workflows";

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
                            {
                                WriteLine($"{input.Name}:");
                                WriteLine($"  description: {input.Description}");
                                WriteLine($"  required: {input.Required}");

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
                                WriteLine($"- {branch}");
                        }

                    if (pullRequestTrigger.ExcludedBranches?.Count > 0)
                        using (WriteSection("branches-ignore:"))
                        {
                            foreach (var branch in pullRequestTrigger.ExcludedBranches)
                                WriteLine($"- {branch}");
                        }
                }

            foreach (var pushTrigger in workflow.Triggers.OfType<GithubPushTrigger>())
                using (WriteSection("push:"))
                {
                    if (pushTrigger.IncludedBranches.Count > 0)
                        using (WriteSection("branches:"))
                        {
                            foreach (var branch in pushTrigger.IncludedBranches)
                                WriteLine($"- {branch}");
                        }

                    if (pushTrigger.ExcludedBranches?.Count > 0)
                        using (WriteSection("branches-ignore:"))
                        {
                            foreach (var branch in pushTrigger.ExcludedBranches)
                                WriteLine($"- {branch}");
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
            if (job.JobDependencies is { Count: > 0 })
            {
                var jobRequirementNames = job.JobDependencies;
                WriteLine($"needs: [ {string.Join(", ", jobRequirementNames)} ]");
            }

            WriteLine("runs-on: ubuntu-latest");

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
                    WriteStep(workflow, job, step);
                }
            }
        }
    }

    private void WriteStep(WorkflowModel workflow, WorkflowJobModel job, IWorkflowStepModel step)
    {
        switch (step)
        {
            case CommandWorkflowStep commandStep:

                using (WriteSection("- name: Checkout"))
                    WriteLine("uses: actions/checkout@v2");

                WriteLine();

                var target = buildModel.Targets.Single(t => t.Name == commandStep.Name);

                if (target.ConsumedArtifacts.Count > 0)
                {
                    if (workflow
                        .Options
                        .OfType<UseCustomArtifactProvider>()
                        .Any())
                    {
                        WriteCommandStep(workflow,
                            new(nameof(IDownloadArtifact.DownloadArtifact)),
                            buildModel.Targets.Single(t => t.Name == nameof(IDownloadArtifact.DownloadArtifact)),
                            [("atom-artifacts", string.Join(";", target.ConsumedArtifacts.Select(x => x.ArtifactName)))],
                            false);

                        WriteLine();
                    }
                    else
                    {
                        foreach (var artifact in target.ConsumedArtifacts)
                        {
                            using (WriteSection($"- name: Download {artifact.ArtifactName}"))
                            {
                                WriteLine("uses: actions/download-artifact@v4");

                                using (WriteSection("with:"))
                                {
                                    WriteLine($"name: {artifact.ArtifactName}");
                                    WriteLine($"path: \"{Github.PipelineArtifactDirectory}/{artifact.ArtifactName}\"");
                                }
                            }

                            WriteLine();
                        }
                    }
                }

                WriteCommandStep(workflow, commandStep, target, [], true);

                if (target.ProducedArtifacts.Count > 0)
                {
                    if (workflow
                        .Options
                        .OfType<UseCustomArtifactProvider>()
                        .Any())
                    {
                        WriteLine();

                        WriteCommandStep(workflow,
                            new(nameof(IUploadArtifact.UploadArtifact)),
                            buildModel.Targets.Single(t => t.Name == nameof(IUploadArtifact.UploadArtifact)),
                            [("atom-artifacts", string.Join(";", target.ProducedArtifacts.Select(x => x.ArtifactName)))],
                            false);
                    }
                    else
                    {
                        foreach (var artifact in target.ProducedArtifacts)
                        {
                            using (WriteSection($"- name: Upload {artifact.ArtifactName}"))
                            {
                                WriteLine("uses: actions/upload-artifact@v4");

                                using (WriteSection("with:"))
                                {
                                    WriteLine($"name: {artifact.ArtifactName}");
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
                    var paramDefinition = buildDefinition.ParamDefinitions.GetValueOrDefault(injectedSecret.Param);

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

            var paramInjections = workflow
                .Options
                .OfType<WorkflowParamInjection>()
                .Where(x => x.Command is null);

            // Later injections override earlier ones, we want the command specific ones to override the global ones
            paramInjections = paramInjections.Concat(workflow
                .Options
                .OfType<WorkflowParamInjection>()
                .Where(x => x.Command?.Name == commandStep.Name));

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

            if (env.Count > 0)
                using (WriteSection("env:"))
                {
                    foreach (var (key, value) in env)
                        WriteLine($"{key}: {value}");

                    foreach (var (key, value) in extraParams)
                        WriteLine($"{key}: {value}");
                }
        }
    }
}