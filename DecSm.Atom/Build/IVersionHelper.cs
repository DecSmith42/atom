namespace DecSm.Atom.Build;

[TargetDefinition]
public partial interface IVersionHelper
{
    VersionInfo Version =>
        GetService<IBuildVersionProvider>()
            .Version;
}