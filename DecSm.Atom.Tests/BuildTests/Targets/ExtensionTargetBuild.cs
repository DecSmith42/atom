namespace DecSm.Atom.Tests.BuildTests.Targets;

[BuildDefinition]
public partial class ExtensionTargetBuild : BuildDefinition, IBaseExtensionTarget, IExtendedExtensionTarget
{
    public bool BaseExtensionTargetExecuted { get; set; }

    public bool ExtendedExtensionTargetExecuted { get; set; }
}

[TargetDefinition]
public partial interface IBaseExtensionTarget
{
    bool BaseExtensionTargetExecuted { get; set; }

    Target BaseExtensionTarget =>
        d => d.Executes(() =>
        {
            BaseExtensionTargetExecuted = true;

            return Task.CompletedTask;
        });
}

[TargetDefinition]
public partial interface IExtendedExtensionTarget
{
    bool ExtendedExtensionTargetExecuted { get; set; }

    Target ExtendedExtensionTarget =>
        d => d
            .Extends<IBaseExtensionTarget>(definition => definition.BaseExtensionTarget)
            .Executes(() =>
            {
                ExtendedExtensionTargetExecuted = true;

                return Task.CompletedTask;
            });
}
