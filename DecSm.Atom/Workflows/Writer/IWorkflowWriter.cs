﻿namespace DecSm.Atom.Workflows.Writer;

[PublicAPI]
public interface IWorkflowWriter
{
    Type WorkflowType { get; }

    Task Generate(WorkflowModel workflow);

    Task<bool> CheckForDirtyWorkflow(WorkflowModel workflow);
}

[PublicAPI]
public interface IWorkflowWriter<T> : IWorkflowWriter
    where T : IWorkflowType
{
    Type IWorkflowWriter.WorkflowType => typeof(T);

    abstract Task IWorkflowWriter.Generate(WorkflowModel workflow);

    abstract Task<bool> IWorkflowWriter.CheckForDirtyWorkflow(WorkflowModel workflow);
}
