namespace Atom.Targets;

internal interface ITestTargets : IDotnetTestHelper
{
    public const string AtomTestsProjectName = "DecSm.Atom.Tests";
    public const string AtomSourceGeneratorTestsProjectName = "DecSm.Atom.SourceGenerators.Tests";
    public const string AtomGithubWorkflowsTestsProjectName = "DecSm.Atom.Module.GithubWorkflows.Tests";

    Target TestAtom =>
        t => t
            .DescribedAs("Runs the unit tests projects")
            .ProducesArtifact(AtomTestsProjectName)
            .ProducesArtifact(AtomSourceGeneratorTestsProjectName)
            .ProducesArtifact(AtomGithubWorkflowsTestsProjectName)
            .Executes(async cancellationToken =>
            {
                var exitCode = 0;

                exitCode += await RunDotnetUnitTests(new(AtomTestsProjectName), cancellationToken);
                exitCode += await RunDotnetUnitTests(new(AtomSourceGeneratorTestsProjectName), cancellationToken);
                exitCode += await RunDotnetUnitTests(new(AtomGithubWorkflowsTestsProjectName), cancellationToken);

                if (exitCode != 0)
                    throw new StepFailedException("One or more unit tests failed");
            });
}
