namespace DecSm.Atom.GithubWorkflows;

public sealed class GithubBuildIdProvider : IBuildIdProvider
{
    public long BuildId => long.Parse(Github.Variables.RunId);
}