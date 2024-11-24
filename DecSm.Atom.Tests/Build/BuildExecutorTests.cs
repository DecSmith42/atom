namespace DecSm.Atom.Tests.Build;

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
        };

        _buildDefinition = new TestBuildDefinition();

        _paramService = A.Fake<IParamService>();
        _workflowVariableService = A.Fake<IWorkflowVariableService>();
        _outcomeReporters = [];
        _console = new();
        _reportService = A.Fake<IReportService>();
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
        public T? GetParam<T>(Expression<Func<T?>> parameterExpression, T? defaultValue = default, Func<string?, T?>? converter = null) =>
            throw new NotImplementedException();

        public Task WriteVariable(string name, string value) =>
            throw new NotImplementedException();

        public void AddReportData(IReportData reportData) =>
            throw new NotImplementedException();
    }

    private CommandLineArgs _commandLineArgs;
    private BuildModel _buildModel;

    // private Mock<IBuildDefinition> _buildDefinition;
    private IBuildDefinition _buildDefinition;

    // private Mock<IParamService> _paramService;
    private IParamService _paramService;

    // private Mock<IWorkflowVariableService> _workflowVariableService;
    private IWorkflowVariableService _workflowVariableService;

    // private IReadOnlyList<Mock<IOutcomeReporter>> _outcomeReporters;
    private IReadOnlyList<IOutcomeReporter> _outcomeReporters;
    private TestConsole _console;

    // private Mock<IReportService> _reportService;
    private IReportService _reportService;

    // private Mock<ILogger<BuildExecutor>> _logger;
    private ILogger<BuildExecutor> _logger;

    [Test]
    public async Task Execute_NoCommand_SucceedsAndLogs()
    {
        // Arrange
        // var buildExecutor = new BuildExecutor(_commandLineArgs,
        //     _buildModel,
        //     _paramService.Object,
        //     _workflowVariableService.Object,
        //     _outcomeReporters.Select(x => x.Object),
        //     _console,
        //     _reportService.Object,
        //     _logger.Object);

        var buildExecutor = new BuildExecutor(_commandLineArgs,
            _buildModel,
            _buildDefinition,
            _paramService,
            _workflowVariableService,
            _outcomeReporters,
            _console,
            _reportService,
            _logger);

        // Act
        await buildExecutor.Execute();

        // Assert
        // _logger.VerifyLog(x => x.LogInformation("No targets specified; execution skipped"), Times.Once);
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
        };

        // var buildExecutor = new BuildExecutor(_commandLineArgs,
        //     _buildModel,
        //     _paramService.Object,
        //     _workflowVariableService.Object,
        //     _outcomeReporters.Select(x => x.Object),
        //     _console,
        //     _reportService.Object,
        //     _logger.Object);

        var buildExecutor = new BuildExecutor(_commandLineArgs,
            _buildModel,
            _buildDefinition,
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
