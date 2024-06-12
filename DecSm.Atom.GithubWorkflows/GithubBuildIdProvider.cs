namespace DecSm.Atom.GithubWorkflows;

public sealed class GithubBuildIdProvider : IBuildIdProvider
{
    public long BuildId => int.Parse(Github.Variables.RunId);
}