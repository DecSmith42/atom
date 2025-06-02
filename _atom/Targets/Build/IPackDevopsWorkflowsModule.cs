namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackDevopsWorkflowsModule : IDotnetPackHelper
{
    public const string AtomDevopsWorkflowsModuleProjectName = "DecSm.Atom.Module.DevopsWorkflows";

    Target PackDevopsWorkflowsModule =>
        d => d
            .DescribedAs("Builds the DevopsWorkflows extension project into a nuget package")
            .ProducesArtifact(AtomDevopsWorkflowsModuleProjectName)
            .Executes(() => DotnetPackProject(new(AtomDevopsWorkflowsModuleProjectName)));
}
