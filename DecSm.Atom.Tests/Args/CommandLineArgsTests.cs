namespace DecSm.Atom.Tests.Args;

[TestFixture]
public class CommandLineArgsTests
{
    [Test]
    public void HasHelp_ShouldReturnTrue_WhenHelpArgIsPresent()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new HelpArg(),
            });

        // Act
        var result = args.HasHelp;

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void HasHelp_ShouldReturnFalse_WhenHelpArgIsNotPresent()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new GenArg(),
            });

        // Act
        var result = args.HasHelp;

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void HasGen_ShouldReturnTrue_WhenGenArgIsPresent()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new GenArg(),
            });

        // Act
        var result = args.HasGen;

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void HasGen_ShouldReturnFalse_WhenGenArgIsNotPresent()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new HelpArg(),
            });

        // Act
        var result = args.HasGen;

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void HasSkip_ShouldReturnTrue_WhenSkipArgIsPresent()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new SkipArg(),
            });

        // Act
        var result = args.HasSkip;

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void HasSkip_ShouldReturnFalse_WhenSkipArgIsNotPresent()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new HelpArg(),
            });

        // Act
        var result = args.HasSkip;

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void HasHeadless_ShouldReturnTrue_WhenHeadlessArgIsPresent()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new HeadlessArg(),
            });

        // Act
        var result = args.HasHeadless;

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void HasHeadless_ShouldReturnFalse_WhenHeadlessArgIsNotPresent()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new HelpArg(),
            });

        // Act
        var result = args.HasHeadless;

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void HasVerbose_ShouldReturnTrue_WhenVerboseArgIsPresent()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new VerboseArg(),
            });

        // Act
        var result = args.HasVerbose;

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void HasVerbose_ShouldReturnFalse_WhenVerboseArgIsNotPresent()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new HelpArg(),
            });

        // Act
        var result = args.HasVerbose;

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void HasProject_ShouldReturnTrue_WhenProjectArgIsPresent()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new ProjectArg("project-1"),
            });

        // Act
        var result = args.HasProject;

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void Params_ShouldReturnAllParamArgs()
    {
        var paramArg1 = new ParamArg("param-1", "Param1", "1");
        var paramArg2 = new ParamArg("param-2", "Param2", "2");

        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                paramArg1,
                paramArg2,
                new HelpArg(),
            });

        // Act
        var result = args.Params;

        // Assert
        result.ShouldSatisfyAllConditions(() => result.ShouldNotBeNull(),
            () => result.Count.ShouldBe(2),
            () => result[0]
                .ShouldBeEquivalentTo(paramArg1),
            () => result[1]
                .ShouldBeEquivalentTo(paramArg2));
    }

    [Test]
    public void Commands_ShouldReturnAllCommandArgs()
    {
        var commandArg1 = new CommandArg("command-1");
        var commandArg2 = new CommandArg("command-2");

        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                commandArg1,
                commandArg2,
                new HelpArg(),
            });

        // Act
        var result = args.Commands;

        // Assert
        result.ShouldSatisfyAllConditions(() => result.ShouldNotBeNull(),
            () => result.Count.ShouldBe(2),
            () => result[0]
                .ShouldBeEquivalentTo(commandArg1),
            () => result[1]
                .ShouldBeEquivalentTo(commandArg2));
    }
}
