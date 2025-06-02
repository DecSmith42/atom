namespace DecSm.Atom.Tests.BuildTests.Core;

[BuildDefinition]
public partial class TestTargetAtomBuild : BuildDefinition, ITestTarget
{
    public string Description { get; set; } = "Test target";

    public Func<Task> Execute { get; set; } = () => Task.CompletedTask;
}

[TargetDefinition]
public partial interface ITestTarget
{
    public string Description { get; set; }

    public Func<Task> Execute { get; set; }

    Target TestTarget =>
        d => d
            .DescribedAs(Description)
            .Executes(Execute);
}
