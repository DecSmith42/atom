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
                        WriteLine($"type: {registry.Type}");
                        WriteLine($"url: {registry.Url}");

                        if (registry.Username is not null)
                            WriteLine($"username: {registry.Username}");

                        if (registry.Password is not null)
                            WriteLine($"password: {registry.Password}");

                        if (registry.Token is not null)
                            WriteLine($"token: {registry.Token}");
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
                        WriteLine($"target-branch: '{update.TargetBranch}'");
                        WriteLine($"directory: '{update.Directory}'");

                        if (update.ExcludePaths is { Count: > 0 })
                            using (WriteSection("exclude-paths:"))
                            {
                                foreach (var excludePath in update.ExcludePaths)
                                    WriteLine($"- '{excludePath}'");
                            }

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
                                    {
                                        if (group.Patterns is not { Count: > 0 })
                                            continue;

                                        using (WriteSection("patterns:"))
                                        {
                                            foreach (var pattern in group.Patterns)
                                                WriteLine($"- '{pattern}'");
                                        }
                                    }
                            }

                        if (update.Allow is { Count: > 0 })
                            using (WriteSection("allow:"))
                            {
                                foreach (var dependency in update.Allow)
                                    using (WriteSection($"- dependency-name: '{dependency.DependencyName}'"))
                                    {
                                        if (dependency.Versions is { Count: > 0 })
                                            using (WriteSection("versions:"))
                                            {
                                                foreach (var version in dependency.Versions)
                                                    WriteLine($"- '{version}'");
                                            }

                                        if (dependency.UpdateTypes.Count > 0)
                                            using (WriteSection("update-types:"))
                                            {
                                                foreach (var updateType in dependency.UpdateTypes)
                                                    WriteLine($"- '{updateType}'");
                                            }
                                    }
                            }

                        if (update.Ignore is { Count: > 0 })
                            using (WriteSection("ignore:"))
                            {
                                foreach (var dependency in update.Ignore)
                                    using (WriteSection($"- dependency-name: '{dependency.DependencyName}'"))
                                    {
                                        if (dependency.Versions is { Count: > 0 })
                                            using (WriteSection("versions:"))
                                            {
                                                foreach (var version in dependency.Versions)
                                                    WriteLine($"- '{version}'");
                                            }

                                        if (dependency.UpdateTypes.Count > 0)
                                            using (WriteSection("update-types:"))
                                            {
                                                foreach (var updateType in dependency.UpdateTypes)
                                                    WriteLine($"- '{updateType}'");
                                            }
                                    }
                            }

                        if (update.VersioningStrategy is not null)
                            WriteLine($"versioning-strategy: {update.VersioningStrategy switch
                            {
                                DependabotVersioningStrategy.Auto => "auto",
                                DependabotVersioningStrategy.Increase => "increase",
                                DependabotVersioningStrategy.IncreaseIfNecessary => "increase-if-necessary",
                                DependabotVersioningStrategy.LockfileOnly => "lockfile-only",
                                DependabotVersioningStrategy.Widen => "widen",
                                _ => throw new ArgumentOutOfRangeException(nameof(workflow),
                                    nameof(update.VersioningStrategy),
                                    $"Dependabot versioning strategy '{update.VersioningStrategy}' is not supported."),
                            }}");

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
