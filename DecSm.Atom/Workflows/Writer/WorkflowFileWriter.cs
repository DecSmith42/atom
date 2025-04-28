namespace DecSm.Atom.Workflows.Writer;

[PublicAPI]
public abstract class WorkflowFileWriter<T>(IAtomFileSystem fileSystem, ILogger<WorkflowFileWriter<T>> logger) : IWorkflowWriter<T>
    where T : IWorkflowType
{
    private readonly StringBuilder _stringBuilder = new();

    private int IndentLevel { get; set; }

    protected virtual int TabSize => 2;

    protected virtual RootedPath FileLocation => fileSystem.AtomRootDirectory;

    protected abstract string FileExtension { get; }

    public async Task Generate(WorkflowModel workflow)
    {
        var filePath = FileLocation / $"{workflow.Name}.{FileExtension}";

        WriteWorkflow(workflow);

        var newText = _stringBuilder.ToString();
        _stringBuilder.Clear();

        var existingText = fileSystem.File.Exists(filePath)
            ? await fileSystem.File.ReadAllTextAsync(filePath)
            : string.Empty;

        if (existingText == newText)
            return;

        switch (existingText.Length)
        {
            case > 0:
                logger.LogInformation("Updating workflow file: {FilePath}", filePath);

                break;
            default:
                logger.LogInformation("Writing new workflow file: {FilePath}", filePath);

                break;
        }

        if (!fileSystem.Directory.Exists(FileLocation))
            fileSystem.Directory.CreateDirectory(FileLocation);

        await fileSystem.File.WriteAllTextAsync(filePath, newText);
    }

    public async Task<bool> CheckForDirtyWorkflow(WorkflowModel workflow)
    {
        var filePath = FileLocation / $"{workflow.Name}.{FileExtension}";

        WriteWorkflow(workflow);

        var newText = _stringBuilder
            .ToString()
            .ReplaceLineEndings();

        _stringBuilder.Clear();

        var existingText = fileSystem.File.Exists(filePath)
            ? await fileSystem.File.ReadAllTextAsync(filePath)
            : string.Empty;

        if (existingText
            .ReplaceLineEndings()
            .Equals(newText.ReplaceLineEndings(), StringComparison.CurrentCulture))
            return false;

        logger.LogInformation("Workflow file is dirty and needs to be regenerated: {FilePath}\nExisting:\n{Existing}\nNew:\n{New}",
            filePath,
            existingText,
            newText);

        return true;
    }

    protected void WriteLine(string? value = null)
    {
        if (IndentLevel > 0)
            _stringBuilder.Append(new string(' ', IndentLevel));

        _stringBuilder.AppendLine(value);
    }

    protected IDisposable WriteSection(string header)
    {
        WriteLine(header);
        IndentLevel += TabSize;

        return new DisposableAction(() => IndentLevel -= TabSize);
    }

    protected abstract void WriteWorkflow(WorkflowModel workflow);
}
