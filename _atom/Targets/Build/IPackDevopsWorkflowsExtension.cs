namespace Atom.Targets.Build;

[TargetDefinition]
internal partial interface IPackDevopsWorkflowsExtension : IDotnetPackHelper
{
    public const string AtomDevopsWorkflowsExtensionProjectName = "DecSm.Atom.Extensions.DevopsWorkflows";

    Target PackDevopsWorkflowsExtension =>
        d => d
            .WithDescription("Builds the DevopsWorkflows extension project into a nuget package")
            .ProducesArtifact(AtomDevopsWorkflowsExtensionProjectName)
            .Executes(() => DotnetPackProject(new(AtomDevopsWorkflowsExtensionProjectName)));
}
