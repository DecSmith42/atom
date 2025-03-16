namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Represents the primary interface for defining build processes within the Atom framework.
///     Recommended to inherit from specialized implementations such as <see cref="BuildDefinition" /> or
///     <see cref="DefaultBuildDefinition" /> instead of directly using this interface.
/// </summary>
/// <example>
///     <code>
/// [BuildDefinition]
/// internal partial class Build : BuildDefinition
/// {
///     public override IReadOnlyList&lt;WorkflowDefinition&gt; Workflows =>
///     [
///         new("Build")
///         {
///             Triggers = [...],
///             StepDefinitions =
///             [
///                 Commands.Setup,
///                 Commands.BuildProject,
///                 Commands.PushToNuget,
///             ],
///             WorkflowTypes = [...],
///         },
///         new("OtherWorkflow")
///         {
///             ...
///         },
///     ];
/// }
/// </code>
/// </example>
[PublicAPI]
public interface IBuildDefinition
{
    /// <summary>
    ///     All workflows defined for a build. Inheritors should override this property to provide a list of workflows.
    /// </summary>
    IReadOnlyList<WorkflowDefinition> Workflows => [];

    /// <summary>
    ///     All targets defined for a build. Inheritors should override this property to provide a list of targets.
    /// </summary>
    IReadOnlyDictionary<string, Target> TargetDefinitions { get; }

    /// <summary>
    ///     All parameters defined for a build. Inheritors should override this property to provide a list of parameters.
    /// </summary>
    IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions { get; }

    /// <summary>
    ///     The service provider used to resolve services required by the build definition.
    /// </summary>
    IServiceProvider Services { get; }

    /// <summary>
    ///     Workflow options that are applied to all targets in all workflows.
    /// </summary>
    IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions => [];

    /// <summary>
    ///     Retrieves a parameter value using the specified expression, with an optional default value and converter.
    /// </summary>
    /// <typeparam name="T">The type of the parameter value.</typeparam>
    /// <param name="parameterExpression">An expression identifying the parameter..</param>
    /// <param name="defaultValue">The default value to return if the parameter is not set.</param>
    /// <param name="converter">An optional function to convert the parameter value from a string.</param>
    /// <returns>The parameter value, or the default value if not set.</returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    T? GetParam<T>(Expression<Func<T?>> parameterExpression, T? defaultValue = default, Func<string?, T?>? converter = null);

    /// <summary>
    ///     Writes a variable to the workflow context, making it available for subsequent steps.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="value">The value of the variable.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task WriteVariable(string name, string value);

    /// <summary>
    ///     Adds report data to the build process, which can be used for generating reports or logs.
    ///     If running locally, the report will be output to the console.
    ///     If running as part of a CI/CD pipeline, the report may be attached to the CI/CD run summary.
    /// </summary>
    /// <param name="reportData">The report data to add.</param>
    void AddReportData(IReportData reportData);

    /// <summary>
    ///     Shortcut for getting a service from the <see cref="Services" /> service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve.</typeparam>
    /// <returns>The service instance.</returns>
    T GetService<T>()
        where T : notnull =>
        typeof(T).GetInterface(nameof(IBuildDefinition)) != null
            ? (T)this
            : Services.GetRequiredService<T>();

    /// <summary>
    ///     Retrieves all registered services of the specified type from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of the services to retrieve.</typeparam>
    /// <returns>A collection of instances of the specified type.</returns>
    IEnumerable<T> GetServices<T>()
        where T : notnull =>
        typeof(T).GetInterface(nameof(IBuildDefinition)) != null
            ? [(T)this]
            : Services.GetServices<T>();

    /// <summary>
    ///     Called by the Atom framework to register services required by the build definition.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    static virtual void Register(IServiceCollection services) { }
}
