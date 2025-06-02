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
            .DependsOn(nameof(ICircularTarget1))
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
            .DependsOn(nameof(ICircularTarget2))
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
            .DependsOn(nameof(ITestCircularTarget4))
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
            .DependsOn(nameof(ITestCircularTarget5))
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
            .DependsOn(nameof(ITestCircularTarget3))
            .Executes(() =>
            {
                CircularTarget5Executed = true;

                return Task.CompletedTask;
            });
}
