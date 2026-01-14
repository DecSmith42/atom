namespace DecSm.Atom.Module.GithubWorkflows.Generation.Options;

[PublicAPI]
public interface IGithubCustomStepOption : IWorkflowOption
{
    bool IWorkflowOption.AllowMultiple => true;

    GithubCustomStepOrder Order { get; }

    int Priority { get; }

    void WriteStep(GithubStepWriter writer);
}
