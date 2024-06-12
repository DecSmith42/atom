namespace DecSm.Atom.GithubWorkflows;

public sealed class GithubBuildIdProvider : IBuildIdProvider
{
    public int BuildId => int.Parse(Github.Variables.RunId);
}