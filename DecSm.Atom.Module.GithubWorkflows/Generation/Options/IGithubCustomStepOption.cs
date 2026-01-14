namespace DecSm.Atom.Module.GithubWorkflows.Generation.Options;

[PublicAPI]
public interface IGithubCustomStepOption : IWorkflowOption
{
    GithubCustomStepOrder Order { get; }

    int Priority { get; }

    bool IWorkflowOption.AllowMultiple => true;

    void WriteStep(GithubStepWriter writer);
}
