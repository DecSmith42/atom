using DecSm.Atom.Variables;

namespace DecSm.Atom.GithubWorkflows;

public class GithubVariableProvider(IFileSystem fileSystem, ILogger<GithubVariableProvider> logger)
    : IWorkflowVariableProvider<GithubWorkflowType>
{
    public async Task WriteVariable(string variableName, string variableValue)
    {
        var githubOutputPath = Github.Variables.Output;
        
        if (githubOutputPath is null)
            throw new(
                $"{Github.VariableNames.Output} environment variable not set. This should not occur when running in a GitHub workflow");
        
        logger.LogInformation("Writing variable {VariableName} with value {VariableValue} to {GithubOutputPath}",
            variableName,
            variableValue,
            githubOutputPath);
        
        await fileSystem.File.AppendAllTextAsync(githubOutputPath, $"{variableName}={variableValue}");
    }
    
    public Task ReadVariable(string jobName, string variableName) =>
        Task.CompletedTask;
}