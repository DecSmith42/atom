namespace DecSm.Atom.Workflows.Generation;

public abstract class AtomWorkflowFileWriter<T>(IFileSystem fileSystem, ILogger<AtomWorkflowFileWriter<T>> logger) : IAtomWorkflowWriter<T>
    where T : IWorkflowType
{
    private readonly StringBuilder _stringBuilder = new();
    
    private int IndentLevel { get; set; }
    
    protected virtual int TabSize => 2;
    
    protected virtual AbsolutePath FileLocation => fileSystem.SolutionRoot();
    
    protected abstract string FileExtension { get; }
    
    protected virtual bool AppendWorkflowTypeToFile => false;
    
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
    
    public IDisposable WriteSection(string header)
    {
        WriteLine(header);
        IndentLevel += TabSize;
        
        return new Disposable(() => IndentLevel -= TabSize);
    }
    
    protected abstract void WriteWorkflow(Workflow workflow);
    
    public void Generate(Workflow workflow)
    {
        var filePath = AppendWorkflowTypeToFile
            ? FileLocation / $"{workflow.Name}-{typeof(T).Name.Replace("WorkflowType", string.Empty)}.{FileExtension}"
            : FileLocation / $"{workflow.Name}.{FileExtension}";
        
        WriteWorkflow(workflow);
        
        var newText = _stringBuilder.ToString();
        _stringBuilder.Clear();
        
        var existingText = fileSystem.File.Exists(filePath)
            ? fileSystem.File.ReadAllText(filePath)
            : string.Empty;
        
        if (existingText == newText)
        {
            logger.LogInformation("Workflow file is up to date: {FilePath}", filePath);
            
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
}