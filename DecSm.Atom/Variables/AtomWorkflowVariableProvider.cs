namespace DecSm.Atom.Variables;

internal sealed partial class AtomWorkflowVariableProvider(IAtomFileSystem fileSystem, BuildModel buildModel)
    : IWorkflowVariableProvider
{
    [JsonSerializable(typeof(Dictionary<string, string>))]
    internal partial class AppJsonContext : JsonSerializerContext;

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
            variables = JsonSerializer.Deserialize(text, AppJsonContext.Default.DictionaryStringString) ?? [];
        }
        else
        {
            variables = new();
        }

        var jobScopedVariableName = $"{buildModel.CurrentTarget!.Name}:{variableName}";
        variables[jobScopedVariableName] = variableValue;
        var json = JsonSerializer.Serialize(variables, AppJsonContext.Default.DictionaryStringString);

        await fileSystem.File.WriteAllTextAsync(variablesPath, json, cancellationToken);

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

        var variables = JsonSerializer.Deserialize(text, AppJsonContext.Default.DictionaryStringString) ?? [];

        var jobScopedVariableName = $"{jobName}:{variableName}";

        if (!variables.TryGetValue(jobScopedVariableName, out var variable))
            return true;

        Environment.SetEnvironmentVariable(variableName, variable);

        return true;
    }
}
