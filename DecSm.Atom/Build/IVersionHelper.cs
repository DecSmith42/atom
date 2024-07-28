namespace DecSm.Atom.Build;

[TargetDefinition]
public interface IVersionHelper : IBuildDefinition
{
    VersionInfo Version =>
        Services.GetRequiredService<IBuildVersionProvider>()
            .Version;
}