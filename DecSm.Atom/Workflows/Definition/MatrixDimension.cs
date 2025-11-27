namespace DecSm.Atom.Workflows.Definition;

/// <summary>
///     Represents a dimension in a build matrix, used to define variations for a workflow target.
/// </summary>
/// <param name="Name">
///     The name of the matrix dimension. This is often derived from an interface property like
///     <c>nameof(IJobRunsOn.JobRunsOn)</c>.
/// </param>
/// <remarks>
///     A <c>MatrixDimension</c> is typically used in conjunction with <c>WorkflowTargetDefinition</c> methods like
///     <c>WithMatrixDimensions</c>
///     or <c>WithAddedMatrixDimensions</c> to configure build matrixes.
///     Each instance defines a named set of possible values, and a workflow target can have multiple such dimensions.
///     For example, one dimension could be "OperatingSystem" with values ["Windows", "Linux", "MacOS"], and another could
///     be "DotNetVersion"
///     with values ["net8.0", "net9.0"].
///     This allows for running the same set of tasks (jobs) across different configurations.
///     The <c>Name</c> parameter is crucial for identifying the dimension, and the <c>Values</c> parameter provides the
///     set of variations for
///     that dimension.
/// </remarks>
[PublicAPI]
public record MatrixDimension(string Name)
{
    /// <summary>
    ///     A read-only list of string values for this dimension. These values represent the different options for this
    ///     dimension,
    ///     for example, different operating systems like <c>IJobRunsOn.WindowsLatestTag</c> or
    ///     <c>IJobRunsOn.UbuntuLatestTag</c>.
    /// </summary>
    public IReadOnlyList<string> Values { get; init; } = [];
}
