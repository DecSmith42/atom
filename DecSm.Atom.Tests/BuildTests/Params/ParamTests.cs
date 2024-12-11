namespace DecSm.Atom.Tests.BuildTests.Params;

[TestFixture]
public class ParamTests
{
    [Test]
    public void Param_IsReadFromCommandLine()
    {
        // Arrange
        var host = CreateTestHost<ParamBuild>(commandLineArgs: new(true, [new CommandArg(nameof(IParamTarget1.ParamTarget1))]));

        var build = (ParamBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        host.Run();

        // Assert
        build.ExecuteValue.ShouldBe("DefaultValue");
    }

    [Test]
    public void Param_FallsBackToDefault()
    {
        // Arrange
        var host = CreateTestHost<ParamBuild>(commandLineArgs: new(true,
            [new CommandArg(nameof(IParamTarget1.ParamTarget1)), new ParamArg("param-1", nameof(IParamTarget1.Param1), "TestValue")]));

        var build = (ParamBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        host.Run();

        // Assert
        build.ExecuteValue.ShouldBe("TestValue");
    }

    [Test]
    public void Param_WhenRequiredAndNotSupplied_StopsAndReturnsError()
    {
        // Arrange
        var loggerProvider = new TestLoggerProvider();

        var host = CreateTestHost<ParamBuild>(commandLineArgs: new(true, [new CommandArg(nameof(IParamTarget2.ParamTarget2))]),
            configure: builder => builder.Logging.AddProvider(loggerProvider));

        // Act
        host.Run();

        // Assert
        Environment.ExitCode.ShouldBe(1);

        loggerProvider
            .Logger
            .LogContent
            .ToString()
            .ShouldContain("Missing required parameter 'Param2' for target ParamTarget2");
    }
}
