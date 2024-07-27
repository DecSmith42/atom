using DecSm.Atom.Process;

namespace DecSm.Atom.Extensions.GitVersioning;

public sealed class GitVersioningBuildIdProvider(IProcessRunner processRunner) : IBuildIdProvider
{
    private string? _buildId;

    public string BuildId
    {
        get
        {
            if (_buildId is not null)
                return _buildId;

            processRunner.RunProcess("dotnet", "tool install --global nbgv");

            var output = processRunner.RunProcess("nbgv", "get-version -f json");
            var jsonOutput = JsonSerializer.Deserialize<JsonElement>(output);

            var buildId = jsonOutput
                .GetProperty("NuGetPackageVersion")
                .GetString();

            if (buildId is null)
                throw new InvalidOperationException("Failed to determine build ID");

            return _buildId = buildId;
        }
    }
}