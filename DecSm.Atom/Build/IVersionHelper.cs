namespace DecSm.Atom.Build;

[TargetDefinition]
public interface IVersionHelper : IBuildDefinition
{
    VersionInfo Version =>
        GetService<IBuildVersionProvider>()
            .Version;
}