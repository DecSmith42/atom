namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackPrivateTestLib : IDotnetPackHelper
{
    public const string PrivateTestLibProjectName = "DecSm.PrivateTestLib";

    Target PackPrivateTestLib =>
        d => d
            .WithDescription("Builds the PrivateTestLib project into a nuget package")
            .ProducesArtifact(PrivateTestLibProjectName)
            .Executes(() => DotnetPackProject(new(PrivateTestLibProjectName)));
}
