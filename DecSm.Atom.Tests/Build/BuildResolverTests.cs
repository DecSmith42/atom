﻿namespace DecSm.Atom.Tests.Build;

[TestFixture]
public class BuildResolverTests
{
    [SetUp]
    public void Setup() =>
        _services = new ServiceCollection()
            .AddSingleton<IServiceProvider>(_ => _services)
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

    [Test]
    public void Resolve_WithCircularDependency_ThrowsException()
    {
        // Arrange
        var buildDefinition = new TestBuildDefinition(_services)
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["Target1"] = d => d.DependsOn("Target2"),
                ["Target2"] = d => d.DependsOn("Target1"),
            },
        };

        var commandLineArgs = new CommandLineArgs(true, []);
        var buildResolver = new BuildResolver(buildDefinition, commandLineArgs);

        // Act
        void Resolve()
        {
            buildResolver.Resolve();
        }

        // Assert
        Assert.Throws<Exception>(Resolve);
    }

    [Test]
    public void Resolve_WithSingleTarget_ReturnsModelWithSingleTarget()
    {
        // Arrange
        var buildDefinition = new TestBuildDefinition(_services)
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["Target1"] = d => d.Executes(() => Task.CompletedTask),
            },
        };

        var commandLineArgs = new CommandLineArgs(true, [new CommandArg("Target1")]);
        var buildResolver = new BuildResolver(buildDefinition, commandLineArgs);

        // Act
        var buildModel = buildResolver.Resolve();

        // Assert
        buildModel.ShouldSatisfyAllConditions(x => x.Targets.ShouldHaveSingleItem(),
            x => x
                .Targets[0]
                .Name
                .ShouldBe("Target1"),
            x => x
                .TargetStates[x.Targets[0]]
                .Status
                .ShouldBe(TargetRunState.PendingRun));
    }

    [Test]
    public void Resolve_WithMultipleTargets_ReturnsModelWithAllTargets()
    {
        // Arrange
        var buildDefinition = new TestBuildDefinition(_services)
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["Target1"] = d => d.Executes(() => Task.CompletedTask),
                ["Target2"] = d => d.Executes(() => Task.CompletedTask),
            },
        };

        var commandLineArgs = new CommandLineArgs(true, [new CommandArg("Target1"), new CommandArg("Target2")]);
        var buildResolver = new BuildResolver(buildDefinition, commandLineArgs);

        // Act
        var buildModel = buildResolver.Resolve();

        // Assert
        buildModel.ShouldSatisfyAllConditions(x => x.Targets.Count.ShouldBe(2),
            x => x
                .Targets
                .Any(t => t.Name == "Target1")
                .ShouldBeTrue(),
            x => x
                .Targets
                .Any(t => t.Name == "Target2")
                .ShouldBeTrue(),
            x => x
                .TargetStates[x.Targets.First(t => t.Name == "Target1")]
                .Status
                .ShouldBe(TargetRunState.PendingRun),
            x => x
                .TargetStates[x.Targets.First(t => t.Name == "Target2")]
                .Status
                .ShouldBe(TargetRunState.PendingRun));
    }

    [Test]
    public void Resolve_WithDependencies_ReturnsModelWithResolvedDependencies()
    {
        // Arrange
        var buildDefinition = new TestBuildDefinition(_services)
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["Target1"] = d => d
                    .DependsOn("Target2")
                    .Executes(() => Task.CompletedTask),
                ["Target2"] = d => d.Executes(() => Task.CompletedTask),
            },
        };

        var commandLineArgs = new CommandLineArgs(true, [new CommandArg("Target1")]);
        var buildResolver = new BuildResolver(buildDefinition, commandLineArgs);

        // Act
        var buildModel = buildResolver.Resolve();

        // Assert
        buildModel.ShouldSatisfyAllConditions(x => x.Targets.Count.ShouldBe(2),
            x => x
                .Targets
                .Any(t => t.Name == "Target1")
                .ShouldBeTrue(),
            x => x
                .Targets
                .Any(t => t.Name == "Target2")
                .ShouldBeTrue(),
            x => x
                .TargetStates[x.Targets.First(t => t.Name == "Target1")]
                .Status
                .ShouldBe(TargetRunState.PendingRun),
            x => x
                .TargetStates[x.Targets.First(t => t.Name == "Target2")]
                .Status
                .ShouldBe(TargetRunState.PendingRun));
    }

    [Test]
    public void Resolve_WithMissingDependency_ThrowsException()
    {
        // Arrange
        var buildDefinition = new TestBuildDefinition(_services)
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["Target1"] = d => d
                    .DependsOn("Target2")
                    .Executes(() => Task.CompletedTask),
            },
        };

        var commandLineArgs = new CommandLineArgs(true, [new CommandArg("Target1")]);
        var buildResolver = new BuildResolver(buildDefinition, commandLineArgs);

        // Act
        void Resolve()
        {
            buildResolver.Resolve();
        }

        // Assert
        Assert.Throws<Exception>(Resolve);
    }

    [Test]
    public void Resolve_WithSkippedTargets_ReturnsModelWithSkippedTargets()
    {
        // Arrange
        var buildDefinition = new TestBuildDefinition(_services)
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["Target1"] = d => d.Executes(() => Task.CompletedTask),
                ["Target2"] = d => d.Executes(() => Task.CompletedTask),
            },
        };

        var commandLineArgs = new CommandLineArgs(false, [new CommandArg("Target1")]);
        var buildResolver = new BuildResolver(buildDefinition, commandLineArgs);

        // Act
        var buildModel = buildResolver.Resolve();

        // Assert
        buildModel.ShouldSatisfyAllConditions(x => x.Targets.Count.ShouldBe(2),
            x => x
                .Targets
                .Any(t => t.Name == "Target1")
                .ShouldBeTrue(),
            x => x
                .Targets
                .Any(t => t.Name == "Target2")
                .ShouldBeTrue(),
            x => x
                .TargetStates[x.Targets.First(t => t.Name == "Target1")]
                .Status
                .ShouldBe(TargetRunState.PendingRun),
            x => x
                .TargetStates[x.Targets.First(t => t.Name == "Target2")]
                .Status
                .ShouldBe(TargetRunState.Skipped));
    }

    [Test]
    public void Resolve_WithArtifactDependencies_ReturnsModelWithResolvedArtifactDependencies()
    {
        // Arrange
        var buildDefinition = new TestBuildDefinition(_services)
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["Target1"] = d => d
                    .ConsumesArtifact("Target2", "Artifact1")
                    .Executes(() => Task.CompletedTask),
                ["Target2"] = d => d
                    .ProducesArtifact("Artifact1")
                    .Executes(() => Task.CompletedTask),
            },
        };

        var commandLineArgs = new CommandLineArgs(true, [new CommandArg("Target1")]);
        var buildResolver = new BuildResolver(buildDefinition, commandLineArgs);

        // Act
        var buildModel = buildResolver.Resolve();

        // Assert
        buildModel.ShouldSatisfyAllConditions(x => x.Targets.Count.ShouldBe(2),
            x => x
                .Targets
                .Any(t => t.Name == "Target1")
                .ShouldBeTrue(),
            x => x
                .Targets
                .FirstOrDefault(t => t.Name == "Target1")
                .ShouldNotBeNull()
                .ShouldSatisfyAllConditions(t => t.ConsumedArtifacts.Count.ShouldBe(1),
                    t => t
                        .ConsumedArtifacts[0]
                        .TargetName
                        .ShouldBe("Target2"),
                    t => t
                        .ConsumedArtifacts[0]
                        .ArtifactName
                        .ShouldBe("Artifact1")),
            x => x
                .Targets
                .Any(t => t.Name == "Target2")
                .ShouldBeTrue(),
            x => x
                .Targets
                .First(t => t.Name == "Target2")
                .ProducedArtifacts
                .Count
                .ShouldBe(1),
            x => x
                .TargetStates[x.Targets.First(t => t.Name == "Target1")]
                .Status
                .ShouldBe(TargetRunState.PendingRun),
            x => x
                .TargetStates[x.Targets.First(t => t.Name == "Target2")]
                .Status
                .ShouldBe(TargetRunState.PendingRun));
    }
}
