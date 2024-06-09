namespace DecSm.Atom.Variables;

public sealed class BaseWorkflowVariableProvider(IFileSystem fileSystem, BuildModel buildModel) : IWorkflowVariableProvider
{
    public async Task<bool> WriteVariable(string variableName, string variableValue)
    {
        var variablesPath = fileSystem.TempDirectory() / "variables";
        
        Dictionary<string, string>? variables;
        
        if (variablesPath.FileExists)
        {
            var text = await fileSystem.File.ReadAllTextAsync(variablesPath);
            variables = JsonSerializer.Deserialize<Dictionary<string, string>>(text) ?? [];
        }
        else
        {
            variables = new();
        }
        
        var jobScopedVariableName = $"{buildModel.CurrentTarget!.Name}:{variableName}";
        variables[jobScopedVariableName] = variableValue;
        
        await fileSystem.File.WriteAllTextAsync(variablesPath, JsonSerializer.Serialize(variables));
        
        return true;
    }
    
    public async Task<bool> ReadVariable(string jobName, string variableName)
    {
        var variablesPath = fileSystem.TempDirectory() / "variables";
        
        if (!variablesPath.FileExists)
            return false;
        
        var text = await fileSystem.File.ReadAllTextAsync(variablesPath);
        
        var variables = JsonSerializer.Deserialize<Dictionary<string, string>>(text) ?? [];
        
        var jobScopedVariableName = $"{jobName}:{variableName}";
        
        if (!variables.TryGetValue(jobScopedVariableName, out var variable))
            return true;
        
        Environment.SetEnvironmentVariable(variableName, variable);
        
        return true;
    }
}