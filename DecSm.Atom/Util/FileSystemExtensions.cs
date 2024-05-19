namespace DecSm.Atom.Util;

public static class FileSystemExtensions
{
    private static AbsolutePath? _root;
    
    public static AbsolutePath AtomRoot(this IFileSystem fileSystem)
    {
        if (_root is not null)
            return _root;
        
        var currentDirectory = fileSystem.Directory.GetCurrentDirectory();
        var foundSolutionRoot = false;
        var foundGitRoot = false;
        
        while (currentDirectory is not null && fileSystem.Directory.GetParent(currentDirectory) is not null)
        {
            if (!foundSolutionRoot)
            {
                if (fileSystem
                    .Directory
                    .GetFiles(currentDirectory, "*.sln")
                    .Any())
                    foundSolutionRoot = true;
            }
            
            if (!foundGitRoot)
            {
                if (fileSystem.Directory.Exists(fileSystem.Path.Combine(currentDirectory, ".git")))
                    foundGitRoot = true;
            }
            
            if (foundSolutionRoot || foundGitRoot)
                break;
            
            currentDirectory = fileSystem.Directory.GetParent(currentDirectory)
                ?.FullName;
        }
        
        if (!foundSolutionRoot && !foundGitRoot)
            throw new InvalidOperationException("Could not find the root of the Atom repository.");
        
        return _root = new(fileSystem, currentDirectory!);
    }
    
    public static AbsolutePath ArtifactDirectory(this IFileSystem fileSystem) =>
        fileSystem.AtomRoot() / "artifacts";
    
    public static AbsolutePath PublishDirectory(this IFileSystem fileSystem)
    {
        if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null)
            return fileSystem.AtomRoot() / "publish";
        
        return fileSystem.AtomRoot() / "artifacts";
    }
}