namespace DecSm.Atom.Tests.BuildTests.Targets;

[MinimalBuildDefinition]
internal partial class UnspecifiedTargetsBuild : IUnspecifiedTarget1, IUnspecifiedTarget2, IUnspecifiedTarget3;

[TargetDefinition]
internal partial interface IUnspecifiedTarget1
{
    Target UnspecifiedTarget1 => t => t.DescribedAs("Unspecified target 1");
}

[TargetDefinition]
internal partial interface IUnspecifiedTarget2
{
    Target UnspecifiedTarget2 =>
        t => t
            .DependsOn(nameof(IUnspecifiedTarget3.UnspecifiedTarget3))
            .DescribedAs("Unspecified target 2");
}

[TargetDefinition]
internal partial interface IUnspecifiedTarget3
{
    Target UnspecifiedTarget3 => t => t.DescribedAs("Unspecified target 3");
}
