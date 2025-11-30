namespace Atom.Targets;

internal interface ITestTargets : IDotnetTestHelper
{
    List<RootedPath> ProjectsToTest =>
    [
        FileSystem.GetPath<Projects.DecSm_Atom_Tests>(),
        FileSystem.GetPath<Projects.DecSm_Atom_Module_GithubWorkflows_Tests>(),
    ];

    Target TestProjects =>
        t => t
            .DescribedAs("Runs all unit tests for the Atom projects")
            .ProducesArtifacts(ProjectsToTest.Select(project => project.FileNameWithoutExtension))
            .Executes(async cancellationToken =>
            {
                var exitCode = 0;

                string[] frameworks = ["net8.0", "net9.0", "net10.0"];

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var project in ProjectsToTest)
                foreach (var framework in frameworks)
                    exitCode += await RunDotnetUnitTests(new(project.FileNameWithoutExtension)
                        {
                            Configuration = "Release",
                            Framework = framework,
                        },
                        cancellationToken);

                if (exitCode != 0)
                    throw new StepFailedException("One or more unit tests failed");
            });
}
