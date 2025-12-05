namespace DecSm.Atom.Workflows.Writer;

/// <summary>
///     An abstract base class for generating platform-specific workflow files (e.g., for GitHub Actions or Azure DevOps).
/// </summary>
/// <typeparam name="T">The workflow type this writer handles, which must implement <see cref="IWorkflowType" />.</typeparam>
/// <param name="fileSystem">The file system service for file operations.</param>
/// <param name="logger">The logger for diagnostics.</param>
/// <remarks>
///     This class provides a template for writing structured workflow files, handling file I/O, change detection,
///     and indentation. Concrete implementations must override <see cref="WriteWorkflow" /> to define the specific
///     file format and <see cref="FileExtension" /> to specify the file extension.
/// </remarks>
[PublicAPI]
public abstract class WorkflowFileWriter<T>(IAtomFileSystem fileSystem, ILogger<WorkflowFileWriter<T>> logger)
    : IWorkflowWriter<T>
    where T : IWorkflowType
{
    private readonly StringBuilder _stringBuilder = new();

    /// <summary>
    ///     Gets the current indentation level for formatting nested content.
    /// </summary>
    private int IndentLevel { get; set; }

    /// <summary>
    ///     Gets the number of spaces to use for each indentation level. Defaults to 2.
    /// </summary>
    protected virtual int TabSize => 2;

    /// <summary>
    ///     Gets the root directory where workflow files should be generated. Defaults to the Atom root directory.
    /// </summary>
    protected virtual RootedPath FileLocation => fileSystem.AtomRootDirectory;

    /// <summary>
    ///     Gets the file extension for the generated workflow file (e.g., "yml").
    /// </summary>
    protected abstract string FileExtension { get; }

    /// <summary>
    ///     Generates a workflow file from the provided model, writing it only if the content has changed.
    /// </summary>
    /// <param name="workflow">The workflow model to generate.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task Generate(WorkflowModel workflow, CancellationToken cancellationToken = default)
    {
        var filePath = FileLocation / $"{workflow.Name}.{FileExtension}";

        WriteWorkflow(workflow);

        var newText = _stringBuilder.ToString();
        _stringBuilder.Clear();

        var existingText = fileSystem.File.Exists(filePath)
            ? await fileSystem.File.ReadAllTextAsync(filePath, cancellationToken)
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

        await fileSystem.File.WriteAllTextAsync(filePath, newText, cancellationToken);
    }

    /// <summary>
    ///     Checks if the existing workflow file is outdated and needs to be regenerated.
    /// </summary>
    /// <param name="workflow">The workflow model to compare against the existing file.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the workflow file is missing or outdated; otherwise, <c>false</c>.</returns>
    public async Task<bool> CheckForDirtyWorkflow(WorkflowModel workflow, CancellationToken cancellationToken = default)
    {
        var filePath = FileLocation / $"{workflow.Name}.{FileExtension}";

        WriteWorkflow(workflow);

        var newText = _stringBuilder
            .ToString()
            .ReplaceLineEndings();

        _stringBuilder.Clear();

        var existingText = fileSystem.File.Exists(filePath)
            ? await fileSystem.File.ReadAllTextAsync(filePath, cancellationToken)
            : string.Empty;

        if (existingText
            .ReplaceLineEndings()
            .Equals(newText.ReplaceLineEndings(), StringComparison.CurrentCulture))
            return false;

        logger.LogInformation(
            "Workflow file is dirty and needs to be regenerated: {FilePath}\nExisting:\n{Existing}\nNew:\n{New}",
            filePath,
            existingText,
            newText);

        return true;
    }

    /// <summary>
    ///     Writes a line of text to the output with the current indentation.
    /// </summary>
    /// <param name="value">The text to write. If null, an empty line is written.</param>
    protected void WriteLine(string? value = null)
    {
        if (IndentLevel > 0)
            _stringBuilder.Append(new string(' ', IndentLevel));

        _stringBuilder.AppendLine(value);
    }

    /// <summary>
    ///     Writes a section header and returns a disposable scope that manages indentation for the section's content.
    /// </summary>
    /// <param name="header">The header text for the section.</param>
    /// <returns>A disposable object that decreases the indentation level upon disposal.</returns>
    /// <example>
    ///     <code>
    /// using (WriteSection("jobs:"))
    /// {
    ///     WriteLine("build:");
    /// }
    ///     </code>
    /// </example>
    protected IDisposable WriteSection(string header)
    {
        WriteLine(header);
        IndentLevel += TabSize;

        return new ActionScope(() => IndentLevel -= TabSize);
    }

    /// <summary>
    ///     When overridden in a derived class, writes the content of the workflow file using the provided helper methods.
    /// </summary>
    /// <param name="workflow">The workflow model to write.</param>
    protected abstract void WriteWorkflow(WorkflowModel workflow);
}
