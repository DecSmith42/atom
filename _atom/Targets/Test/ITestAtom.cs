namespace Atom.Targets.Test;

[TargetDefinition]
internal partial interface ITestAtom : IDotnetTestHelper
{
    public const string AtomTestsProjectName = "DecSm.Atom.Tests";

    Target TestAtom =>
        d => d
            .WithDescription("Runs the DecSm.Atom.Tests project")
            .ProducesArtifact(AtomTestsProjectName)
            .Executes(() => RunDotnetUnitTests(new(AtomTestsProjectName)));
}
