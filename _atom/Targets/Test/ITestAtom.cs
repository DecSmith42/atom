namespace Atom.Targets.Test;

[TargetDefinition]
internal partial interface ITestAtom : IDotnetTestHelper
{
    public const string AtomTestsProjectName = "DecSm.Atom.Tests";
    public const string AtomSourceGeneratorTestsProjectName = "DecSm.Atom.SourceGenerators.Tests";

    Target TestAtom =>
        d => d
            .WithDescription("Runs the DecSm.Atom.Tests and DecSm.Atom.SourceGenerators.Tests projects.")
            .ProducesArtifact(AtomTestsProjectName)
            .ProducesArtifact(AtomSourceGeneratorTestsProjectName)
            .Executes(async () =>
            {
                await RunDotnetUnitTests(new(AtomTestsProjectName));
                await RunDotnetUnitTests(new(AtomSourceGeneratorTestsProjectName));
            });
}
