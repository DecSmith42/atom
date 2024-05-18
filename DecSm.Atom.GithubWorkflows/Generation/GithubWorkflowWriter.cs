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
            WriteLine();
            
            using (WriteSection("workflow_dispatch:"))
            {
                // if (PipelineInputs.Any())
                //     using (WriteSection("inputs:"))
                //     {
                //         foreach (var input in PipelineInputs)
                //             using (WriteSection($"{input.Name}:"))
                //             {
                //                 WriteLine($"type: {input.Type}");
                //                 WriteLine($"description: {input.Description}");
                //                 WriteLine($"required: {input.Required}");
                //
                //                 if (input.DefaultValue is not null)
                //                     WriteLine($"default: {input.DefaultValue}");
                //
                //                 if (input is PipelineChoiceInput choiceInput)
                //                     using (WriteSection("options:"))
                //                     {
                //                         foreach (var choice in choiceInput.Choices)
                //                             WriteLine($"- {choice}");
                //                     }
                //             }
                //     }
            }
            
            WriteLine();
            
            // if (PullRequestBranchTriggers.Length > 0)
            using (WriteSection("pull_request:"))
            {
                WriteLine("types: [opened, reopened, synchronize]");
                
                using (WriteSection("branches:"))
                    
                    // foreach (var branch in PullRequestBranchTriggers)
                    //     WriteLine($"- {branch}");
                    WriteLine("- main");
            }
            
            // if (PushBranchTriggers.Length > 0)
            //     using (WriteSection("push:"))
            //     {
            //         using (WriteSection("branches:"))
            //         {
            //             foreach (var branch in PushBranchTriggers)
            //                 WriteLine($"- {branch}");
            //         }
            //     }
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
                WriteLine($"  run: dotnet run --project {assemblyName}\\{assemblyName}.csproj {commandStep.Name} --skip");
                
                break;
        }
    }
}