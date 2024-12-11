namespace Atom.Targets.Test;

[TargetDefinition]
internal partial interface ITestAtom : IDotnetTestHelper
{
    public const string AtomTestsProjectName = "DecSm.Atom.Tests";
    public const string AtomSourceGeneratorTestsProjectName = "DecSm.Atom.SourceGenerators.Tests";
    public const string AtomGithubWorkflowsTestsProjectName = "DecSm.Atom.Module.GithubWorkflows.Tests";

    Target TestAtom =>
        d => d
            .WithDescription("Runs the unit tests projects")
            .ProducesArtifact(AtomTestsProjectName)
            .ProducesArtifact(AtomSourceGeneratorTestsProjectName)
            .ProducesArtifact(AtomGithubWorkflowsTestsProjectName)
            .Executes(async () =>
            {
                await RunDotnetUnitTests(new(AtomTestsProjectName));
                await RunDotnetUnitTests(new(AtomSourceGeneratorTestsProjectName));
                await RunDotnetUnitTests(new(AtomGithubWorkflowsTestsProjectName));
            });
}
