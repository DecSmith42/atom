namespace Atom.Targets;

[TargetDefinition]
public partial interface IPushToPrivateNuget : INugetHelper
{
    [ParamDefinition("private-nuget-feed", "The private Nuget feed to push to.", "https://nuget.pkg.github.com/decsmith42/index.json")]
    string PrivateNugetFeed => GetParam(() => PrivateNugetFeed) ?? "https://nuget.pkg.github.com/decsmith42/index.json";

    [SecretDefinition("private-nuget-api-key", "The private API key to use to push to Nuget.")]
    string PrivateNugetApiKey => GetParam(() => PrivateNugetApiKey)!;

    Target PushToPrivateNuget =>
        d => d
            .WithDescription("Pushes the private test lib to the private Nuget feed")
            .ConsumesArtifact(Commands.PackPrivateTestLib, IPackPrivateTestLib.PrivateTestLibProjectName)
            .RequiresParam(Params.PrivateNugetFeed)
            .RequiresParam(Params.PrivateNugetApiKey)
            .Executes(async () =>
            {
                await PushProject(IPackPrivateTestLib.PrivateTestLibProjectName, PrivateNugetFeed, PrivateNugetApiKey);
            });
}
