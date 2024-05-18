namespace DecSm.Atom.Workflows.Generation;

public interface IAtomWorkflowWriter
{
    Type WorkflowType { get; }
    void Generate(Workflow workflow);
}

public interface IAtomWorkflowWriter<T> : IAtomWorkflowWriter where T : IWorkflowType
{
    Type IAtomWorkflowWriter.WorkflowType => typeof(T);
    abstract void IAtomWorkflowWriter.Generate(Workflow workflow);
}