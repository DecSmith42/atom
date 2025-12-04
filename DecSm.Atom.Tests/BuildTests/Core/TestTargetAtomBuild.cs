namespace DecSm.Atom.Tests.BuildTests.Core;

[MinimalBuildDefinition]
public partial class TestTargetAtomBuild : ITestTarget
{
    public string Description { get; set; } = "Test target";

    public Func<Task> Execute { get; set; } = () => Task.CompletedTask;
}

[TargetDefinition]
public partial interface ITestTarget
{
    string Description { get; set; }

    Func<Task> Execute { get; set; }

    Target TestTarget =>
        t => t
            .DescribedAs(Description)
            .Executes(Execute);
}
