namespace DecSm.Atom.Artifacts;

/// <summary>
///     Interface for artifact providers.
/// </summary>
/// <remarks>
///     An artifact provider is responsible for managing the storage and retrieval of build artifacts.
///     Implementations of this interface can provide functionality to upload, download, and clean up artifacts.
/// </remarks>
[PublicAPI]
public interface IArtifactProvider
{
    /// <summary>
    ///     Parameters that are required for the artifact provider to function.
    ///     The provider may not function correctly or at all if these parameters are not provided.
    /// </summary>
    /// <remarks>
    ///     These parameters are typically configuration settings or credentials required to access the artifact storage.
    /// </remarks>
    IReadOnlyList<string> RequiredParams => [];

    /// <summary>
    ///     Uploads the specified artifacts to the storage.
    /// </summary>
    /// <remarks>
    ///     Artifacts to be stored will be in <see cref="IAtomFileSystem.AtomPublishDirectory" />.
    /// </remarks>
    /// <param name="artifactNames">The names of the artifacts to upload.</param>
    /// <param name="buildId">The build ID associated with the artifacts. If not provided, a default build ID will be used.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StoreArtifacts(IReadOnlyList<string> artifactNames, string? buildId = null);

    /// <summary>
    ///     Downloads the specified artifacts from the storage.
    /// </summary>
    /// <remarks>
    ///     Artifacts to be retrieved are expected to be placed in <see cref="IAtomFileSystem.AtomArtifactsDirectory" />.
    /// </remarks>
    /// <param name="artifactNames">The names of the artifacts to download.</param>
    /// <param name="buildId">The build ID associated with the artifacts. If not provided, a default build ID will be used.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RetrieveArtifacts(IReadOnlyList<string> artifactNames, string? buildId = null);

    /// <summary>
    ///     Downloads a specific artifact for the given build IDs.
    /// </summary>
    /// <remarks>
    ///     Artifacts to be retrieved are expected to be placed in <see cref="IAtomFileSystem.AtomArtifactsDirectory" />.
    /// </remarks>
    /// <param name="artifactName">The name of the artifact to download.</param>
    /// <param name="buildIds">The list of build IDs to download the artifact for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RetrieveArtifact(string artifactName, IReadOnlyList<string> buildIds);

    /// <summary>
    ///     Cleans up artifacts for the specified build IDs.
    /// </summary>
    /// <param name="buildIds">The list of build IDs to clean up artifacts for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Cleanup(IReadOnlyList<string> buildIds);

    /// <summary>
    ///     Retrieves the stored build IDs from the storage.
    /// </summary>
    /// <param name="artifactName">The name of the artifact to filter the build IDs by. If not provided, all build IDs will be retrieved.</param>
    /// <returns>A task representing the asynchronous operation, with a list of stored build IDs as the result.</returns>
    Task<IReadOnlyList<string>> GetStoredBuildIds(string? artifactName = null);
}
