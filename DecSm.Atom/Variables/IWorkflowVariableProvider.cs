namespace DecSm.Atom.Variables;

public interface IWorkflowVariableProvider
{
    public Task<bool> WriteVariable(string variableName, string variableValue);

    public Task<bool> ReadVariable(string jobName, string variableName);
}