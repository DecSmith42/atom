using DecSm.Atom.Build;

namespace Atom;

public sealed class GitVersionBuildIdProvider(IProcessRunner processRunner) : IBuildIdProvider
{
    private string? _buildId;

    public string BuildId
    {
        get
        {
            if (_buildId is not null)
                return _buildId;

            var output = processRunner.RunProcess("dotnet", "gitversion");
            var jsonOutput = JsonSerializer.Deserialize<JsonElement>(output);

            var buildId = jsonOutput
                .GetProperty("SemVer")
                .GetString();

            if (buildId is null)
                throw new InvalidOperationException("Failed to determine build ID");

            return _buildId = buildId;
        }
    }
}