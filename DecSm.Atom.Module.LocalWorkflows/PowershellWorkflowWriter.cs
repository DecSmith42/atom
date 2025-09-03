using DecSm.Atom.Artifacts;
using DecSm.Atom.Nuget;
using DecSm.Atom.Workflows.Definition.Options;
using DecSm.Atom.Workflows.Definition.Triggers;

namespace DecSm.Atom.Module.LocalWorkflows;

internal sealed class PowershellWorkflowWriter(
    IAtomFileSystem fileSystem,
    IBuildDefinition buildDefinition,
    BuildModel buildModel,
    ILogger<PowershellWorkflowWriter> logger
) : WorkflowFileWriter<PowershellWorkflowType>(fileSystem, logger)
{
    private readonly IAtomFileSystem _fileSystem = fileSystem;

    protected override string FileExtension => "ps1";

    protected override int TabSize => 2;

    protected override RootedPath FileLocation => _fileSystem.AtomRootDirectory / ".atom" / "workflows";

    protected override void WriteWorkflow(WorkflowModel workflow)
    {
        WriteLine($"# name: {workflow.Name}");
        WriteLine();

        var manualTrigger = workflow
            .Triggers
            .OfType<ManualTrigger>()
            .FirstOrDefault();

        if (manualTrigger is { Inputs.Count: > 0 })
        {
            WriteLine("param(");
            Indent();

            for (var i = 0; i < manualTrigger.Inputs.Count; i++)
            {
                var input = manualTrigger.Inputs[i];

                var inputPascalName = buildDefinition.ParamDefinitions.First(x => x.Value.ArgName == input.Name)
                    .Key;

                var type = input switch
                {
                    ManualBoolInput => "[bool]",
                    _ => "[string]",
                };

                var defaultValue = input switch
                {
                    ManualBoolInput boolInput => boolInput.DefaultValue is not null
                        ? $" = ${boolInput.DefaultValue.Value.ToString().ToLower()}"
                        : "",
                    ManualChoiceInput choiceInput => choiceInput.DefaultValue is not null
                        ? $" = '{choiceInput.DefaultValue}'"
                        : $" = '{choiceInput.Choices[0]}'",
                    ManualStringInput stringInput => stringInput.DefaultValue is not null
                        ? $" = '{stringInput.DefaultValue}'"
                        : "",
                    _ => "",
                };

                var suffix = i < manualTrigger.Inputs.Count - 1
                    ? ","
                    : "";

                WriteLine($"{type}${inputPascalName}{defaultValue}{suffix}");
            }

            Unindent();
            WriteLine(")");
            WriteLine();

            // Prompt for missing parameters
            foreach (var input in manualTrigger.Inputs)
            {
                var inputPascalName = buildDefinition.ParamDefinitions.First(x => x.Value.ArgName == input.Name)
                    .Key;

                WriteLine($"if (-not ${inputPascalName}) {{");
                Indent();
                WriteLine($"${inputPascalName} = Read-Host 'Enter value for {inputPascalName}'");
                Unindent();
                WriteLine("}");
            }
        }

        WriteLine("$atom_variables = @{}");

        foreach (var job in workflow.Jobs)
        {
            WriteLine();
            WriteJob(workflow, job);
        }
    }

    private void WriteJob(WorkflowModel workflow, WorkflowJobModel job)
    {
        if (job.MatrixDimensions.Count > 0)
        {
            foreach (var dimension in job.MatrixDimensions)
            {
                var values = string.Join(", ", dimension.Values.Select(v => $"'{v}'"));
                var argName = buildDefinition.ParamDefinitions[dimension.Name].ArgName;
                WriteLine($"$matrix_{argName} = @({values})");
                WriteLine($"foreach (${argName} in $matrix_{argName})");
                WriteLine("{");
                Indent();
            }

            foreach (var step in job.Steps)
            {
                WriteLine();
                WriteStep(workflow, step, job);
            }

            for (var i = 0; i < job.MatrixDimensions.Count; i++)
            {
                Unindent();
                WriteLine("}");
            }
        }
        else
        {
            foreach (var step in job.Steps)
            {
                WriteLine();
                WriteStep(workflow, step, job);
            }
        }
    }

    private void WriteStep(WorkflowModel workflow, WorkflowStepModel step, WorkflowJobModel job)
    {
        var commandStepTarget = buildModel.GetTarget(step.Name);

        var setupDotnetSteps = workflow
            .Options
            .Concat(step.Options)
            .OfType<SetupDotnetStep>()
            .ToList();

        if (setupDotnetSteps.Count > 0)
            logger.LogWarning("SetupDotnetStep is not supported in PowerShell workflows and will be ignored");

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

            WriteLine("# Setup NuGet feeds");
            WriteLine("dotnet tool update --global DecSm.Atom.Tool");

            foreach (var feedToAdd in feedsToAdd)
            {
                var envVarName = AddNugetFeedsStep.GetEnvVarNameForFeed(feedToAdd.FeedName);
                var line = $"if ($env:{feedToAdd.SecretName}) {{ $env:{envVarName} = $env:{feedToAdd.SecretName} }}";
                WriteLine(line);
                WriteLine($"atom nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\"");
            }

            WriteLine();
        }

        if (commandStepTarget.ConsumedArtifacts.Count > 0)
        {
            if (UseCustomArtifactProvider.IsEnabled(workflow.Options))
            {
                WriteCommandStep(workflow,
                    new(nameof(IRetrieveArtifact.RetrieveArtifact)),
                    buildModel.GetTarget(nameof(IRetrieveArtifact.RetrieveArtifact)),
                    [("atom-artifacts", string.Join(",", commandStepTarget.ConsumedArtifacts.Select(x => x.ArtifactName)))],
                    false);

                WriteLine();
            }
            else
            {
                logger.LogWarning(
                    "Workflow {WorkflowName} command {CommandName} consumes artifacts but no custom artifact provider is configured; artifacts will only be available locally",
                    workflow.Name,
                    step.Name);
            }
        }

        var matrixParams = job
            .MatrixDimensions
            .Select(dimension => buildDefinition.ParamDefinitions[dimension.Name].ArgName)
            .Select(name => (Name: name, Value: $"${name}"))
            .ToArray();

        WriteCommandStep(workflow, step, commandStepTarget, matrixParams, true);

        // ReSharper disable once InvertIf
        if (commandStepTarget.ProducedArtifacts.Count > 0 && !step.SuppressArtifactPublishing)
        {
            WriteLine();

            if (UseCustomArtifactProvider.IsEnabled(workflow.Options))
                WriteCommandStep(workflow,
                    new(nameof(IStoreArtifact.StoreArtifact)),
                    buildModel.GetTarget(nameof(IStoreArtifact.StoreArtifact)),
                    [("atom-artifacts", string.Join(",", commandStepTarget.ProducedArtifacts.Select(x => x.ArtifactName)))],
                    false);
            else
                logger.LogWarning(
                    "Workflow {WorkflowName} command {CommandName} produces artifacts but no custom artifact provider is configured; artifacts will only be available locally",
                    workflow.Name,
                    step.Name);
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
        var args = new List<string>();

        var manualTrigger = workflow
            .Triggers
            .OfType<ManualTrigger>()
            .FirstOrDefault();

        if (manualTrigger?.Inputs is not null)
            foreach (var input in manualTrigger.Inputs.Where(i => target
                         .RequiredParams
                         .Select(p => p.ArgName)
                         .Any(p => p == i.Name)))
            {
                var inputPascalName = buildDefinition.ParamDefinitions.First(x => x.Value.ArgName == input.Name)
                    .Key;

                args.Add($"--{input.Name} ${inputPascalName}");
            }

        foreach (var (name, value) in extraParams)
            args.Add($"--{name} {value}");

        foreach (var consumedVariable in target.ConsumedVariables)
        {
            var argName = buildDefinition.ParamDefinitions[consumedVariable.VariableName].ArgName;

            // Use environment variable for variable passing (set by AtomWorkflowVariableProvider)
            args.Add($"--{argName} $env:{argName}");
        }

        var injections = new Dictionary<string, string>();
        var environmentInjections = workflow.Options.OfType<WorkflowEnvironmentInjection>();
        var paramInjections = workflow.Options.OfType<WorkflowParamInjection>();
        environmentInjections = environmentInjections.Where(e => paramInjections.All(p => p.Name != e.Value));

        foreach (var environmentInjection in environmentInjections.Where(e => e.Value is not null))
        {
            if (!buildDefinition.ParamDefinitions.TryGetValue(environmentInjection.Value!, out var paramDefinition))
                continue;

            var envVarName = paramDefinition
                .ArgName
                .ToUpper()
                .Replace('-', '_');

            injections[paramDefinition.ArgName] = $"$env:{envVarName}";
        }

        foreach (var paramInjection in paramInjections)
        {
            if (!buildDefinition.ParamDefinitions.TryGetValue(paramInjection.Name, out var paramDefinition))
                continue;

            injections[paramDefinition.ArgName] = $"'{paramInjection.Value}'";
        }

        var requiredSecrets = target
            .RequiredParams
            .Where(x => x.IsSecret)
            .ToArray();

        foreach (var requiredSecret in requiredSecrets)
        {
            var injectedSecret = workflow
                .Options
                .Concat(workflowStep.Options)
                .OfType<WorkflowSecretInjection>()
                .FirstOrDefault(x => x.Value == requiredSecret.Name);

            if (injectedSecret is not null)
                injections[requiredSecret.ArgName] = $"$env:{requiredSecret.ArgName.ToUpper().Replace('-', '_')}";
        }

        foreach (var (key, value) in injections)
            args.Add($"--{key} {value}");

        var projectArg = projectName is "_atom"
            ? ""
            : $" --project {projectName}";

        var command = $"dotnet run --project {projectName}/{projectName}.csproj -- {workflowStep.Name}{projectArg} --skip --headless";

        if (args.Count > 0)
            command += $" {string.Join(" ", args)}";

        if (includeName)
            WriteLine($"# Command: {workflowStep.Name}");

        // Remove JSON output logic; rely on AtomWorkflowVariableProvider for variable passing
        WriteLine(command);
    }
}
