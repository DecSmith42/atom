namespace DecSm.Atom.Module.Dotnet.Helpers;

public partial interface IDotnetTestHelper : IDotnetCliHelper, IBuildInfo, IDotnetToolInstallHelper, IReportsHelper
{
    Task<int> DotnetTestAndStage(
        string projectName,
        DotnetTestAndStageOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var projectPath = DotnetFileUtil.GetProjectFilePathByName(FileSystem, projectName) ??
                          throw new StepFailedException($"Could not locate project file for project {projectName}.");

        Logger.LogDebug("Located project file for project {ProjectName} at {ProjectPath}", projectName, projectPath);

        return DotnetTestAndStage(projectPath, options, cancellationToken);
    }

    async Task<int> DotnetTestAndStage(
        RootedPath projectPath,
        DotnetTestAndStageOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var projectName = projectPath.FileNameWithoutExtension;

        options ??= new();
        var testOptions = options.TestOptions ?? new();

        testOptions = testOptions with
        {
            Output = testOptions.Output is { Length: > 0 }
                ? testOptions.Output
                : FileSystem.AtomRootDirectory / projectName / "TestResults",
            Logger = testOptions.Logger ??
            [
                $"\"trx;LogFileName={projectName}.trx\"", $"\"html;LogFileName={projectName}.html\"",
            ],
            Collect = testOptions.Collect ??
                      (options.IncludeCoverage
                          ? ["\"XPlat Code Coverage\""]
                          : null),
        };

        var testOutputDirectory = FileSystem.CreateRootedPath(testOptions.Output);

        var publishDirectory = FileSystem.AtomPublishDirectory / projectName;

        var testResultsPublishDirectory = publishDirectory / "test-results";

        if (FileSystem.Directory.Exists(testResultsPublishDirectory))
            FileSystem.Directory.Delete(testResultsPublishDirectory, true);

        FileSystem.Directory.CreateDirectory(testResultsPublishDirectory);

        var coverageResultsPublishDirectory = publishDirectory / "coverage-results";

        if (options.IncludeCoverage && FileSystem.Directory.Exists(coverageResultsPublishDirectory))
            FileSystem.Directory.Delete(coverageResultsPublishDirectory, true);

        if (options.IncludeCoverage)
            FileSystem.Directory.CreateDirectory(coverageResultsPublishDirectory);

        Logger.LogInformation("Running unit tests for project {Project}", projectName);

        if (FileSystem.Directory.Exists(testOutputDirectory))
        {
            Logger.LogDebug("Deleting existing test output directory {TestOutputDirectory}", testOutputDirectory);
            FileSystem.Directory.Delete(testOutputDirectory, true);
        }

        Logger.LogDebug(
            "Transforming project properties: SetVersionsFromProviders={SetVersionsFromProviders}, CustomPropertiesTransform={CustomPropertiesTransform}",
            options.SetVersionsFromProviders,
            options.CustomPropertiesTransform is not null
                ? "true"
                : "false");

        await using var transformFilesScope =
            (options.SetVersionsFromProviders, options.CustomPropertiesTransform) switch
            {
                (true, not null) => await TransformProjectVersionScope
                    .CreateAsync(DotnetFileUtil.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                        BuildVersion,
                        cancellationToken)
                    .AddAsync(options.CustomPropertiesTransform),

                (true, null) => await TransformProjectVersionScope.CreateAsync(
                    DotnetFileUtil.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                    BuildVersion,
                    cancellationToken),

                (false, not null) => await TransformMultiFileScope.CreateAsync(
                    DotnetFileUtil.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                    options.CustomPropertiesTransform!,
                    cancellationToken),

                _ => null,
            };

        var result = await DotnetCli.Test(projectPath,
            testOptions,
            new("", "")
            {
                TransformError = s => s.Contains(", is an invalid character")
                    ? null
                    : s,
            },
            cancellationToken);

        // Copy html file to publish directory
        FileSystem.File.Copy(testOutputDirectory / $"{projectName}.html",
            testResultsPublishDirectory / $"{projectName}.html");

        GenerateTestReport(projectName,
            testOptions.Configuration,
            testOptions.Framework,
            testOutputDirectory / $"{projectName}.trx");

        if (!options.IncludeCoverage)
            return result.ExitCode;

        await DotnetCli.ToolExecute("dotnet-reportgenerator-globaltool",
            [
                $"-reports:{testOutputDirectory / "**" / "coverage.cobertura.xml"}",
                $"-targetdir:{coverageResultsPublishDirectory}",
                "-reporttypes:HtmlInline;JsonSummary",
                "-sourcedirs:" + FileSystem.AtomRootDirectory,
            ],
            new()
            {
                Yes = true,
            },
            new("", "")
            {
                AllowFailedResult = true,
            },
            cancellationToken);

        GenerateCoverageReport(projectName,
            testOptions.Configuration,
            testOptions.Framework,
            coverageResultsPublishDirectory / "Summary.json");

        Logger.LogInformation("Ran unit tests for Atom project {AtomProjectName}", projectName);

        return result.ExitCode;
    }

    [UnconditionalSuppressMessage("Trimming",
        "IL2026:RequiresUnreferencedCode",
        Justification = "Deserialized type `TestRun` is manually preserved via DynamicallyAccessedMembers.")]
    void GenerateTestReport(string projectName, string? configuration, string? framework, string trxFile)
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
            Title =
                $"Test run summary | {projectName} | {configuration ?? "Release"} | {framework ?? RuntimeInformation.FrameworkDescription}",
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

    void GenerateCoverageReport(string projectName, string? configuration, string? framework, string coverageJsonFile)
    {
        var coverageJson = FileSystem.File.ReadAllText(coverageJsonFile);

        var summary = JsonSerializer.Deserialize(coverageJson, CoverageModelContext.Default.CoverageModel)!.Summary;

        AddReportData(new TableReportData([
            ["Lines", summary.TotalLines.ToString()],
            ["Covered", summary.CoveredLines.ToString()],
            ["Uncovered", summary.UncoveredLines.ToString()],
            ["Coverable", summary.CoverableLines.ToString()],
            ["Line coverage", (summary.LineCoverage / 100).ToString("P")],
            ["Branch coverage", (summary.BranchCoverage / 100).ToString("P")],
        ])
        {
            Title =
                $"Test run summary | {projectName} | {configuration ?? "Release"} | {framework ?? RuntimeInformation.FrameworkDescription}",
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

    [JsonSerializable(typeof(CoverageModel))]
    internal partial class CoverageModelContext : JsonSerializerContext;
}

[PublicAPI]
public sealed record DotnetTestAndStageOptions
{
    public TestOptions? TestOptions { get; init; }

    public bool SetVersionsFromProviders { get; init; } = true;

    public Func<string, string>? CustomPropertiesTransform { get; init; }

    public bool IncludeCoverage { get; init; } = true;
}
