﻿namespace Atom.Targets.Test;

[TargetDefinition]
internal partial interface ITestAtom : IDotnetTestHelper
{
    public const string AtomUnitTestsProjectName = "DecSm.Atom.Tests";

    Target TestAtom =>
        d => d
            .WithDescription("Runs the DecSm.Atom.Tests unit tests project")
            .ProducesArtifact(AtomUnitTestsProjectName)
            .Executes(() => RunDotnetUnitTests(new(AtomUnitTestsProjectName)));
}