using DecSm.Atom.GithubWorkflows.Triggers;

namespace DecSm.Atom.GithubWorkflows.Generation;

public class GithubWorkflowWriter(IFileSystem fileSystem, ILogger<GithubWorkflowWriter> logger)
    : AtomWorkflowFileWriter<GithubWorkflowType>(fileSystem, logger)
{
    private readonly IFileSystem _fileSystem = fileSystem;
    
    protected override string FileExtension => "yml";
    
    protected override int TabSize => 2;
    
    protected override AbsolutePath FileLocation => _fileSystem.AtomRoot() / ".github" / "workflows";
    
    protected override void WriteWorkflow(Workflow workflow)
    {
        WriteLine($"name: {workflow.Name.ToLower()}");
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
                WriteJob(job);
            }
        }
    }
    
    private void WriteJob(WorkflowJob job)
    {
        using (WriteSection($"{job.Name}:"))
        {
            if (job.JobRequirements is { Count: > 0 })
            {
                var jobRequirementNames = job.JobRequirements.Select(x => x.Name);
                WriteLine($"needs: [ {string.Join(", ", jobRequirementNames)} ]");
            }
            
            WriteLine("runs-on: ubuntu-latest");
            
            using (WriteSection("steps:"))
            {
                foreach (var step in job.Steps)
                {
                    WriteLine();
                    WriteStep(step);
                }
            }
        }
    }
    
    private void WriteStep(IWorkflowStep step)
    {
        switch (step)
        {
            case CommandWorkflowStep commandStep:
                
                WriteLine("- name: Checkout");
                WriteLine("  uses: actions/checkout@v2");
                WriteLine();
                WriteLine("- name: Setup .NET");
                WriteLine("  uses: actions/setup-dotnet@v4");
                WriteLine();
                
                var assemblyName = Assembly.GetEntryAssembly()!.GetName()
                    .Name!;
                
                WriteLine($"- name: {commandStep.Name}");
                WriteLine($"  run: dotnet run --project {assemblyName}/{assemblyName}.csproj {commandStep.Name} --skip");
                
                break;
        }
    }
}