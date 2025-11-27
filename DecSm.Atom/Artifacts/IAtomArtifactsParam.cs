namespace DecSm.Atom.Artifacts;

/// <summary>
///     Provides shared parameters for working with artifacts within the Atom framework.
/// </summary>
/// <remarks>
///     This interface defines a common parameter used by internal Atom code to determine which artifacts
///     to store or retrieve. It is typically implemented by other artifact-related interfaces such as
///     <see cref="IRetrieveArtifact" /> and <see cref="IStoreArtifact" />.
///     The <see cref="AtomArtifacts" /> parameter allows specifying one or more artifact names, often
///     passed via command-line arguments or environment variables in CI/CD pipelines.
/// </remarks>
/// <example>
///     In a GitHub workflow, the <c>atom-artifacts</c> environment variable is used to specify artifacts:
///     <code>
/// // From .github/workflows/Test_BuildWithCustomArtifacts.yml
/// - name: StoreArtifact
///   run: dotnet run --project _atom/_atom.csproj StoreArtifact --skip --headless
///   env:
///     # ... other environment variables ...
///     atom-artifacts: DecSm.Atom,DecSm.Atom.Tool
/// </code>
///     This would make "DecSm.Atom" and "DecSm.Atom.Tool" available to the <see cref="AtomArtifacts" /> parameter.
/// </example>
[TargetDefinition]
public partial interface IAtomArtifactsParam
{
    /// <summary>
    ///     Gets the names of the artifact(s) to be processed.
    /// </summary>
    /// <remarks>
    ///     This parameter is typically used to specify which artifacts should be stored or retrieved.
    ///     Multiple artifact names can be provided, usually separated by commas when passed as a
    ///     command-line argument or environment variable (e.g., "artifact1,artifact2,artifact3").
    ///     The Atom framework parses this comma-separated string into an array of strings.
    ///     If no value is provided, it defaults to an empty array.
    /// </remarks>
    /// <example>
    ///     If the <c>--atom-artifacts</c> command-line argument is <c>"MyPackage,MySymbols"</c>,
    ///     this property will return <c>["MyPackage", "MySymbols"]</c>.
    /// </example>
    [ParamDefinition("atom-artifacts",
        "The name of the artifact/s to work with, use ',' to separate multiple artifacts")]
    string[] AtomArtifacts => GetParam(() => AtomArtifacts, []);
}
