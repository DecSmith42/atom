namespace DecSm.Atom.Workflows.Writer;

public interface IWorkflowWriter
{
    Type WorkflowType { get; }

    void Generate(WorkflowModel workflow);
}

public interface IWorkflowWriter<T> : IWorkflowWriter
    where T : IWorkflowType
{
    Type IWorkflowWriter.WorkflowType => typeof(T);

    abstract void IWorkflowWriter.Generate(WorkflowModel workflow);
}