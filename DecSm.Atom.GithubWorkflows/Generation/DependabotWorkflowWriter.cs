namespace DecSm.Atom.GithubWorkflows.Generation;

public sealed class DependabotWorkflowWriter(IFileSystem fileSystem, ILogger<DependabotWorkflowWriter> logger)
    : AtomWorkflowFileWriter<DependabotWorkflowType>(fileSystem, logger)
{
    private readonly IFileSystem _fileSystem = fileSystem;
    
    protected override string FileExtension => "yml";
    
    protected override int TabSize => 2;
    
    protected override AbsolutePath FileLocation => _fileSystem.SolutionRoot() / ".github";
    
    protected override void WriteWorkflow(Workflow workflow)
    {
        var dependabot = workflow
            .Options
            .OfType<DependabotOptions>()
            .Single();
        
        WriteLine("version: 2");
        
        if (dependabot.Registries.Count == 0)
            return;
        
        WriteLine();
        
        using (WriteSection("registries:"))
        {
            foreach (var registry in dependabot.Registries)
                using (WriteSection($"{registry.Name}:"))
                {
                    WriteLine("type: nuget-feed");
                    WriteLine($"url: {registry.Url}");
                    
                    if (registry.Token is not null)
                        WriteLine($"token: ${{{{{registry.Token}}}}}");
                }
        }
        
        WriteLine();
        
        using (WriteSection("updates:"))
        {
            foreach (var update in dependabot.Updates)
                using (WriteSection("- package-ecosystem: \"nuget\""))
                {
                    WriteLine($"target-branch: \"{update.TargetBranch}\"");
                    WriteLine("directory: \"/\"");
                    
                    if (dependabot.Registries.Count != 0)
                    {
                        WriteLine("registries:");
                        
                        foreach (var registry in dependabot.Registries)
                            WriteLine($"  - {registry.Name}");
                    }
                    
                    using (WriteSection("groups:"))
                    using (WriteSection("nuget-deps:"))
                    using (WriteSection("patterns:"))
                        WriteLine("- \"*\"");
                    
                    WriteLine($"open-pull-requests-limit: {update.OpenPullRequestsLimit}");
                    
                    using (WriteSection("schedule:"))
                    {
                        using (WriteSection("interval:"))
                        {
                            WriteLine(update.Schedule switch
                            {
                                DependabotSchedule.Daily => "daily",
                                DependabotSchedule.Weekly => "weekly",
                                DependabotSchedule.Monthly => "monthly",
                                _ => throw new ArgumentOutOfRangeException(),
                            });
                        }
                    }
                }
        }
    }
}