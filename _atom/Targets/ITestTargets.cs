namespace Atom.Targets;

internal interface ITestTargets : IDotnetTestHelper
{
    static readonly string[] ProjectsToTest =
    [
        Projects.DecSm_Atom_Tests.Name,
        Projects.DecSm_Atom_Analyzers_Tests.Name,
        Projects.DecSm_Atom_SourceGenerators_Tests.Name,
        Projects.DecSm_Atom_Module_GithubWorkflows_Tests.Name,
    ];

    Target TestProjects =>
        t => t
            .DescribedAs("Runs all unit tests for the Atom projects")
            .ProducesArtifacts(ProjectsToTest)
            .Executes(async cancellationToken =>
            {
                var exitCode = 0;

                string[] frameworks = ["net8.0", "net9.0", "net10.0"];

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var project in ProjectsToTest)
                foreach (var framework in frameworks)
                    exitCode += await DotnetTestAndStage(project,
                        new()
                        {
                            TestOptions = new()
                            {
                                Framework = framework,
                            },
                            IncludeCoverage = !project.Contains("Analyzers") && !project.Contains("SourceGenerators"),
                        },
                        cancellationToken);

                if (exitCode != 0)
                    throw new StepFailedException("One or more unit tests failed");
            });
}
