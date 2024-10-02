namespace DecSm.Atom.Tests.Args;

[TestFixture]
public class CommandLineArgParserTests
{
    [Test]
    public void Parse_No_Args()
    {
        string[] rawArgs = [];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();
        var parser = new CommandLineArgsParser(build, console);

        var parsedArgs = parser.Parse(rawArgs);

        parsedArgs.Args.ShouldBeEmpty();
    }

    [TestCase("-h")]
    [TestCase("-H")]
    [TestCase("--help")]
    [TestCase("--HELP")]
    public void Parse_Help_Arg(string arg)
    {
        string[] rawArgs = [arg];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();
        var parser = new CommandLineArgsParser(build, console);

        var parsedArgs = parser.Parse(rawArgs);

        parsedArgs.Args.ShouldHaveSingleItem();

        parsedArgs
            .Args[0]
            .ShouldBeOfType<HelpArg>();
    }

    [TestCase("-g")]
    [TestCase("-G")]
    [TestCase("--gen")]
    [TestCase("--GEN")]
    public void Parse_Gen_Arg(string arg)
    {
        string[] rawArgs = [arg];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();
        var parser = new CommandLineArgsParser(build, console);

        var parsedArgs = parser.Parse(rawArgs);

        parsedArgs.Args.ShouldHaveSingleItem();

        parsedArgs
            .Args[0]
            .ShouldBeOfType<GenArg>();
    }

    [TestCase("-s")]
    [TestCase("-S")]
    [TestCase("--skip")]
    [TestCase("--SKIP")]
    public void Parse_Skip_Arg(string arg)
    {
        string[] rawArgs = [arg];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();
        var parser = new CommandLineArgsParser(build, console);

        var parsedArgs = parser.Parse(rawArgs);

        parsedArgs.Args.ShouldHaveSingleItem();

        parsedArgs
            .Args[0]
            .ShouldBeOfType<SkipArg>();
    }

    [TestCase("-hl")]
    [TestCase("-HL")]
    [TestCase("--headless")]
    [TestCase("--HEADLESS")]
    public void Parse_Headless_Arg(string arg)
    {
        string[] rawArgs = [arg];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();
        var parser = new CommandLineArgsParser(build, console);

        var parsedArgs = parser.Parse(rawArgs);

        parsedArgs.Args.ShouldHaveSingleItem();

        parsedArgs
            .Args[0]
            .ShouldBeOfType<HeadlessArg>();
    }

    [TestCase("-v")]
    [TestCase("-V")]
    [TestCase("--verbose")]
    [TestCase("--VERBOSE")]
    public void Parse_Verbose_Arg(string arg)
    {
        string[] rawArgs = [arg];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();
        var parser = new CommandLineArgsParser(build, console);

        var parsedArgs = parser.Parse(rawArgs);

        parsedArgs.Args.ShouldHaveSingleItem();

        parsedArgs
            .Args[0]
            .ShouldBeOfType<VerboseArg>();
    }

    [TestCase("--param1", "param1", "Param1")]
    [TestCase("--PARAM1", "param1", "Param1")]
    [TestCase("--param2", "param2", "Param2")]
    [TestCase("--PARAM2", "param2", "Param2")]
    public void Parse_Params_Arg(string arg, string argName, string paramName)
    {
        string[] rawArgs = [arg, "value"];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();

        A
            .CallTo(() => build.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                ["Param1"] = new("Param1", new("param1", "Param 1")),
                ["Param2"] = new("Param2", new("param2", "Param 2")),
                ["Param3"] = new("Param3", new("param3", "Param 3")),
            });

        var parser = new CommandLineArgsParser(build, console);

        var parsedArgs = parser.Parse(rawArgs);

        parsedArgs.Args.ShouldHaveSingleItem();

        parsedArgs
            .Args[0]
            .ShouldBeOfType<ParamArg>()
            .ShouldSatisfyAllConditions(x => x.ArgName.ShouldBe(argName),
                x => x.ParamName.ShouldBe(paramName),
                x => x.ParamValue.ShouldBe("value"));
    }

    [Test]
    public void Parse_Param_Without_Value()
    {
        string[] rawArgs = ["--param1", "--param2"];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();

        A
            .CallTo(() => build.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                ["Param1"] = new("Param1", new("param1", "Param 1")),
                ["Param2"] = new("Param2", new("param2", "Param 2")),
                ["Param3"] = new("Param3", new("param3", "Param 3")),
            });

        var parser = new CommandLineArgsParser(build, console);

        Should.Throw<ArgumentException>(() => parser.Parse(rawArgs));
    }

    [Test]
    public void Parse_Param_At_End()
    {
        string[] rawArgs = ["--param1"];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();

        A
            .CallTo(() => build.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                ["Param1"] = new("Param1", new("param1", "Param 1")),
                ["Param2"] = new("Param2", new("param2", "Param 2")),
                ["Param3"] = new("Param3", new("param3", "Param 3")),
            });

        var parser = new CommandLineArgsParser(build, console);

        Should.Throw<ArgumentException>(() => parser.Parse(rawArgs));
    }

    [TestCase("Command1", "Command1")]
    [TestCase("COMMAND1", "Command1")]
    [TestCase("Command2", "Command2")]
    [TestCase("COMMAND2", "Command2")]
    public void Parse_Command_Arg(string arg, string commandName)
    {
        string[] rawArgs = [arg];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();

        A
            .CallTo(() => build.TargetDefinitions)
            .Returns(new Dictionary<string, Target>
            {
                ["Command1"] = definition => definition,
                ["Command2"] = definition => definition,
                ["Command3"] = definition => definition,
            });

        var parser = new CommandLineArgsParser(build, console);

        var parsedArgs = parser.Parse(rawArgs);

        parsedArgs.Args.ShouldHaveSingleItem();

        parsedArgs
            .Args[0]
            .ShouldBeOfType<CommandArg>()
            .ShouldSatisfyAllConditions(x => x.Name.ShouldBe(commandName));
    }

    [TestCase("Unknown1")]
    [TestCase("asdf")]
    [TestCase("wololo")]
    public void Parse_Unknown_Command_Arg(string arg)
    {
        string[] rawArgs = [arg];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();

        A
            .CallTo(() => build.TargetDefinitions)
            .Returns(new Dictionary<string, Target>
            {
                ["Command1"] = definition => definition,
                ["Command2"] = definition => definition,
                ["Command3"] = definition => definition,
            });

        A
            .CallTo(() => build.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>());

        var parser = new CommandLineArgsParser(build, console);
        var parsedArgs = parser.Parse(rawArgs);

        parsedArgs.IsValid.ShouldBeFalse();
    }

    [Test]
    public void Parse_Complex_1()
    {
        string[] rawArgs = ["-h", "--param1", "value1", "--param2", "value2", "Command1"];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();

        A
            .CallTo(() => build.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                ["Param1"] = new("Param1", new("param1", "Param 1")),
                ["Param2"] = new("Param2", new("param2", "Param 2")),
                ["Param3"] = new("Param3", new("param3", "Param 3")),
            });

        A
            .CallTo(() => build.TargetDefinitions)
            .Returns(new Dictionary<string, Target>
            {
                ["Command1"] = definition => definition,
                ["Command2"] = definition => definition,
                ["Command3"] = definition => definition,
            });

        var parser = new CommandLineArgsParser(build, console);

        var parsedArgs = parser.Parse(rawArgs);

        parsedArgs.Args.ShouldSatisfyAllConditions(args => args.Count.ShouldBe(4),
            args => args[0]
                .ShouldBeOfType<HelpArg>(),
            args => args[1]
                .ShouldBeOfType<ParamArg>()
                .ShouldSatisfyAllConditions(x => x.ArgName.ShouldBe("param1"),
                    x => x.ParamName.ShouldBe("Param1"),
                    x => x.ParamValue.ShouldBe("value1")),
            args => args[2]
                .ShouldBeOfType<ParamArg>()
                .ShouldSatisfyAllConditions(x => x.ArgName.ShouldBe("param2"),
                    x => x.ParamName.ShouldBe("Param2"),
                    x => x.ParamValue.ShouldBe("value2")),
            args => args[3]
                .ShouldBeOfType<CommandArg>()
                .ShouldSatisfyAllConditions(x => x.Name.ShouldBe("Command1")));
    }

    [Test]
    public void Parse_Complex_2()
    {
        string[] rawArgs = ["--param1", "value1", "--param2", "value2", "Command1", "--skip"];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();

        A
            .CallTo(() => build.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                ["Param1"] = new("Param1", new("param1", "Param 1")),
                ["Param2"] = new("Param2", new("param2", "Param 2")),
                ["Param3"] = new("Param3", new("param3", "Param 3")),
            });

        A
            .CallTo(() => build.TargetDefinitions)
            .Returns(new Dictionary<string, Target>
            {
                ["Command1"] = definition => definition,
                ["Command2"] = definition => definition,
                ["Command3"] = definition => definition,
            });

        var parser = new CommandLineArgsParser(build, console);

        var parsedArgs = parser.Parse(rawArgs);

        parsedArgs.Args.ShouldSatisfyAllConditions(args => args.Count.ShouldBe(4),
            args => args[0]
                .ShouldBeOfType<ParamArg>()
                .ShouldSatisfyAllConditions(x => x.ArgName.ShouldBe("param1"),
                    x => x.ParamName.ShouldBe("Param1"),
                    x => x.ParamValue.ShouldBe("value1")),
            args => args[1]
                .ShouldBeOfType<ParamArg>()
                .ShouldSatisfyAllConditions(x => x.ArgName.ShouldBe("param2"),
                    x => x.ParamName.ShouldBe("Param2"),
                    x => x.ParamValue.ShouldBe("value2")),
            args => args[2]
                .ShouldBeOfType<CommandArg>()
                .ShouldSatisfyAllConditions(x => x.Name.ShouldBe("Command1")),
            args => args[3]
                .ShouldBeOfType<SkipArg>());
    }

    [Test]
    public void Parse_Complex_3()
    {
        string[] rawArgs = ["--param1", "value1", "--param2", "value2", "Command1", "-s", "--param3", "value3"];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();

        A
            .CallTo(() => build.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                ["Param1"] = new("Param1", new("param1", "Param 1")),
                ["Param2"] = new("Param2", new("param2", "Param 2")),
                ["Param3"] = new("Param3", new("param3", "Param 3")),
            });

        A
            .CallTo(() => build.TargetDefinitions)
            .Returns(new Dictionary<string, Target>
            {
                ["Command1"] = definition => definition,
                ["Command2"] = definition => definition,
                ["Command3"] = definition => definition,
            });

        var parser = new CommandLineArgsParser(build, console);

        var parsedArgs = parser.Parse(rawArgs);

        parsedArgs.Args.ShouldSatisfyAllConditions(args => args.Count.ShouldBe(5),
            args => args[0]
                .ShouldBeOfType<ParamArg>()
                .ShouldSatisfyAllConditions(x => x.ArgName.ShouldBe("param1"),
                    x => x.ParamName.ShouldBe("Param1"),
                    x => x.ParamValue.ShouldBe("value1")),
            args => args[1]
                .ShouldBeOfType<ParamArg>()
                .ShouldSatisfyAllConditions(x => x.ArgName.ShouldBe("param2"),
                    x => x.ParamName.ShouldBe("Param2"),
                    x => x.ParamValue.ShouldBe("value2")),
            args => args[2]
                .ShouldBeOfType<CommandArg>()
                .ShouldSatisfyAllConditions(x => x.Name.ShouldBe("Command1")),
            args => args[3]
                .ShouldBeOfType<SkipArg>(),
            args => args[4]
                .ShouldBeOfType<ParamArg>()
                .ShouldSatisfyAllConditions(x => x.ArgName.ShouldBe("param3"),
                    x => x.ParamName.ShouldBe("Param3"),
                    x => x.ParamValue.ShouldBe("value3")));
    }
}
