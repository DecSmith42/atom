namespace Atom.Targets;

[TargetDefinition]
internal partial interface INugetCredentials
{
    [ParamDefinition("nuget-feed", "The Nuget feed to push to.", "https://api.nuget.org/v3/index.json")]
    string NugetFeed => GetParam(() => NugetFeed) ?? "https://api.nuget.org/v3/index.json";

    [SecretDefinition("nuget-api-key", "The API key to use to push to Nuget.")]
    string NugetApiKey => GetParam(() => NugetApiKey)!;
}