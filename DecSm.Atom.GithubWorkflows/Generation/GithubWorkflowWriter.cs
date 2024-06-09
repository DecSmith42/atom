namespace DecSm.Atom.GithubWorkflows.Generation;

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
                
                WriteLine("- name: Checkout");
                WriteLine("  uses: actions/checkout@v2");
                WriteLine();
                
                var target = buildModel.Targets.Single(t => t.Name == commandStep.Name);
                
                foreach (var artifact in target.ConsumedArtifacts)
                {
                    if (workflow
                        .Options
                        .OfType<UseArtifactProvider>()
                        .Any())
                        WriteCommandStep(workflow,
                            new(nameof(IDownloadArtifact.DownloadArtifact)),
                            buildModel.Targets.Single(t => t.Name == nameof(IDownloadArtifact.DownloadArtifact)),
                            [("download-artifact-name", artifact.ArtifactName)]);
                    else
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
                
                WriteCommandStep(workflow, commandStep, target, []);
                
                foreach (var artifact in target.ProducedArtifacts)
                {
                    WriteLine();
                    
                    if (workflow
                        .Options
                        .OfType<UseArtifactProvider>()
                        .Any())
                        WriteCommandStep(workflow,
                            new(nameof(IUploadArtifact.UploadArtifact)),
                            buildModel.Targets.Single(t => t.Name == nameof(IUploadArtifact.UploadArtifact)),
                            [("upload-artifact-name", artifact.ArtifactName)]);
                    else
                        using (WriteSection($"- name: Upload {artifact.ArtifactName}"))
                        {
                            WriteLine("uses: actions/upload-artifact@v4");
                            
                            using (WriteSection("with:"))
                            {
                                WriteLine($"name: {artifact.ArtifactName}");
                                WriteLine($"path: \"{Github.PipelinePublishDirectory}/{artifact.ArtifactName}\"");
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
        (string name, string value)[] extraParams)
    {
        var assemblyName = Assembly.GetEntryAssembly()!.GetName()
            .Name!;
        
        using (WriteSection($"- name: {commandStep.Name}"))
        {
            WriteLine($"id: {commandStep.Name}");
            
            WriteLine($"run: dotnet run --project {assemblyName}/{assemblyName}.csproj {commandStep.Name} --skip --headless");
            
            var env = new Dictionary<string, string>();
            
            foreach (var consumedVariable in target.ConsumedVariables)
                env[buildDefinition.ParamDefinitions[consumedVariable.VariableName].Attribute.ArgName] =
                    $"${{{{ needs.{consumedVariable.TargetName}.outputs.{buildDefinition.ParamDefinitions[consumedVariable.VariableName].Attribute.ArgName} }}}}";
            
            if (target
                .RequiredParams
                .Select(x => buildDefinition.ParamDefinitions[x])
                .Any(x => x.Attribute.IsSecret))
                foreach (var injectedSecret in workflow.Options.OfType<WorkflowSecretInjection>())
                {
                    var paramDefinition = buildDefinition.ParamDefinitions.GetValueOrDefault(injectedSecret.Param);
                    
                    if (paramDefinition is not null)
                        env[paramDefinition.Attribute.ArgName] =
                            $"${{{{ secrets.{paramDefinition.Attribute.ArgName.ToUpper().Replace('-', '_')} }}}}";
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