namespace DecSm.Atom.GithubWorkflows;

public class GithubVariableProvider(IFileSystem fileSystem) : IWorkflowVariableProvider<GithubWorkflowType>
{
    public async Task WriteVariable(string variableName, string variableValue)
    {
        var githubOutputPath = Github.Variables.Output;
        
        if (githubOutputPath is null)
            throw new(
                $"{Github.VariableNames.Output} environment variable not set. This should not occur when running in a GitHub workflow");
        
        await fileSystem.File.AppendAllTextAsync(githubOutputPath, $"{variableName}={variableValue}");
    }
    
    public Task ReadVariable(string jobName, string variableName) =>
        Task.CompletedTask;
}