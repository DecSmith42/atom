namespace DecSm.Atom.Build.Definition;

/// <summary>
///     An attribute used to mark a class as a build definition within the Atom framework.
///     This attribute is applied to classes that define the structure and behavior of a build process.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class BuildDefinitionAttribute : Attribute;
