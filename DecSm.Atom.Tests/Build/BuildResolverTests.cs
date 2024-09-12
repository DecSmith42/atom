namespace DecSm.Atom.Tests.Build;

[TestFixture]
public class BuildResolverTests
{
    [SetUp]
    public void Setup() =>
        _services = new ServiceCollection()
            .AddSingleton<IServiceProvider>(x => _services)
            .BuildServiceProvider();

    [TearDown]
    public void TearDown() =>
        _services.Dispose();

    public class TestBuildDefinition(IServiceProvider services) : BuildDefinition(services)
    {
        public IReadOnlyDictionary<string, Target> ManualTargetDefinitions { get; init; } = new Dictionary<string, Target>();

        public IReadOnlyDictionary<string, ParamDefinition> ManualParamDefinitions { get; init; } =
            new Dictionary<string, ParamDefinition>();

        public override IReadOnlyDictionary<string, Target> TargetDefinitions => ManualTargetDefinitions;

        public override IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions => ManualParamDefinitions;
    }

    private ServiceProvider _services = null!;

    [Test]
    public void Resolve_WithNoTargets_ReturnsEmptyModel()
    {
        // Arrange
        var buildDefinition = new TestBuildDefinition(_services);
        var commandLineArgs = new CommandLineArgs(true, []);
        var buildResolver = new BuildResolver(buildDefinition, commandLineArgs);

        // Act
        var buildModel = buildResolver.Resolve();

        // Assert
        buildModel.ShouldSatisfyAllConditions(x => x.Targets.ShouldBeEmpty(), x => x.TargetStates.ShouldBeEmpty());
    }

    [Test]
    public void Resolve_WithTargetNotSpecified_ReturnsModelWithTarget()
    {
        // Arrange
        var buildDefinition = new TestBuildDefinition(_services)
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["Target1"] = d => d.Executes(() => Task.CompletedTask),
            },
        };

        var commandLineArgs = new CommandLineArgs(true, []);
        var buildResolver = new BuildResolver(buildDefinition, commandLineArgs);

        // Act
        var buildModel = buildResolver.Resolve();

        // Assert
        buildModel.ShouldSatisfyAllConditions(x => x.Targets.ShouldHaveSingleItem(),
            x => x
                .Targets[0]
                .Name
                .ShouldBe("Target1"));
    }
}
