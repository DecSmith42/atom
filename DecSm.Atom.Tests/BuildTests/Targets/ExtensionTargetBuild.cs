namespace DecSm.Atom.Tests.BuildTests.Targets;

[MinimalBuildDefinition]
public partial class ExtensionTargetBuild : IBaseExtensionTarget, IExtendedExtensionTarget
{
    public bool BaseExtensionTargetExecuted { get; set; }

    public bool ExtendedExtensionTargetExecuted { get; set; }
}

[TargetDefinition]
public partial interface IBaseExtensionTarget
{
    bool BaseExtensionTargetExecuted { get; set; }

    Target BaseExtensionTarget =>
        t => t.Executes(() =>
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
        t => t
            .Extends<IBaseExtensionTarget>(definition => definition.BaseExtensionTarget)
            .Executes(() =>
            {
                ExtendedExtensionTargetExecuted = true;

                return Task.CompletedTask;
            });
}
