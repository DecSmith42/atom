namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Represents the default implementation of a build definition, providing core
///     functionality and structure for build-related operations. This abstract class
///     serves as a foundation for defining build targets, parameters, and workflows
///     while implementing various build-related interfaces such as
///     <see cref="ISetupBuildInfo" />, <see cref="IValidateBuild" />, and
///     <see cref="IDotnetUserSecrets" />.
/// </summary>
/// <remarks>
///     The <see cref="DefaultBuildDefinition" /> class inherits from <see cref="BuildDefinition" />
///     and extends its functionality by incorporating additional interfaces to enable
///     setup, validation, and management of user secrets during build processes. This
///     class is designed to be further derived by specific build implementations.
/// </remarks>
/// <seealso cref="BuildDefinition" />
/// <seealso cref="ISetupBuildInfo" />
/// <seealso cref="IValidateBuild" />
/// <seealso cref="IDotnetUserSecrets" />
[PublicAPI]
public abstract class DefaultBuildDefinition(IServiceProvider services)
    : BuildDefinition(services), ISetupBuildInfo, IValidateBuild, IDotnetUserSecrets;
