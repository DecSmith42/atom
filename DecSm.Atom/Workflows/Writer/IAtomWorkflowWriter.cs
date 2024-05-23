namespace DecSm.Atom.Workflows.Writer;

public interface IAtomWorkflowWriter
{
    Type WorkflowType { get; }
    
    void Generate(WorkflowModel workflow);
}

public interface IAtomWorkflowWriter<T> : IAtomWorkflowWriter where T : IWorkflowType
{
    Type IAtomWorkflowWriter.WorkflowType => typeof(T);
    
    abstract void IAtomWorkflowWriter.Generate(WorkflowModel workflow);
}