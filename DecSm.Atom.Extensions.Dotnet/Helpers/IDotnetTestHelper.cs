namespace DecSm.Atom.Extensions.Dotnet.Helpers;

[TargetDefinition]
public partial interface IDotnetTestHelper : IProcessHelper
{
    async Task RunDotnetUnitTests(string projectName)
    {
        Logger.LogInformation("Running unit tests for Atom project {AtomProjectName}", projectName);

        var project = FileSystem.FileInfo.New(FileSystem.SolutionRoot() / projectName / $"{projectName}.csproj");

        if (!project.Exists)
            throw new InvalidOperationException($"Project file {project.FullName} does not exist.");

        var testOutputDirectory = FileSystem.SolutionRoot() / projectName / "TestResults";

        if (testOutputDirectory.DirectoryExists)
            FileSystem.Directory.Delete(testOutputDirectory, true);

        FileSystem.Directory.CreateDirectory(testOutputDirectory);

        var projectPublishDirectory = FileSystem.PublishDirectory() / projectName;

        if (projectPublishDirectory.DirectoryExists)
            FileSystem.Directory.Delete(projectPublishDirectory, true);

        FileSystem.Directory.CreateDirectory(projectPublishDirectory);

        var testResultsPublishDirectory = projectPublishDirectory / "test-results";
        FileSystem.Directory.CreateDirectory(testResultsPublishDirectory);

        var coverageResultsPublishDirectory = projectPublishDirectory / "coverage-results";
        FileSystem.Directory.CreateDirectory(coverageResultsPublishDirectory);

        // Run test
        await RunProcessAsync("dotnet",
            [
                $"test {project.FullName}",
                "--configuration Release",
                $"--logger \"trx;LogFileName={projectName}.trx\"",
                $"--logger \"html;LogFileName={projectName}.html\"",
                "--collect:\"XPlat Code Coverage\"",
            ],
            suppressError: true);

        // Copy html file to publish directory
        FileSystem.File.Copy(testOutputDirectory / $"{projectName}.html", testResultsPublishDirectory / $"{projectName}.html");

        GenerateTestReport(projectName, testOutputDirectory / $"{projectName}.trx");

        // Install/update reportgenerator
        await RunProcessAsync("dotnet", ["tool", "install", "dotnet-reportgenerator-globaltool", "--global"]);

        // Run coverage report generator
        await RunProcessAsync("reportgenerator",
        [
            $"-reports:{testOutputDirectory / "**" / "coverage.cobertura.xml"}",
            $"-targetdir:{coverageResultsPublishDirectory}",
            "-reporttypes:HtmlInline;JsonSummary",
            "-sourcedirs:" + FileSystem.SolutionRoot(),
        ]);

        GenerateCoverageReport(projectName, coverageResultsPublishDirectory / "Summary.json");

        Logger.LogInformation("Ran unit tests for Atom project {AtomProjectName}", projectName);
    }

    private void GenerateTestReport(string projectName, string trxFile)
    {
        var serializer = new XmlSerializer(typeof(TestRun));

        using var reader = new StreamReader(trxFile);
        var testRun = (TestRun)serializer.Deserialize(reader)!;

        AddReportData(new TableReportData([
            ["⌛ Run duration", PrintDuration(testRun.Times.Finish - testRun.Times.Start)],
            ["🔢 Total tests", testRun.ResultSummary.Counters.Total.ToString()],
            ["✅ Passed tests", testRun.ResultSummary.Counters.Passed.ToString()],
            ["❌ Failed tests", testRun.ResultSummary.Counters.Failed.ToString()],
            ["⏩ Skipped tests", testRun.ResultSummary.Counters.NotExecuted.ToString()],
        ])
        {
            Title = $"{projectName} - Test run summary",
            ColumnAlignments = [ColumnAlignment.Left, ColumnAlignment.Right],
        });

        var failedCaseRows = new List<string>();

        var failedTests = testRun
            .Results
            .Where(x => x.Outcome == "Failed")
            .Select(x => (Result: x, Definition: testRun.TestDefinitions.First(y => y.Id == x.TestId)));

        var failedTestsByClass = failedTests.GroupBy(x => x.Definition.TestMethod);

        foreach (var classWithTests in failedTestsByClass)
        {
            failedCaseRows.Add($"- {classWithTests.Key.ClassName}");

            failedCaseRows.AddRange(classWithTests.Select(test =>
                $"  - {test.Definition.Name} | {PrintDuration(test.Result.EndTime - test.Result.StartTime)} | {test.Result.Output.ErrorInfo.Message}"));
        }

        if (failedCaseRows.Count > 0)
            AddReportData(new ListReportData(failedCaseRows)
            {
                Title = "❌ Failed cases",
                Prefix = string.Empty,
            });
    }

    private void GenerateCoverageReport(string projectName, string coverageJsonFile)
    {
        var coverageJson = FileSystem.File.ReadAllText(coverageJsonFile);

        var summary = JsonSerializer.Deserialize<CoverageModel>(coverageJson)!.Summary;

        AddReportData(new TableReportData([
            [
                summary.TotalLines.ToString(),
                summary.CoveredLines.ToString(),
                summary.UncoveredLines.ToString(),
                summary.CoverableLines.ToString(),
                (summary.LineCoverage / 100).ToString("P"),
                (summary.BranchCoverage / 100).ToString("P"),
            ],
        ])
        {
            Title = $"{projectName} - Coverage summary",
            Header = ["Lines", "Covered", "Uncovered", "Coverable", "Line coverage", "Branch coverage"],
            ColumnAlignments = [ColumnAlignment.Left, ColumnAlignment.Right],
        });
    }

    private static string PrintDuration(TimeSpan duration) =>
        duration.TotalSeconds switch
        {
            < 60 => $"{duration.TotalSeconds:0.##}s",
            < 3600 => $"{duration.Minutes}m {duration.TotalSeconds % 60:0.##}s",
            _ => $"{duration.Hours}h {duration.Minutes}m {duration.Seconds}s",
        };
}