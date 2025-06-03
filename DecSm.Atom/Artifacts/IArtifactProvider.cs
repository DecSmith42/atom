namespace DecSm.Atom.Artifacts;

/// <summary>
///     Interface for artifact providers within the Atom framework.
/// </summary>
/// <remarks>
///     An artifact provider is responsible for managing the storage and retrieval of build artifacts.
///     Implementations of this interface can provide functionality to upload, download, and clean up artifacts.
///     This allows the Atom framework to abstract the underlying artifact storage mechanism, supporting various
///     backends like Azure Blob Storage, GitHub Actions artifacts, or local file systems.
/// </remarks>
/// <example>
///     An example of how an artifact provider might be invoked within a workflow:
///     <code>
/// // In a GitHub Workflow YML file (Test_BuildWithCustomArtifacts.yml)
/// // This step would indirectly use an IArtifactProvider implementation
/// - name: StoreArtifact
///   run: dotnet run --project _atom/_atom.csproj StoreArtifact --skip --headless
///   env:
///     # ... environment variables for artifact provider configuration ...
///     atom-artifacts: DecSm.Atom
/// </code>
/// </example>
[PublicAPI]
public interface IArtifactProvider
{
    /// <summary>
    /// Gets a list of parameter names that are required for the artifact provider to function.
    /// The provider may not function correctly or at all if these parameters are not provided.
    /// </summary>
    /// <remarks>
    /// These parameters are typically configuration settings or credentials required to access the artifact storage.
    /// </remarks>
    IReadOnlyList<string> RequiredParams => [];

    /// <summary>
    /// Uploads the specified artifacts to the configured storage.
    /// </summary>
    /// <remarks>
    /// Artifacts to be stored are expected to be located in the directory specified by <see cref="IAtomFileSystem.AtomPublishDirectory" />.
    /// The Atom framework calls this method as part of targets like <see cref="IStoreArtifact.StoreArtifact"/>.
    /// </remarks>
    /// <param name="artifactNames">A list of names for the artifacts to be uploaded. Each name typically corresponds to a directory or a set of files.</param>
    /// <param name="buildId">The unique identifier for the current build. If not provided, the provider should attempt to use the current run's ID.</param>
    /// <param name="buildSlice">
    /// An optional identifier for a specific slice or variation of the build (e.g., for matrix builds).
    /// This helps in organizing artifacts when a build produces multiple variations.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous upload operation.</returns>
    /// <example>
    /// A build step might invoke this to store a compiled package:
    /// <code>
    /// // Target definition using IStoreArtifact
    /// Target StoreMyPackage => d => d
    ///     .Executes(async () => {
    ///         // ... logic to produce MyPackage.nupkg ...
    ///         // The IStoreArtifact target would then call provider.StoreArtifacts(["MyPackage"], buildId, buildSlice);
    ///         // where "MyPackage" corresponds to the artifact produced.
    ///     });
    /// </code>
    /// </example>
    Task StoreArtifacts(IReadOnlyList<string> artifactNames, string? buildId = null, string? buildSlice = null);

    /// <summary>
    /// Downloads the specified artifacts from the configured storage.
    /// </summary>
    /// <remarks>
    /// Artifacts that are retrieved are expected to be placed in the directory specified by <see cref="IAtomFileSystem.AtomArtifactsDirectory" />.
    /// The Atom framework calls this method as part of targets like <see cref="IRetrieveArtifact.RetrieveArtifact"/>.
    /// </remarks>
    /// <param name="artifactNames">A list of names for the artifacts to be downloaded.</param>
    /// <param name="buildId">The unique identifier of the build from which to retrieve the artifacts. If not provided, the current run's ID might be assumed or it might retrieve the latest.</param>
    /// <param name="buildSlice">An optional identifier for a specific slice or variation of the build from which to retrieve artifacts.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous download operation.</returns>
    /// <example>
    /// A build step might invoke this to retrieve a previously built package:
    /// <code>
    /// // Target definition using IRetrieveArtifact
    /// Target UseMyPackage => d => d
    ///     .DependsOn(RetrieveMyPackage) // RetrieveMyPackage would call provider.RetrieveArtifacts
    ///     .Executes(async () => {
    ///         // ... logic to use the retrieved "MyPackage" ...
    ///     });
    /// </code>
    /// </example>
    Task RetrieveArtifacts(IReadOnlyList<string> artifactNames, string? buildId = null, string? buildSlice = null);

    /// <summary>
    /// Cleans up (deletes) artifacts associated with the specified build run identifiers.
    /// </summary>
    /// <param name="runIdentifiers">A list of build run identifiers whose artifacts should be cleaned up.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous cleanup operation.</returns>
    /// <remarks>
    /// This is useful for managing storage space by removing old or temporary artifacts.
    /// An example target using this is <c>ICleanupPrereleaseArtifacts</c>.
    /// </remarks>
    Task Cleanup(IReadOnlyList<string> runIdentifiers);

    /// <summary>
    /// Retrieves a list of stored run identifiers from the artifact storage.
    /// </summary>
    /// <param name="artifactName">
    /// Optional. The name of the artifact to filter the run identifiers by. If not provided,
    /// all relevant run identifiers may be retrieved.
    /// </param>
    /// <param name="buildSlice">Optional. The build slice to filter run identifiers by.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a list of stored run identifiers as the result.</returns>
    /// <remarks>
    /// This can be used to list available builds or to find specific runs for retrieval or cleanup.
    /// The <c>AzureBlobArtifactProvider</c> uses this to find build IDs for cleanup.
    /// </remarks>
    Task<IReadOnlyList<string>> GetStoredRunIdentifiers(string? artifactName = null, string? buildSlice = null);
}
