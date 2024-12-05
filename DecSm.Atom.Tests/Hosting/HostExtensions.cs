namespace DecSm.Atom.Tests.Hosting;

[TestFixture]
public class HostExtensionsTests
{
    [UsedImplicitly]
    private class TestBuildDefinition(IServiceProvider services) : BuildDefinition(services), IBuildDefinition
    {
        public override IReadOnlyDictionary<string, Target> TargetDefinitions => new Dictionary<string, Target>();

        public override IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions => new Dictionary<string, ParamDefinition>();
    }

    [Test]
    public void AddAtom_RegistersRequiredServices()
    {
        // Arrange
        var builder = new HostApplicationBuilder();

        // Act
        builder.AddAtom<HostApplicationBuilder, TestBuildDefinition>([]);

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();

        Assert.Multiple(() =>
        {
            serviceProvider
                .GetService<IHostedService>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<IBuildDefinition>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<IParamService>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<ReportService>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<IAtomFileSystem>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<IFileSystem>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<IAnsiConsole>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<BuildExecutor>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<WorkflowGenerator>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<ProcessRunner>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<IOutcomeReporter>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<IWorkflowVariableProvider>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<IWorkflowVariableService>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<IBuildIdProvider>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<IBuildVersionProvider>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<CheatsheetService>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<CommandLineArgsParser>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<CommandLineArgs>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<BuildResolver>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<WorkflowResolver>()
                .ShouldNotBeNull();

            serviceProvider
                .GetService<BuildModel>()
                .ShouldNotBeNull();
        });
    }

    [Test]
    public void AddAtom_ConfiguresLogging()
    {
        // Arrange
        var builder = new HostApplicationBuilder();

        // Act
        builder.AddAtom<HostApplicationBuilder, TestBuildDefinition>([]);

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

        loggerFactory.ShouldNotBeNull();
        var logger = loggerFactory.CreateLogger("TestLogger");
        logger.ShouldNotBeNull();
    }
}
