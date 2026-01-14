namespace DecSm.Atom.Module.GithubWorkflows.Generation.Options;

[PublicAPI]
public abstract record GithubCustomStepOption(GithubCustomStepOrder Order, int Priority = 0) : IGithubCustomStepOption
{
    public abstract void WriteStep(GithubStepWriter writer);
}
