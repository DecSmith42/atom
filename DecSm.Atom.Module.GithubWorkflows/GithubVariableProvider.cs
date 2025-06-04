namespace DecSm.Atom.Module.GithubWorkflows;

internal sealed class GithubVariableProvider(IAtomFileSystem fileSystem, ILogger<GithubVariableProvider> logger) : IWorkflowVariableProvider
{
    public async Task<bool> WriteVariable(string variableName, string variableValue, CancellationToken cancellationToken = default)
    {
        if (!Github.IsGithubActions)
            return false;

        var githubOutputPath = Github.Variables.Output;

        if (githubOutputPath is null)
            throw new(
                $"{Github.VariableNames.Output} environment variable not set. This should not occur when running in a GitHub workflow");

        logger.LogInformation("Writing variable {VariableName} with value {VariableValue} to {GithubOutputPath}",
            variableName,
            variableValue,
            githubOutputPath);

        await fileSystem.File.AppendAllTextAsync(githubOutputPath,
            $"{variableName}={variableValue}{Environment.NewLine}",
            cancellationToken);

        return true;
    }

    public Task<bool> ReadVariable(string jobName, string variableName, CancellationToken cancellationToken = default) =>
        Task.FromResult(Github.IsGithubActions);
}
