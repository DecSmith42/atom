﻿namespace DecSm.Atom.Artifacts;

/// <summary>
///     Provides shared params for working with artifacts.
/// </summary>
/// <remarks>
///     Used by internal Atom code to determine artifacts to store/retrieve.
/// </remarks>
[TargetDefinition]
public partial interface IArtifactHelper : IBuildInfo
{
    /// <summary>
    ///     The name of the artifact/s to work with.
    /// </summary>
    [ParamDefinition("atom-artifacts", "The name of the artifact/s to work with, use ',' to separate multiple artifacts")]
    string[] AtomArtifacts => GetParam(() => AtomArtifacts, []);
}
