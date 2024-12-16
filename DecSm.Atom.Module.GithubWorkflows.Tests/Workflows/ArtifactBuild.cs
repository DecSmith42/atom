using DecSm.Atom.Workflows.Definition.Triggers;

namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class ArtifactBuild : BuildDefinition,
    IGithubWorkflows,
    IArtifactTarget1,
    IArtifactTarget2,
    IArtifactTarget3,
    IArtifactTarget4
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("artifact-workflow")
        {
            Triggers =
            [
                new GitPullRequestTrigger
                {
                    IncludedBranches = ["main"],
                },
            ],
            StepDefinitions =
            [
                Commands.ArtifactTarget1,
                Commands.ArtifactTarget2,
                Commands.ArtifactTarget3,
                Commands.ArtifactTarget4.WithMatrixDimensions(new MatrixDimension(nameof(IArtifactSliceTarget1.Slice),
                    [IArtifactTarget2.Slice1, IArtifactTarget2.Slice2])),
            ],
            WorkflowTypes = [new GithubWorkflowType()],
        },
    ];
}

[BuildDefinition]
public partial class CustomArtifactBuild : BuildDefinition,
    IGithubWorkflows,
    IStoreArtifact,
    IRetrieveArtifact,
    ISetupBuildInfo,
    IArtifactTarget1,
    IArtifactTarget2,
    IArtifactTarget3,
    IArtifactTarget4
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("custom-artifact-workflow")
        {
            Triggers =
            [
                new GitPullRequestTrigger
                {
                    IncludedBranches = ["main"],
                },
            ],
            StepDefinitions =
            [
                Commands.SetupBuildInfo,
                Commands.ArtifactTarget1,
                Commands.ArtifactTarget2,
                Commands.ArtifactTarget3,
                Commands.ArtifactTarget4.WithMatrixDimensions(new MatrixDimension(nameof(IArtifactSliceTarget1.Slice),
                    [IArtifactTarget2.Slice1, IArtifactTarget2.Slice2])),
            ],
            WorkflowTypes = [new GithubWorkflowType()],
            Options = [UseCustomArtifactProvider.Enabled],
        },
    ];
}

[TargetDefinition]
public partial interface IArtifactSliceTarget1
{
    const string Slice1 = "Slice1";
    const string Slice2 = "Slice2";

    [ParamDefinition("slice", "Slice")]
    string Slice => GetParam(() => Slice)!;
}

[TargetDefinition]
public partial interface IArtifactTarget1
{
    const string Artifact1 = "TestArtifact1";

    Target ArtifactTarget1 =>
        d => d
            .WithDescription("Artifact Target 1")
            .ProducesArtifact(Artifact1);
}

[TargetDefinition]
public partial interface IArtifactTarget2 : IArtifactSliceTarget1
{
    const string Artifact2 = "TestArtifact2";

    Target ArtifactTarget2 =>
        d => d
            .WithDescription("Artifact Target 2")
            .ConsumesArtifact(ArtifactBuild.Commands.ArtifactTarget1, IArtifactTarget1.Artifact1)
            .ProducesArtifact(Artifact2, Slice1)
            .ProducesArtifact(Artifact2, Slice2);
}

[TargetDefinition]
public partial interface IArtifactTarget3 : IArtifactSliceTarget1
{
    Target ArtifactTarget3 =>
        d => d
            .WithDescription("Artifact Target 3")
            .ConsumesArtifact(ArtifactBuild.Commands.ArtifactTarget1, IArtifactTarget1.Artifact1)
            .ConsumesArtifact(ArtifactBuild.Commands.ArtifactTarget2, IArtifactTarget2.Artifact2, Slice1)
            .ConsumesArtifact(ArtifactBuild.Commands.ArtifactTarget2, IArtifactTarget2.Artifact2, Slice2);
}

[TargetDefinition]
public partial interface IArtifactTarget4
{
    Target ArtifactTarget4 =>
        d => d
            .WithDescription("Artifact Target 4")
            .ConsumesArtifact(ArtifactBuild.Commands.ArtifactTarget2, IArtifactTarget2.Artifact2);
}
