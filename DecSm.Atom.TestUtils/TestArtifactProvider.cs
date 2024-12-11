﻿namespace DecSm.Atom.TestUtils;

public sealed class TestArtifactProvider : IArtifactProvider
{
    public Task StoreArtifacts(IReadOnlyList<string> artifactNames, string? buildId = null, string? buildSlice = null) =>
        throw new NotImplementedException();

    public Task RetrieveArtifacts(IReadOnlyList<string> artifactNames, string? buildId = null, string? buildSlice = null) =>
        throw new NotImplementedException();

    public Task RetrieveArtifact(string artifactName, IReadOnlyList<string> buildIds, string? buildSlice = null) =>
        throw new NotImplementedException();

    public Task Cleanup(IReadOnlyList<string> runIdentifiers) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<string>> GetStoredRunIdentifiers(string? artifactName = null, string? buildSlice = null) =>
        throw new NotImplementedException();
}
