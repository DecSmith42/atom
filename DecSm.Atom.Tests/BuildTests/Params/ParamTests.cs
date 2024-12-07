namespace DecSm.Atom.Tests.BuildTests.Params;

[BuildDefinition]
public partial class ParamTestBuild : BuildDefinition, IParamTarget1, IParamTarget2
{
    public string? ExecuteValue { get; set; }
}

[TargetDefinition]
public partial interface IParamTarget1
{
    [ParamDefinition("param-1", "Param 1", "DefaultValue")]
    string Param1 => GetParam(() => Param1, "DefaultValue");

    string? ExecuteValue { get; set; }

    Target ParamTarget1 =>
        d => d.Executes(() =>
        {
            ExecuteValue = Param1;

            return Task.CompletedTask;
        });
}

[TargetDefinition]
public partial interface IParamTarget2
{
    [ParamDefinition("param-2", "Param 2")]
    string Param2 => GetParam(() => Param2)!;

    string? ExecuteValue { get; set; }

    Target ParamTarget2 =>
        d => d
            .RequiresParam(nameof(Param2))
            .Executes(() =>
            {
                ExecuteValue = Param2;

                return Task.CompletedTask;
            });
}

[TestFixture]
public class ParamTests
{
    [Test]
    public void Param_IsReadFromCommandLine()
    {
        // Arrange
        var host = CreateTestHost<ParamTestBuild>(commandLineArgs: new(true, [new CommandArg(nameof(IParamTarget1.ParamTarget1))]));

        var build = (ParamTestBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        host.Run();

        // Assert
        build.ExecuteValue.ShouldBe("DefaultValue");
    }

    [Test]
    public void Param_FallsBackToDefault()
    {
        // Arrange
        var host = CreateTestHost<ParamTestBuild>(commandLineArgs: new(true,
            [new CommandArg(nameof(IParamTarget1.ParamTarget1)), new ParamArg("param-1", nameof(IParamTarget1.Param1), "TestValue")]));

        var build = (ParamTestBuild)host.Services.GetRequiredService<IBuildDefinition>();

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

        var host = CreateTestHost<ParamTestBuild>(commandLineArgs: new(true, [new CommandArg(nameof(IParamTarget2.ParamTarget2))]),
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
