namespace Atom.Targets;

internal interface ITestTargets : IDotnetTestHelper
{
    const string AtomTestsProjectName = "DecSm.Atom.Tests";
    const string AtomSourceGeneratorTestsProjectName = "DecSm.Atom.SourceGenerators.Tests";
    const string AtomGithubWorkflowsTestsProjectName = "DecSm.Atom.Module.GithubWorkflows.Tests";

    Target TestAtom =>
        t => t
            .DescribedAs("Runs the unit tests projects")
            .ProducesArtifact(AtomTestsProjectName)
            .ProducesArtifact(AtomSourceGeneratorTestsProjectName)
            .ProducesArtifact(AtomGithubWorkflowsTestsProjectName)
            .Executes(async cancellationToken =>
            {
                var exitCode = 0;

                string[] projects = [AtomTestsProjectName, AtomSourceGeneratorTestsProjectName, AtomGithubWorkflowsTestsProjectName];
                string[] frameworks = ["net8.0", "net9.0", "net10.0"];

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var project in projects)
                foreach (var framework in frameworks)
                    exitCode += await RunDotnetUnitTests(new(project)
                        {
                            Configuration = "Release",
                            Framework = framework,
                        },
                        cancellationToken);

                if (exitCode != 0)
                    throw new StepFailedException("One or more unit tests failed");
            });
}
