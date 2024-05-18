namespace DecSm.Atom.Workflows.Implementation.Workflow.Bat;

public class BatWorkflowWriter(IFileSystem fileSystem, ILogger<BatWorkflowWriter> logger)
    : AtomWorkflowFileWriter<BatWorkflowType>(fileSystem, logger)
{
    protected override string FileExtension => "bat";
    
    protected override void WriteWorkflow(Model.Workflow workflow)
    {
        WriteLine("@echo off");
        WriteLine();
        
        using (WriteSection(":: Variables"))
        {
            WriteLine("setlocal");
            WriteLine();
        }
        
        using (WriteSection(":: Workflow"))
        {
            foreach (var job in workflow.Jobs)
            {
                WriteLine($":: {job.Name}");
                WriteLine($"echo Running {job.Name}...");
                
                foreach (var step in job.Steps)
                    WriteStep(step);
                
                WriteLine();
            }
        }
        
        using (WriteSection(":: Cleanup"))
            WriteLine("endlocal");
    }
    
    private void WriteStep(IWorkflowStep step)
    {
        switch (step)
        {
            case CommandWorkflowStep commandStep:
                
                var assemblyName = Assembly.GetEntryAssembly()!.GetName()
                    .Name!;
                
                WriteLine($"dotnet run --project {assemblyName}\\{assemblyName}.csproj {commandStep.Name} --skip");
                
                break;
            
            default:
                throw new NotSupportedException($"Unsupported step type: {step.GetType().Name}");
        }
    }
}