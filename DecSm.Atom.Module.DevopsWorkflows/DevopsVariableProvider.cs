namespace DecSm.Atom.Module.DevopsWorkflows;

internal sealed class DevopsVariableProvider(ILogger<DevopsVariableProvider> logger) : IWorkflowVariableProvider
{
    public Task<bool> WriteVariable(string variableName, string variableValue, CancellationToken cancellationToken = default)
    {
        if (!Devops.IsDevopsPipelines)
            return Task.FromResult(false);

        logger.LogInformation("Writing variable {VariableName} with value {VariableValue} to Azure DevOps pipeline",
            variableName,
            variableValue);

        Console.WriteLine($"##vso[task.setvariable variable={variableName};isoutput=true;]{variableValue}");

        return Task.FromResult(true);
    }

    public Task<bool> ReadVariable(string jobName, string variableName, CancellationToken cancellationToken = default)
    {
        if (!Devops.IsDevopsPipelines)
            return Task.FromResult(false);

        var value = Environment.GetEnvironmentVariable(variableName);

        if (value == null)
        {
            logger.LogWarning("Variable {VariableName} not found in Azure DevOps pipeline", variableName);

            return Task.FromResult(false);
        }

        logger.LogInformation("Read variable {VariableName} with value {VariableValue} from Azure DevOps pipeline",
            variableName,
            value.SanitizeForLogging());

        return Task.FromResult(true);
    }
}
