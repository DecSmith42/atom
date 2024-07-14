using DecSm.Atom.Reporter;

namespace DecSm.Atom.Tests.Build;

[TestFixture]
public class BuildExecutorTests
{
    private CommandLineArgs _commandLineArgs;
    private BuildModel _buildModel;
    private Mock<IParamService> _paramService;
    private Mock<IWorkflowVariableService> _workflowVariableService;
    private IReadOnlyList<Mock<IOutcomeReporter>> _outcomeReporters; 
    private TestConsole _console;
    private Mock<ILogger<BuildExecutor>> _logger;

    [SetUp]
    public void SetUp()
    {
        _commandLineArgs = new([]);

        _buildModel = new()
        {
            Targets = [],
            TargetStates = new Dictionary<TargetModel, TargetState>(),
        };

        _paramService = new();
        _workflowVariableService = new();
        _outcomeReporters = [];
        _console = new();
        _logger = new();
    }

    [TearDown]
    public void TearDown() =>
        _console.Dispose();

    [Test]
    public async Task Execute_NoCommand_SucceedsAndLogs()
    {
        // Arrange
        var buildExecutor = new BuildExecutor(_commandLineArgs,
            _buildModel,
            _paramService.Object,
            _workflowVariableService.Object,
            _outcomeReporters.Select(x => x.Object),
            _console,
            _logger.Object);

        // Act
        await buildExecutor.Execute();

        // Assert
        _logger.VerifyLog(x => x.LogInformation("No targets specified; execution skipped"), Times.Once);
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

        _commandLineArgs = new([new CommandArg("Test")]);

        var target = new TargetModel("Test", null)
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

        var buildExecutor = new BuildExecutor(_commandLineArgs,
            _buildModel,
            _paramService.Object,
            _workflowVariableService.Object,
            _outcomeReporters.Select(x => x.Object),
            _console,
            _logger.Object);

        // Act
        await buildExecutor.Execute();

        // Assert
        testVal.Value.ShouldBe("Test");
    }
}