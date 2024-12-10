// TODO: Improve documentation for BuildDefinition

namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Represents the base class for defining build processes within the Atom framework.
///     This class provides the necessary infrastructure to define workflows, parameters, and targets
///     for a build process, leveraging dependency injection for service access.
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
/// <param name="services">The service provider used to resolve dependencies.</param>
[PublicAPI]
public abstract class BuildDefinition(IServiceProvider services) : IBuildInfo
{
    /// <summary>
    ///     Gets a dictionary of target definitions, where each target is identified by a unique name.
    ///     Targets define specific build steps or actions that can be executed.
    /// </summary>
    public abstract IReadOnlyDictionary<string, Target> TargetDefinitions { get; }

    /// <summary>
    ///     Gets a dictionary of parameter definitions, where each parameter is identified by a unique name.
    ///     Parameters allow for configuration and customization of the build process.
    /// </summary>
    public abstract IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions { get; }

    /// <summary>
    ///     Gets the service provider used to resolve services required by the build definition.
    /// </summary>
    public IServiceProvider Services => services;

    /// <summary>
    ///     Gets the list of workflow definitions that define the sequence of steps to be executed in the build process.
    ///     By default, this returns an empty list and should be overridden in derived classes.
    /// </summary>
    public virtual IReadOnlyList<WorkflowDefinition> Workflows => [];

    /// <summary>
    ///     Gets the default workflow options that apply to all workflows in the build process.
    ///     By default, this returns an empty list and can be overridden to provide specific options.
    /// </summary>
    public virtual IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions => [];

    // TODO: improve documentation for expression
    //
    /// <summary>
    ///     Retrieves a parameter value using the specified expression, with an optional default value and converter.
    /// </summary>
    /// <typeparam name="T">The type of the parameter value.</typeparam>
    /// <param name="parameterExpression">An expression identifying the parameter..</param>
    /// <param name="defaultValue">The default value to return if the parameter is not set.</param>
    /// <param name="converter">An optional function to convert the parameter value from a string.</param>
    /// <returns>The parameter value, or the default value if not set.</returns>
    public T? GetParam<T>(Expression<Func<T?>> parameterExpression, T? defaultValue = default, Func<string?, T?>? converter = null) =>
        Services
            .GetRequiredService<IParamService>()
            .GetParam(parameterExpression, defaultValue, converter);

    /// <summary>
    ///     Writes a variable to the workflow context, making it available for subsequent steps.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="value">The value of the variable.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task WriteVariable(string name, string value) =>
        Services
            .GetRequiredService<IWorkflowVariableService>()
            .WriteVariable(name, value);

    /// <summary>
    ///     Adds report data to the build process, which can be used for generating reports or logs.
    /// </summary>
    /// <param name="reportData">The report data to add.</param>
    public void AddReportData(IReportData reportData) =>
        Services
            .GetRequiredService<ReportService>()
            .AddReportData(reportData);
}
