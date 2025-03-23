namespace DecSm.Atom.Build.Definition;

/// <summary>
///     A delegate that can apply configuration to (build) a target definition.
/// </summary>
[PublicAPI]
public delegate TargetDefinition Target(TargetDefinition definition);
