namespace DecSm.Atom.Build.Definition;

/// <summary>
///     An attribute used to indicate that a class should have an entry point generated for it.
///     This is typically used in conjunction with build definitions to automatically create
///     a main method that serves as the entry point for the application.
/// </summary>
/// <remarks>
///     If used on the build definition, the following code will be generated:
///     <code>
/// // &lt;auto-generated/&gt;
/// DecSm.Atom.Hosting.AtomHost.Run&lt;TBuildType&gt;(args);
/// </code>
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateEntryPointAttribute : Attribute;
