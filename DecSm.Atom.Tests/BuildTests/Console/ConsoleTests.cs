namespace DecSm.Atom.Tests.BuildTests.Console;

[TestFixture]
public class ConsoleTests
{
    [Test]
    public void Minimal_BuildDefinition_Displays_DefaultConsoleMessage()
    {
        // Arrange
        var testConsole = new TestConsole();
        var host = CreateTestHost<MinimalAtomBuild>(testConsole);

        // Act
        host.Run();

        // Assert
        testConsole.Output.ShouldBe("""

                                    Usage

                                    atom [options]
                                    atom [command/s] [parameters] [options]

                                    Options
                                    
                                      -h,  --help      Show help
                                      -g,  --gen       Generate build script
                                      -s,  --skip      Skip dependency execution
                                      -hl, --headless  Run in headless mode
                                      -v,  --verbose   Show verbose output

                                    Atom Commands

                                    ValidateBuild | Checks the atom build for common issues.


                                    """.Replace("\r\n", "\n"));
    }
}
