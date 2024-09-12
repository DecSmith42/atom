namespace DecSm.Atom.Extensions.Dotnet;

public sealed record CoverageModel
{
    [JsonPropertyName("summary")]
    public CoverageSummary Summary { get; init; } = new();
}

public sealed record CoverageSummary
{
    [JsonPropertyName("generatedon")]
    public DateTime GeneratedOn { get; init; }

    [JsonPropertyName("parser")]
    public string Parser { get; init; } = string.Empty;

    [JsonPropertyName("assemblies")]
    public int Assemblies { get; init; }

    [JsonPropertyName("classes")]
    public int Classes { get; init; }

    [JsonPropertyName("files")]
    public int Files { get; init; }

    [JsonPropertyName("coveredlines")]
    public int CoveredLines { get; init; }

    [JsonPropertyName("uncoveredlines")]
    public int UncoveredLines { get; init; }

    [JsonPropertyName("coverablelines")]
    public int CoverableLines { get; init; }

    [JsonPropertyName("totallines")]
    public int TotalLines { get; init; }

    [JsonPropertyName("linecoverage")]
    public double LineCoverage { get; init; }

    [JsonPropertyName("coveredbranches")]
    public int CoveredBranches { get; init; }

    [JsonPropertyName("totalbranches")]
    public int TotalBranches { get; init; }

    [JsonPropertyName("branchcoverage")]
    public double BranchCoverage { get; init; }

    [JsonPropertyName("coveredmethods")]
    public int CoveredMethods { get; init; }

    [JsonPropertyName("totalmethods")]
    public int TotalMethods { get; init; }

    [JsonPropertyName("methodcoverage")]
    public double MethodCoverage { get; init; }
}
