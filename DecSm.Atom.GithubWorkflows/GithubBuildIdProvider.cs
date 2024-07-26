namespace DecSm.Atom.GithubWorkflows;

public sealed class GithubBuildIdProvider : IBuildIdProvider
{
    public string BuildId => Github.Variables.RunId;
}