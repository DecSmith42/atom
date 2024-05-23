namespace DecSm.Atom.Variables;

public interface IWorkflowVariableProvider
{
    public Type WorkflowType { get; }
    
    public Task WriteVariable(string variableName, string variableValue);
    
    public Task ReadVariable(string jobName, string variableName);
}

public interface IWorkflowVariableProvider<T> : IWorkflowVariableProvider
{
    Type IWorkflowVariableProvider.WorkflowType => typeof(T);
}