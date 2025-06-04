﻿namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

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
                Targets.ArtifactTarget1,
                Targets.ArtifactTarget2,
                Targets.ArtifactTarget3,
                Targets.ArtifactTarget4.WithMatrixDimensions(new MatrixDimension(nameof(IArtifactSliceTarget1.Slice))
                {
                    Values = [IArtifactTarget2.Slice1, IArtifactTarget2.Slice2],
                }),
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
                Targets.SetupBuildInfo,
                Targets.ArtifactTarget1,
                Targets.ArtifactTarget2,
                Targets.ArtifactTarget3,
                Targets.ArtifactTarget4.WithMatrixDimensions(new MatrixDimension(nameof(IArtifactSliceTarget1.Slice))
                {
                    Values = [IArtifactTarget2.Slice1, IArtifactTarget2.Slice2],
                }),
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
        t => t
            .DescribedAs("Artifact Target 1")
            .ProducesArtifact(Artifact1);
}

[TargetDefinition]
public partial interface IArtifactTarget2 : IArtifactSliceTarget1
{
    const string Artifact2 = "TestArtifact2";

    Target ArtifactTarget2 =>
        t => t
            .DescribedAs("Artifact Target 2")
            .ConsumesArtifact(nameof(IArtifactTarget1.ArtifactTarget1), IArtifactTarget1.Artifact1)
            .ProducesArtifact(Artifact2, Slice1)
            .ProducesArtifact(Artifact2, Slice2);
}

[TargetDefinition]
public partial interface IArtifactTarget3 : IArtifactSliceTarget1
{
    Target ArtifactTarget3 =>
        t => t
            .DescribedAs("Artifact Target 3")
            .ConsumesArtifact(nameof(IArtifactTarget1.ArtifactTarget1), IArtifactTarget1.Artifact1)
            .ConsumesArtifact(nameof(IArtifactTarget2.ArtifactTarget2), IArtifactTarget2.Artifact2, Slice1)
            .ConsumesArtifact(nameof(IArtifactTarget2.ArtifactTarget2), IArtifactTarget2.Artifact2, Slice2);
}

[TargetDefinition]
public partial interface IArtifactTarget4
{
    Target ArtifactTarget4 =>
        t => t
            .DescribedAs("Artifact Target 4")
            .ConsumesArtifact(nameof(IArtifactTarget2.ArtifactTarget2), IArtifactTarget2.Artifact2);
}
