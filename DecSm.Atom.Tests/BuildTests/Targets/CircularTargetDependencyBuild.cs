namespace DecSm.Atom.Tests.BuildTests.Targets;

[BuildDefinition]
public partial class CircularTargetDependencyBuild : BuildDefinition, ICircularTarget1, ICircularTarget2
{
    public bool CircularTarget1Executed { get; set; }

    public bool CircularTarget2Executed { get; set; }
}

[TargetDefinition]
public partial interface ICircularTarget1
{
    bool CircularTarget1Executed { get; set; }

    Target CircularTarget1 =>
        d => d
            .DependsOn(CircularTargetDependencyBuild.Commands.CircularTarget1)
            .Executes(() =>
            {
                CircularTarget1Executed = true;

                return Task.CompletedTask;
            });
}

[TargetDefinition]
public partial interface ICircularTarget2
{
    bool CircularTarget2Executed { get; set; }

    Target CircularTarget2 =>
        d => d
            .DependsOn(CircularTargetDependencyBuild.Commands.CircularTarget2)
            .Executes(() =>
            {
                CircularTarget2Executed = true;

                return Task.CompletedTask;
            });
}

[BuildDefinition]
public partial class CircularTargetDependencyBuild2 : BuildDefinition, ITestCircularTarget3, ITestCircularTarget4, ITestCircularTarget5
{
    public bool CircularTarget3Executed { get; set; }

    public bool CircularTarget4Executed { get; set; }

    public bool CircularTarget5Executed { get; set; }
}

[TargetDefinition]
public partial interface ITestCircularTarget3
{
    bool CircularTarget3Executed { get; set; }

    Target TestCircularTarget3 =>
        d => d
            .DependsOn(CircularTargetDependencyBuild2.Commands.TestCircularTarget4)
            .Executes(() =>
            {
                CircularTarget3Executed = true;

                return Task.CompletedTask;
            });
}

[TargetDefinition]
public partial interface ITestCircularTarget4
{
    bool CircularTarget4Executed { get; set; }

    Target TestCircularTarget4 =>
        d => d
            .DependsOn(CircularTargetDependencyBuild2.Commands.TestCircularTarget5)
            .Executes(() =>
            {
                CircularTarget4Executed = true;

                return Task.CompletedTask;
            });
}

[TargetDefinition]
public partial interface ITestCircularTarget5
{
    bool CircularTarget5Executed { get; set; }

    Target TestCircularTarget5 =>
        d => d
            .DependsOn(CircularTargetDependencyBuild2.Commands.TestCircularTarget3)
            .Executes(() =>
            {
                CircularTarget5Executed = true;

                return Task.CompletedTask;
            });
}
