namespace DecSm.Atom.Workflows.Options;

/// <summary>
///     Represents a step in a workflow that sets up a specific .NET SDK version.
/// </summary>
/// <param name="DotnetVersion">
///     The .NET SDK version to install and use for later steps.
///     If null or empty, the default .NET version configured for the environment will be used.
/// </param>
/// <param name="Quality">
///     The .NET SDK version quality to install.
///     If null, the default quality will be used.
/// </param>
/// <remarks>
///     This record is typically used within a collection of workflow options to customize the .NET environment.
/// </remarks>
[PublicAPI]
public sealed record SetupDotnetStep(string? DotnetVersion = null, SetupDotnetStep.DotnetQuality? Quality = null)
    : CustomStep
{
    [PublicAPI]
    public enum DotnetQuality
    {
        Daily,
        Signed,
        Validated,
        Preview,
        Ga,
    }
}
