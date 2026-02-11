namespace DecSm.Atom.Module.GithubWorkflows.Generation;

internal sealed class DependabotWorkflowWriter(IAtomFileSystem fileSystem, ILogger<DependabotWorkflowWriter> logger)
    : WorkflowFileWriter<DependabotWorkflowType>(fileSystem, logger)
{
    private readonly IAtomFileSystem _fileSystem = fileSystem;

    protected override string FileExtension => "yml";

    protected override int TabSize => 2;

    protected override RootedPath FileLocation => _fileSystem.AtomRootDirectory / ".github";

    protected override void WriteWorkflow(WorkflowModel workflow)
    {
        var dependabot = workflow
            .Options
            .OfType<DependabotOptions>()
            .Single();

        WriteLine("version: 2");

        if (dependabot.Registries.Count > 0)
        {
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
        }

        if (dependabot.Updates.Count > 0)
        {
            WriteLine();

            using (WriteSection("updates:"))
            {
                foreach (var update in dependabot.Updates)
                    using (WriteSection($"- package-ecosystem: \"{update.Ecosystem}\""))
                    {
                        WriteLine($"target-branch: \"{update.TargetBranch}\"");
                        WriteLine($"directory: \"{update.Directory}\"");

                        if (update.Registries.Count > 0)
                            using (WriteSection("registries:"))
                            {
                                foreach (var registry in update.Registries)
                                    WriteLine($"- {registry}");
                            }

                        if (update.Groups.Count > 0)
                            using (WriteSection("groups:"))
                            {
                                foreach (var group in update.Groups)
                                    using (WriteSection($"{group.Name}:"))
                                    using (WriteSection("patterns:"))
                                    {
                                        foreach (var pattern in group.Patterns ?? [])
                                            WriteLine($"- \"{pattern}\"");
                                    }
                            }

                        using (WriteSection("schedule:"))
                        using (WriteSection("interval:"))
                        {
                            WriteLine(update.Schedule switch
                            {
                                DependabotSchedule.Daily => "daily",
                                DependabotSchedule.Weekly => "weekly",
                                DependabotSchedule.Monthly => "monthly",
                                _ => throw new ArgumentOutOfRangeException(nameof(workflow),
                                    nameof(update.Schedule),
                                    $"Dependabot schedule '{update.Schedule}' is not supported."),
                            });
                        }

                        WriteLine($"open-pull-requests-limit: {update.OpenPullRequestsLimit}");
                    }
            }
        }
    }
}
