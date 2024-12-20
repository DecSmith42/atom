﻿namespace DecSm.Atom.Tests.ClassTests.Build;

[TestFixture]
public class BuildExecutorTests
{
    [SetUp]
    public void SetUp()
    {
        _commandLineArgs = new(true, []);

        _buildModel = new()
        {
            Targets = [],
            TargetStates = new Dictionary<TargetModel, TargetState>(),
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

        _buildDefinition = new TestBuildDefinition();

        _paramService = A.Fake<IParamService>();
        _workflowVariableService = A.Fake<IWorkflowVariableService>();
        _outcomeReporters = [];
        _console = new();
        _reportService = new();
        _logger = A.Fake<ILogger<BuildExecutor>>();
    }

    [TearDown]
    public void TearDown() =>
        _console.Dispose();

    private class TestBuildDefinition : IBuildDefinition
    {
        public IReadOnlyDictionary<string, Target> TargetDefinitions { get; } = new Dictionary<string, Target>();

        public IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions { get; } = new Dictionary<string, ParamDefinition>();

        public IServiceProvider Services { get; } = new ServiceCollection().BuildServiceProvider();

        [return: NotNullIfNotNull("defaultValue")]
        public T GetParam<T>(Expression<Func<T?>> parameterExpression, T? defaultValue = default, Func<string?, T?>? converter = null) =>
            throw new NotImplementedException();

        public Task WriteVariable(string name, string value) =>
            throw new NotImplementedException();

        public void AddReportData(IReportData reportData) =>
            throw new NotImplementedException();
    }

    private CommandLineArgs _commandLineArgs;
    private BuildModel _buildModel;

    private IBuildDefinition _buildDefinition;

    private IParamService _paramService;

    private IWorkflowVariableService _workflowVariableService;

    private IReadOnlyList<IOutcomeReporter> _outcomeReporters;
    private TestConsole _console;

    private ReportService _reportService;

    private ILogger<BuildExecutor> _logger;

    [Test]
    public async Task Execute_WithNoCommand_SucceedsAndLogs()
    {
        // Arrange
        var buildExecutor = new BuildExecutor(_commandLineArgs,
            _buildModel,
            _paramService,
            _workflowVariableService,
            _outcomeReporters,
            _console,
            _reportService,
            _logger);

        // Act
        await buildExecutor.Execute();

        // Assert
        // TODO: Verify logs
    }

    private class TestVal
    {
        public string? Value { get; set; }
    }

    [Test]
    public async Task Execute_WhenBuildIsValid_SucceedsAndLogs()
    {
        // Arrange
        var testVal = new TestVal();

        _commandLineArgs = new(true, [new CommandArg("Test")]);

        var target = new TargetModel("Test", null, false)
        {
            Tasks =
            [
                () =>
                {
                    testVal.Value = "Test";

                    return Task.CompletedTask;
                },
            ],
            RequiredParams = [],
            ConsumedArtifacts = [],
            ProducedArtifacts = [],
            ConsumedVariables = [],
            ProducedVariables = [],
            Dependencies = [],
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

        _buildModel = new()
        {
            Targets = [target],
            TargetStates = new Dictionary<TargetModel, TargetState>
            {
                {
                    target, new(target.Name)
                    {
                        Status = TargetRunState.PendingRun,
                        RunDuration = null,
                    }
                },
            },
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

        var buildExecutor = new BuildExecutor(_commandLineArgs,
            _buildModel,
            _paramService,
            _workflowVariableService,
            _outcomeReporters,
            _console,
            _reportService,
            _logger);

        // Act
        await buildExecutor.Execute();

        // Assert
        testVal.Value.ShouldBe("Test");
    }
}
