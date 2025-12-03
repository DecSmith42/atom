namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class DuplicateDependencyBuild : BuildDefinition, IGithubWorkflows, IDuplicateDependencyTarget
{
    public override IReadOnlyList<IWorkflowOption> GlobalWorkflowOptions => [UseCustomArtifactProvider.Enabled];

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("duplicatedependency-workflow")
        {
            Triggers = [ManualTrigger.Empty],
            Targets = [Targets.DuplicateDependencyTarget1],
            WorkflowTypes = [Github.WorkflowType],
        },
    ];
}

[TargetDefinition]
[ConfigureHostBuilder]
public partial interface IDuplicateDependencyTarget : IStoreArtifact, IRetrieveArtifact
{
    Target DuplicateDependencyTarget1 =>
        t => t
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildName))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildId))
            .ProducesArtifact("artifact-name");

    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder.Services.AddSingleton<IArtifactProvider, TestArtifactProvider>();
}

internal sealed class TestArtifactProvider : IArtifactProvider
{
    public Task StoreArtifacts(
        IReadOnlyList<string> artifactNames,
        string? buildId = null,
        string? buildSlice = null,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task RetrieveArtifacts(
        IReadOnlyList<string> artifactNames,
        string? buildId = null,
        string? buildSlice = null,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task Cleanup(IReadOnlyList<string> runIdentifiers, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<string>> GetStoredRunIdentifiers(
        string? artifactName = null,
        string? buildSlice = null,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
