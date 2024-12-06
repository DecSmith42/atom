namespace Atom.Targets.Test;

[TargetDefinition]
internal partial interface IPublishTester : IDotnetPublishHelper
{
    const string PublishTesterProjectName = "PublishTester";

    Target PublishTester =>
        d => d
            .WithDescription("Publishes the PublishTester project.")
            .Executes(() => DotnetPublishProject(new(PublishTesterProjectName)
            {
                OutputArtifactName = "CustomPublish",
            }));
}
