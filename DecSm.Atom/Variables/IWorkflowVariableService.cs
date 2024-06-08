namespace DecSm.Atom.Variables;

public interface IWorkflowVariableService
{
    Task WriteVariable(string variableName, string variableValue);
    
    Task ReadVariable(string jobName, string variableName);
}