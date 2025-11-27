namespace DecSm.Atom.Variables;

internal sealed class AtomWorkflowVariableProvider(IAtomFileSystem fileSystem, BuildModel buildModel)
    : IWorkflowVariableProvider
{
    public async Task<bool> WriteVariable(
        string variableName,
        string variableValue,
        CancellationToken cancellationToken = default)
    {
        var variablesPath = fileSystem.AtomTempDirectory / "variables";

        Dictionary<string, string>? variables;

        if (variablesPath.FileExists)
        {
            var text = await fileSystem.File.ReadAllTextAsync(variablesPath, cancellationToken);
            variables = JsonSerializer.Deserialize<Dictionary<string, string>>(text) ?? [];
        }
        else
        {
            variables = new();
        }

        var jobScopedVariableName = $"{buildModel.CurrentTarget!.Name}:{variableName}";
        variables[jobScopedVariableName] = variableValue;

        await fileSystem.File.WriteAllTextAsync(variablesPath, JsonSerializer.Serialize(variables), cancellationToken);

        return true;
    }

    public async Task<bool> ReadVariable(
        string jobName,
        string variableName,
        CancellationToken cancellationToken = default)
    {
        var variablesPath = fileSystem.AtomTempDirectory / "variables";

        if (!variablesPath.FileExists)
            return false;

        var text = await fileSystem.File.ReadAllTextAsync(variablesPath, cancellationToken);

        var variables = JsonSerializer.Deserialize<Dictionary<string, string>>(text) ?? [];

        var jobScopedVariableName = $"{jobName}:{variableName}";

        if (!variables.TryGetValue(jobScopedVariableName, out var variable))
            return true;

        Environment.SetEnvironmentVariable(variableName, variable);

        return true;
    }
}
