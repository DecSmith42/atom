using System.Linq.Expressions;
using DecSm.Atom.Workflows.Definition.Command;

namespace DecSm.Atom.GithubWorkflows.Generation;

public sealed class GithubWorkflowWriter(
    IFileSystem fileSystem,
    IAtomBuildDefinition buildDefinition,
    ExecutableBuild build,
    ILogger<GithubWorkflowWriter> logger
) : AtomWorkflowFileWriter<GithubWorkflowType>(fileSystem, logger)
{
    private readonly IFileSystem _fileSystem = fileSystem;
    
    protected override string FileExtension => "yml";
    
    protected override int TabSize => 2;
    
    protected override AbsolutePath FileLocation => _fileSystem.SolutionRoot() / ".github" / "workflows";
    
    protected override void WriteWorkflow(Workflow workflow)
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
    
    private void WriteJob(Workflow workflow, WorkflowJob job)
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
                outputs.AddRange(build.Targets.Single(t => t.TargetDefinition.Name == step.Name)
                    .TargetDefinition.ProducedVariables);
            
            if (outputs.Count > 0)
                using (WriteSection("outputs:"))
                {
                    foreach (var output in outputs)
                        WriteLine($"{output}: ${{{{ steps.{job.Name}.outputs.{output} }}}}");
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
    
    private void WriteStep(Workflow workflow, WorkflowJob job, IWorkflowStep step)
    {
        switch (step)
        {
            case CommandWorkflowStep commandStep:
                
                WriteLine("- name: Checkout");
                WriteLine("  uses: actions/checkout@v2");
                WriteLine();
                
                // WriteLine("- name: Setup .NET");
                // WriteLine("  uses: actions/setup-dotnet@v4");
                // WriteLine();
                
                var target = build.Targets.Single(t => t.TargetDefinition.Name == commandStep.Name);
                
                var assemblyName = Assembly.GetEntryAssembly()!.GetName()
                    .Name!;
                
                foreach (var artifact in target.TargetDefinition.ConsumedArtifacts)
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
                
                using (WriteSection($"- name: {commandStep.Name}"))
                {
                    WriteLine($"run: dotnet run --project {assemblyName}/{assemblyName}.csproj {commandStep.Name} --skip");
                    
                    var injectedSecrets = target
                        .TargetDefinition
                        .Requirements
                        .Select(x => buildDefinition.ParamDefinitions[(x.Body as MemberExpression)!.Member.Name])
                        .Where(paramDef => workflow
                            .Options
                            .OfType<InjectGithubSecret>()
                            .Any(injection => injection.Name == paramDef.Name))
                        .ToArray();
                    
                    var env = new Dictionary<string, string>();
                    
                    foreach (var secret in injectedSecrets)
                        env[$"{secret.Attribute.ArgName}"] = $"${{{{ secrets.{secret.Attribute.ArgName.ToUpper().Replace('-', '_')} }}}}";
                    
                    foreach (var consumedVariable in target.TargetDefinition.ConsumedVariables)
                        env[consumedVariable.VariableName] = $"${{{{ needs.{commandStep.Name}.outputs.{consumedVariable.VariableName} }}}}";
                    
                    if (env.Count > 0)
                        using (WriteSection("env:"))
                            foreach (var (key, value) in env)
                                WriteLine($"{key}: {value}");
                }
                
                foreach (var artifact in target.TargetDefinition.ProducedArtifacts)
                {
                    WriteLine();
                    
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
}