namespace DecSm.Atom.Workflows.Writer;

public abstract class WorkflowFileWriter<T>(IFileSystem fileSystem, ILogger<WorkflowFileWriter<T>> logger) : IAtomWorkflowWriter<T>
    where T : IWorkflowType
{
    private readonly StringBuilder _stringBuilder = new();

    private int IndentLevel { get; set; }

    protected virtual int TabSize => 2;

    protected virtual AbsolutePath FileLocation => fileSystem.SolutionRoot();

    protected abstract string FileExtension { get; }

    public void Generate(WorkflowModel workflow)
    {
        var filePath = FileLocation / $"{workflow.Name}.{FileExtension}";

        WriteWorkflow(workflow);

        var newText = _stringBuilder.ToString();
        _stringBuilder.Clear();

        var existingText = fileSystem.File.Exists(filePath)
            ? fileSystem.File.ReadAllText(filePath)
            : string.Empty;

        if (existingText == newText)
        {
            logger.LogDebug("Workflow file is up to date: {FilePath}", filePath);

            return;
        }

        if (existingText.Length > 0)
            logger.LogInformation("Updating workflow file: {FilePath}", filePath);
        else
            logger.LogInformation("Writing new workflow file: {FilePath}", filePath);

        if (filePath.Parent?.Exists is false)
            fileSystem.Directory.CreateDirectory(filePath.Parent);

        fileSystem.File.WriteAllText(filePath, newText);
    }

    protected void WriteLine(string? value = null)
    {
        if (IndentLevel > 0)
        {
            _stringBuilder.Append(new string(' ', IndentLevel));
            _stringBuilder.AppendLine(value);
        }
        else
        {
            _stringBuilder.AppendLine(value);
        }
    }

    protected IDisposable WriteSection(string header)
    {
        WriteLine(header);
        IndentLevel += TabSize;

        return new Disposable(() => IndentLevel -= TabSize);
    }

    protected abstract void WriteWorkflow(WorkflowModel workflow);
}