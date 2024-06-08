namespace DecSm.Atom.Variables;

public class WorkflowVariableService(IEnumerable<IWorkflowVariableProvider> workflowVariableProviders, IBuildDefinition buildDefinition)
    : IWorkflowVariableService
{
    // ReSharper disable once PossibleMultipleEnumeration - Once-only operation
    private readonly IWorkflowVariableProvider[] _customProviders = workflowVariableProviders
        .Where(x => x is not BaseWorkflowVariableProvider)
        .ToArray();
    
    // ReSharper disable once PossibleMultipleEnumeration - Once-only operation
    private readonly IWorkflowVariableProvider _baseProvider = workflowVariableProviders
        .OfType<BaseWorkflowVariableProvider>()
        .Single();
    
    public async Task WriteVariable(string variableName, string variableValue)
    {
        var variableArgName = buildDefinition.ParamDefinitions[variableName].Attribute.ArgName;
        
        foreach (var provider in _customProviders)
            if (await provider.WriteVariable(variableArgName, variableValue))
                return;
        
        await _baseProvider.WriteVariable(variableArgName, variableValue);
    }
    
    public async Task ReadVariable(string jobName, string variableName)
    {
        var variableArgName = buildDefinition.ParamDefinitions[variableName].Attribute.ArgName;
        
        foreach (var provider in _customProviders)
            if (await provider.ReadVariable(jobName, variableArgName))
                return;
        
        await _baseProvider.ReadVariable(jobName, variableArgName);
    }
}