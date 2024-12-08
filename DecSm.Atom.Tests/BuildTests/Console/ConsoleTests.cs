namespace DecSm.Atom.Tests.BuildTests.Console;

[BuildDefinition]
public partial class ConsoleBuild : DefaultBuildDefinition, IConsoleTarget;

[TargetDefinition]
public partial interface IConsoleTarget
{
    [ParamDefinition("required-param", "Required param")]
    string RequiredParam => GetParam(() => RequiredParam)!;

    [ParamDefinition("default-param", "Default param", "default-value")]
    string DefaultParam => GetParam(() => DefaultParam, "default-value");

    [SecretDefinition("secret-param", "Secret param")]
    string SecretParam => GetParam(() => SecretParam)!;

    Target ConsoleTarget =>
        d => d
            .WithDescription("Console target")
            .RequiresParam(nameof(RequiredParam))
            .RequiresParam(nameof(DefaultParam))
            .RequiresParam(nameof(SecretParam))
            .Executes(() => Task.CompletedTask);
}

[TestFixture]
public class ConsoleTests
{
    [Test]
    public async Task MinimalBuildDefinition_Displays_DefaultConsoleMessage()
    {
        // Arrange
        var testConsole = new TestConsole();
        var host = CreateTestHost<MinimalAtomBuild>(testConsole);

        // Act
        await host.RunAsync();

        // Assert
        await Verify(testConsole.Output);
    }

    [Test]
    public async Task DefaultBuildDefinition_Displays_DefaultConsoleMessage()
    {
        // Arrange
        var testConsole = new TestConsole();
        var host = CreateTestHost<DefaultAtomBuild>(testConsole);

        // Act
        await host.RunAsync();

        // Assert
        await Verify(testConsole.Output);
    }

    [Test]
    public async Task ConsoleBuildDefinition_Displays_DefaultConsoleMessage()
    {
        // Arrange
        var testConsole = new TestConsole();
        var host = CreateTestHost<ConsoleBuild>(testConsole);

        // Act
        await host.RunAsync();

        // Assert
        await Verify(testConsole.Output);
    }
}
