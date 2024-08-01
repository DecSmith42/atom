namespace Atom.Targets;

[TargetDefinition]
internal partial interface ITestAtom : IDotnetTestHelper
{
    public const string AtomUnitTestsProjectName = "DecSm.Atom.Tests";

    Target TestAtom =>
        d => d
            .WithDescription("Runs all unit tests")
            .ProducesArtifact(AtomUnitTestsProjectName)
            .Executes(() => RunDotnetUnitTests(AtomUnitTestsProjectName));
}