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
                var exitCode = 0;

                exitCode += await RunDotnetUnitTests(new(AtomTestsProjectName));
                exitCode += await RunDotnetUnitTests(new(AtomSourceGeneratorTestsProjectName));
                exitCode += await RunDotnetUnitTests(new(AtomGithubWorkflowsTestsProjectName));

                if (exitCode != 0)
                    throw new StepFailedException("One or more unit tests failed");
            });
}
