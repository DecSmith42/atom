namespace DecSm.Atom.Module.Dotnet;

[TargetDefinition]
public partial interface IDotnetTestHelper : IDotnetToolHelper
{
    async Task<int> RunDotnetUnitTests(DotnetTestOptions options)
    {
        Logger.LogInformation("Running unit tests for Atom project {AtomProjectName}", options.ProjectName);

        var project = FileSystem.FileInfo.New(FileSystem.AtomRootDirectory / options.ProjectName / $"{options.ProjectName}.csproj");
        var projectPath = new RootedPath(FileSystem, project.FullName);

        if (!project.Exists)
            throw new InvalidOperationException($"Project file {project.FullName} does not exist.");

        List<RootedPath> filesToTransform = [projectPath];

        var dir = projectPath;

        do
        {
            dir = dir.Parent;

            if (dir is null)
                break;

            var file = dir / "Directory.Build.props";

            if (file.FileExists)
                filesToTransform.Add(file);
        } while (dir != FileSystem.AtomRootDirectory);

        var buildVersionProvider = GetService<IBuildVersionProvider>();

        await using var setVersionScope = options.AutoSetVersion
            ? TransformProjectVersionScope.Create(filesToTransform, buildVersionProvider.Version)
            : null;

        var testOutputDirectory = FileSystem.AtomRootDirectory / options.ProjectName / "TestResults";

        if (testOutputDirectory.DirectoryExists)
            FileSystem.Directory.Delete(testOutputDirectory, true);

        FileSystem.Directory.CreateDirectory(testOutputDirectory);

        var outputArtifactName = options.OutputArtifactName ?? options.ProjectName;
        var publishDirectory = FileSystem.AtomPublishDirectory / outputArtifactName;

        if (publishDirectory.DirectoryExists)
            FileSystem.Directory.Delete(publishDirectory, true);

        FileSystem.Directory.CreateDirectory(publishDirectory);

        var testResultsPublishDirectory = publishDirectory / "test-results";
        FileSystem.Directory.CreateDirectory(testResultsPublishDirectory);

        var coverageResultsPublishDirectory = publishDirectory / "coverage-results";
        FileSystem.Directory.CreateDirectory(coverageResultsPublishDirectory);

        // Run test
        var result = await GetService<ProcessRunner>()
            .RunAsync(new("dotnet",
            [
                $"test {project.FullName}",
                $"--configuration {options.Configuration}",
                $"--logger \"trx;LogFileName={options.ProjectName}.trx\"",
                $"--logger \"html;LogFileName={options.ProjectName}.html\"",
                "--collect:\"XPlat Code Coverage\"",
            ])
            {
                AllowFailedResult = true,
            });

        // Copy html file to publish directory
        FileSystem.File.Copy(testOutputDirectory / $"{options.ProjectName}.html",
            testResultsPublishDirectory / $"{options.ProjectName}.html");

        GenerateTestReport(options.ProjectName, testOutputDirectory / $"{options.ProjectName}.trx");

        // Install/update reportgenerator
        await InstallToolAsync("dotnet-reportgenerator-globaltool");

        // Run coverage report generator
        await GetService<ProcessRunner>()
            .RunAsync(new("reportgenerator",
            [
                $"-reports:{testOutputDirectory / "**" / "coverage.cobertura.xml"}",
                $"-targetdir:{coverageResultsPublishDirectory}",
                "-reporttypes:HtmlInline;JsonSummary",
                "-sourcedirs:" + FileSystem.AtomRootDirectory,
            ])
            {
                AllowFailedResult = true,
            });

        GenerateCoverageReport(options.ProjectName, coverageResultsPublishDirectory / "Summary.json");

        Logger.LogInformation("Ran unit tests for Atom project {AtomProjectName}", options.ProjectName);

        return result.ExitCode;
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
