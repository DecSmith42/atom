namespace DecSm.Atom.Build.Definition;

public interface IBuildDefinition
{
    [ParamDefinition("atom-build-name", "Name of the build", "Solution name if provided, otherwise the root directory name")]
    string AtomBuildName => GetParam(() => AtomBuildName, DefaultBuildName);

    [ParamDefinition("matrix-slice", "Unique identifier for the combination of matrix parameters for this job")]
    string MatrixSlice => GetParam(() => MatrixSlice)!;

    IReadOnlyList<WorkflowDefinition> Workflows => [];

    IReadOnlyDictionary<string, Target> TargetDefinitions { get; }

    IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions { get; }

    IServiceProvider Services { get; }

    IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions => [];

    private string DefaultBuildName
    {
        get
        {
            var fileSystem = Services.GetRequiredService<IAtomFileSystem>();

            return fileSystem
                .Directory
                .GetFiles(fileSystem.AtomRootDirectory, "*.sln", SearchOption.TopDirectoryOnly)
                .FirstOrDefault() is { } solutionFile
                ? new AbsolutePath(fileSystem, solutionFile).FileName![..^4]
                : fileSystem.AtomRootDirectory.DirectoryName!;
        }
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    T? GetParam<T>(Expression<Func<T?>> parameterExpression, T? defaultValue = default, Func<string?, T?>? converter = null);

    Task WriteVariable(string name, string value);

    void AddReportData(IReportData reportData);

    T GetService<T>()
        where T : notnull =>
        typeof(T).GetInterface(nameof(IBuildDefinition)) != null
            ? (T)this
            : Services.GetRequiredService<T>();

    IEnumerable<T> GetServices<T>()
        where T : notnull =>
        typeof(T).GetInterface(nameof(IBuildDefinition)) != null
            ? [(T)this]
            : Services.GetServices<T>();

    static virtual void Register(IServiceCollection services) { }
}
