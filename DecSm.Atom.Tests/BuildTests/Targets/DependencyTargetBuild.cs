namespace DecSm.Atom.Tests.BuildTests.Targets;

[BuildDefinition]
public partial class DependencyTargetBuild : BuildDefinition,
    IDependencyTarget1,
    IDependencyTarget2,
    IDependencyFailTarget1,
    IDependencyFailTarget2
{
    public bool DependencyFailTarget1Executed { get; set; }

    public bool DependencyFailTarget2Executed { get; set; }

    public bool DependencyTarget1Executed { get; set; }

    public bool DependencyTarget2Executed { get; set; }
}

[TargetDefinition]
public partial interface IDependencyTarget1
{
    bool DependencyTarget1Executed { get; set; }

    Target DependencyTarget1 =>
        d => d.Executes(() =>
        {
            DependencyTarget1Executed = true;

            return Task.CompletedTask;
        });
}

[TargetDefinition]
public partial interface IDependencyTarget2
{
    bool DependencyTarget2Executed { get; set; }

    Target DependencyTarget2 =>
        d => d
            .DependsOn(DependencyTargetBuild.Commands.DependencyTarget1)
            .Executes(() =>
            {
                DependencyTarget2Executed = true;

                return Task.CompletedTask;
            });
}

[TargetDefinition]
public partial interface IDependencyFailTarget1
{
    bool DependencyFailTarget1Executed { get; set; }

    Target DependencyFailTarget1 =>
        d => d.Executes(() =>
        {
            DependencyFailTarget1Executed = true;

            throw new("TestFailTarget1 failed");
        });
}

[TargetDefinition]
public partial interface IDependencyFailTarget2
{
    bool DependencyFailTarget2Executed { get; set; }

    Target DependencyFailTarget2 =>
        d => d
            .DependsOn(DependencyTargetBuild.Commands.DependencyFailTarget1)
            .Executes(() =>
            {
                DependencyFailTarget2Executed = true;

                return Task.CompletedTask;
            });
}
