namespace DecSm.Atom.Tests.BuildTests.Targets;

[BuildDefinition]
internal partial class UnspecifiedTargetsBuild : BuildDefinition, IUnspecifiedTarget1, IUnspecifiedTarget2, IUnspecifiedTarget3;

[TargetDefinition]
internal partial interface IUnspecifiedTarget1
{
    Target UnspecifiedTarget1 => d => d.WithDescription("Unspecified target 1");
}

[TargetDefinition]
internal partial interface IUnspecifiedTarget2
{
    Target UnspecifiedTarget2 =>
        d => d
            .DependsOn(nameof(IUnspecifiedTarget3.UnspecifiedTarget3))
            .WithDescription("Unspecified target 2");
}

[TargetDefinition]
internal partial interface IUnspecifiedTarget3
{
    Target UnspecifiedTarget3 => d => d.WithDescription("Unspecified target 3");
}
