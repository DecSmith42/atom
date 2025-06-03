namespace DecSm.Atom.Workflows.Writer;

/// <summary>
///     Abstract base class for generating workflow files of different types (GitHub Actions, Azure DevOps, Dependabot, etc.).
///     Provides a standardized approach to writing structured workflow files with proper indentation and change detection.
/// </summary>
/// <typeparam name="T">
///     The workflow type that implements <see cref="IWorkflowType" />. This constrains the writer to a specific workflow
///     format.
/// </typeparam>
/// <param name="fileSystem">File system abstraction for file operations and path management.</param>
/// <param name="logger">Logger instance for tracking file generation activities and debugging.</param>
/// <remarks>
///     <para>
///         This class implements a template method pattern where concrete implementations define the file extension
///         and workflow writing logic, while the base class handles file I/O, change detection, and formatting utilities.
///     </para>
///     <para>
///         Concrete implementations should override <see cref="WriteWorkflow" /> to define the specific workflow format
///         and <see cref="FileExtension" /> to specify the appropriate file extension (e.g., "yml", "yaml", "json").
///     </para>
/// </remarks>
/// <example>
///     <code>
/// public class MyWorkflowWriter : WorkflowFileWriter&lt;MyWorkflowType&gt;
/// {
///     protected override string FileExtension => "yml";
///     protected override void WriteWorkflow(WorkflowModel workflow)
///     {
///         WriteLine("name: " + workflow.Name);
///         using (WriteSection("jobs:"))
///         {
///             WriteLine("build:");
///             using (WriteSection("steps:"))
///             {
///                 WriteLine("- name: Checkout");
///             }
///         }
///     }
/// }
/// </code>
/// </example>
[PublicAPI]
public abstract class WorkflowFileWriter<T>(IAtomFileSystem fileSystem, ILogger<WorkflowFileWriter<T>> logger) : IWorkflowWriter<T>
    where T : IWorkflowType
{
    private readonly StringBuilder _stringBuilder = new();

    /// <summary>
    ///     Gets the current indentation level for nested content formatting.
    /// </summary>
    private int IndentLevel { get; set; }

    /// <summary>
    ///     Gets the number of spaces to use for each indentation level.
    /// </summary>
    /// <value>The default tab size is 2 spaces. Override in derived classes to customize indentation.</value>
    protected virtual int TabSize => 2;

    /// <summary>
    ///     Gets the root directory where workflow files should be generated.
    /// </summary>
    /// <value>Defaults to the Atom root directory. Override to customize the output location.</value>
    /// <remarks>
    ///     The directory will be created automatically if it doesn't exist during file generation.
    /// </remarks>
    protected virtual RootedPath FileLocation => fileSystem.AtomRootDirectory;

    /// <summary>
    ///     Gets the file extension to use for generated workflow files (without the leading dot).
    /// </summary>
    /// <value>File extension such as "yml", "yaml", "json", etc.</value>
    /// <remarks>
    ///     This property must be implemented by concrete classes to specify the appropriate
    ///     file format for the workflow type.
    /// </remarks>
    protected abstract string FileExtension { get; }

    /// <summary>
    ///     Generates or updates a workflow file based on the provided workflow model.
    /// </summary>
    /// <param name="workflow">The workflow model containing the configuration to generate.</param>
    /// <returns>A task representing the asynchronous file generation operation.</returns>
    /// <remarks>
    ///     <para>
    ///         This method performs the following operations:
    ///         1. Constructs the target file path using the workflow name and file extension
    ///         2. Generates the workflow content by calling <see cref="WriteWorkflow" />
    ///         3. Compares the new content with existing file content (if any)
    ///         4. Only writes the file if the content has changed to avoid unnecessary I/O
    ///         5. Creates the target directory if it doesn't exist
    ///         6. Logs whether a new file was created or an existing file was updated
    ///     </para>
    ///     <para>
    ///         The generated file will be named "{workflow.Name}.{FileExtension}" in the <see cref="FileLocation" /> directory.
    ///     </para>
    /// </remarks>
    /// <exception cref="DirectoryNotFoundException">Thrown if the file location cannot be created.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if there are insufficient permissions to write to the target location.</exception>
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

    /// <summary>
    ///     Checks if a workflow file needs to be regenerated by comparing current content with what would be generated.
    /// </summary>
    /// <param name="workflow">The workflow model to check for changes.</param>
    /// <returns>
    ///     A task that returns <c>true</c> if the workflow file is dirty (needs regeneration),
    ///     <c>false</c> if the file is up-to-date or doesn't exist.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         This method is useful for build systems or CI/CD pipelines that need to determine
    ///         if workflow files are out of sync with their configuration without actually writing files.
    ///     </para>
    ///     <para>
    ///         The comparison normalizes line endings using <see cref="string.ReplaceLineEndings()" />
    ///         to ensure consistent results across different platforms (Windows, Linux, macOS).
    ///     </para>
    ///     <para>
    ///         When a dirty workflow is detected, detailed logging information is provided showing
    ///         both the existing and new content for debugging purposes.
    ///     </para>
    /// </remarks>
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

    /// <summary>
    ///     Writes a line of text to the output with appropriate indentation based on the current <see cref="IndentLevel" />.
    /// </summary>
    /// <param name="value">The text to write. If <c>null</c>, writes an empty line.</param>
    /// <remarks>
    ///     <para>
    ///         This method automatically applies the current indentation level using spaces.
    ///         The number of spaces is calculated as <see cref="IndentLevel" /> × <see cref="TabSize" />.
    ///     </para>
    ///     <para>
    ///         Each call appends a line terminator, so subsequent calls will appear on separate lines.
    ///         For inline text without line breaks, build the content manually using a StringBuilder.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// WriteLine("name: My Workflow");  // Writes: "name: My Workflow\n"
    /// WriteLine();                     // Writes: "\n" (empty line)
    /// </code>
    /// </example>
    protected void WriteLine(string? value = null)
    {
        if (IndentLevel > 0)
            _stringBuilder.Append(new string(' ', IndentLevel));

        _stringBuilder.AppendLine(value);
    }

    /// <summary>
    ///     Writes a section header and returns a disposable that automatically manages indentation for the section content.
    /// </summary>
    /// <param name="header">The header text to write for the section.</param>
    /// <returns>
    ///     A disposable object that automatically decreases the indentation level when disposed.
    ///     Use with a <c>using</c> statement to ensure proper indentation management.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         This method implements a convenient pattern for writing nested content with automatic indentation.
    ///         When the returned disposable is disposed (typically at the end of a <c>using</c> block),
    ///         the indentation level is automatically restored to its previous value.
    ///     </para>
    ///     <para>
    ///         The indentation is increased by <see cref="TabSize" /> spaces after writing the header.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// WriteLine("workflow:");
    /// using (WriteSection("jobs:"))
    /// {
    ///     WriteLine("build:");
    ///     using (WriteSection("steps:"))
    ///     {
    ///         WriteLine("- name: Checkout");
    ///         WriteLine("- name: Build");
    ///     }
    /// }
    /// // Indentation automatically restored here
    /// </code>
    /// </example>
    protected IDisposable WriteSection(string header)
    {
        WriteLine(header);
        IndentLevel += TabSize;

        return new DisposableAction(() => IndentLevel -= TabSize);
    }

    /// <summary>
    ///     Abstract method that concrete implementations must override to define how to write workflow content.
    /// </summary>
    /// <param name="workflow">The workflow model containing the configuration to write.</param>
    /// <remarks>
    ///     <para>
    ///         This method should use the provided helper methods like <see cref="WriteLine" /> and <see cref="WriteSection" />
    ///         to build the workflow file content. The base class handles all file I/O operations.
    ///     </para>
    ///     <para>
    ///         Implementations should focus on translating the <see cref="WorkflowModel" /> into the appropriate
    ///         file format (YAML, JSON, etc.) without worrying about file paths, change detection, or indentation management.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// protected override void WriteWorkflow(WorkflowModel workflow)
    /// {
    ///     WriteLine($"name: {workflow.Name}");
    ///     WriteLine($"on: {workflow.Trigger}");
    ///     using (WriteSection("jobs:"))
    ///     {
    ///         foreach (var job in workflow.Jobs)
    ///         {
    ///             using (WriteSection($"{job.Name}:"))
    ///             {
    ///                 WriteLine($"runs-on: {job.RunsOn}");
    ///                 // ... additional job configuration
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    protected abstract void WriteWorkflow(WorkflowModel workflow);
}
