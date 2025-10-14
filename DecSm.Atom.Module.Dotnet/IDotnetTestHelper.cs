namespace DecSm.Atom.Module.Dotnet;

public interface IDotnetTestHelper : IDotnetToolInstallHelper, IReportsHelper
{
    async Task<int> RunDotnetUnitTests(DotnetTestOptions options, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Running unit tests for Atom project {AtomProjectName}", options.ProjectName);

        var projectPath = DotnetFileUtils.GetProjectFilePathByName(FileSystem, options.ProjectName);

        await using var transformFilesScope = (options.AutoSetVersion, options.CustomPropertiesTransform) switch
        {
            (true, not null) => await TransformProjectVersionScope
                .CreateAsync(DotnetFileUtils.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                    GetService<IBuildVersionProvider>()
                        .Version,
                    cancellationToken)
                .AddAsync(options.CustomPropertiesTransform),

            (true, null) => await TransformProjectVersionScope.CreateAsync(
                DotnetFileUtils.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                GetService<IBuildVersionProvider>()
                    .Version,
                cancellationToken),

            (false, not null) => await TransformMultiFileScope.CreateAsync(
                DotnetFileUtils.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                options.CustomPropertiesTransform!,
                cancellationToken),

            _ => null,
        };

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
        var result = await ProcessRunner.RunAsync(new("dotnet",
            [
                $"test {projectPath.Path}",
                options.Configuration is not null
                    ? $"--configuration {options.Configuration}"
                    : string.Empty,
                options.Framework is not null
                    ? $"--framework {options.Framework}"
                    : string.Empty,
                $"--logger \"trx;LogFileName={options.ProjectName}.trx\"",
                $"--logger \"html;LogFileName={options.ProjectName}.html\"",
                "--collect:\"XPlat Code Coverage\"",
            ])
            {
                AllowFailedResult = true,
            },
            cancellationToken);

        // Copy html file to publish directory
        FileSystem.File.Copy(testOutputDirectory / $"{options.ProjectName}.html",
            testResultsPublishDirectory / $"{options.ProjectName}.html");

        GenerateTestReport(options, testOutputDirectory / $"{options.ProjectName}.trx");

        // Install/update reportgenerator
        await InstallToolAsync("dotnet-reportgenerator-globaltool", cancellationToken: cancellationToken);

        // Run coverage report generator
        await ProcessRunner.RunAsync(new("reportgenerator",
            [
                $"-reports:{testOutputDirectory / "**" / "coverage.cobertura.xml"}",
                $"-targetdir:{coverageResultsPublishDirectory}",
                "-reporttypes:HtmlInline;JsonSummary",
                "-sourcedirs:" + FileSystem.AtomRootDirectory,
            ])
            {
                AllowFailedResult = true,
            },
            cancellationToken);

        GenerateCoverageReport(options, coverageResultsPublishDirectory / "Summary.json");

        Logger.LogInformation("Ran unit tests for Atom project {AtomProjectName}", options.ProjectName);

        return result.ExitCode;
    }

    private void GenerateTestReport(DotnetTestOptions options, string trxFile)
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
            Title = $"Test run summary | {options.ProjectName} | {options.Configuration} | {options.Framework}",
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

    private void GenerateCoverageReport(DotnetTestOptions options, string coverageJsonFile)
    {
        var coverageJson = FileSystem.File.ReadAllText(coverageJsonFile);

        var summary = JsonSerializer.Deserialize<CoverageModel>(coverageJson)!.Summary;

        AddReportData(new TableReportData([
            ["Lines", summary.TotalLines.ToString()],
            ["Covered", summary.CoveredLines.ToString()],
            ["Uncovered", summary.UncoveredLines.ToString()],
            ["Coverable", summary.CoverableLines.ToString()],
            ["Line coverage", (summary.LineCoverage / 100).ToString("P")],
            ["Branch coverage", (summary.BranchCoverage / 100).ToString("P")],
        ])
        {
            Title = $"Coverage summary | {options.ProjectName} | {options.Configuration} | {options.Framework}",
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
