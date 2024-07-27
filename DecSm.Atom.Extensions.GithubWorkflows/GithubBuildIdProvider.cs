namespace DecSm.Atom.Extensions.GithubWorkflows;

public sealed class GithubBuildIdProvider : IBuildIdProvider
{
    public string BuildId => Github.Variables.RunId;
}