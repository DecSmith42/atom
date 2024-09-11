namespace DecSm.Atom.Build;

[TargetDefinition]
public partial interface IVersionHelper
{
    SemVer Version =>
        GetService<IBuildVersionProvider>()
            .Version;
}
