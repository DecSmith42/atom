namespace DecSm.Atom.Build.Definition;

[PublicAPI]
public abstract class DefaultBuildDefinition(IServiceProvider services) : BuildDefinition(services), ISetupBuildInfo, IValidateBuild;
