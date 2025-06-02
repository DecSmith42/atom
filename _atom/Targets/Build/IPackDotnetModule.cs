namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackDotnetModule : IDotnetPackHelper
{
    public const string DotnetModuleProjectName = "DecSm.Atom.Module.Dotnet";

    Target PackDotnetModule =>
        d => d
            .DescribedAs("Builds the Dotnet extension project into a nuget package")
            .ProducesArtifact(DotnetModuleProjectName)
            .Executes(() => DotnetPackProject(new(DotnetModuleProjectName)));
}
