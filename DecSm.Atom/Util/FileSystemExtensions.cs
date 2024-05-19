namespace DecSm.Atom.Util;

public static class FileSystemExtensions
{
    private static AbsolutePath? _atomRoot;
    private static AbsolutePath? _repoRoot;
    
    public static AbsolutePath SolutionRoot(this IFileSystem fileSystem)
    {
        if (_atomRoot is not null)
            return _atomRoot;
        
        var currentDirectory = fileSystem.Directory.GetCurrentDirectory();
        string? topmostSolutionDirectory = null;
        
        while (currentDirectory is not null && fileSystem.Directory.GetParent(currentDirectory) is not null)
        {
            var solutionFiles = fileSystem.Directory.GetFiles(currentDirectory, "*.sln");
            
            if (solutionFiles.Length != 0)
                topmostSolutionDirectory = currentDirectory;
            
            currentDirectory = fileSystem.Directory.GetParent(currentDirectory)
                ?.FullName;
        }
        
        if (topmostSolutionDirectory is null)
            throw new InvalidOperationException(
                "Could not find the root of the Atom repository. Ensure that a .sln file exists in the root directory.");
        
        return _atomRoot = new(fileSystem, topmostSolutionDirectory);
    }
    
    public static AbsolutePath RepoRoot(this IFileSystem fileSystem)
    {
        if (_repoRoot is not null)
            return _repoRoot;
        
        var currentDirectory = fileSystem.Directory.GetCurrentDirectory();
        string? topmostGitDirectory = null;
        
        while (currentDirectory is not null && fileSystem.Directory.GetParent(currentDirectory) is not null)
        {
            var hasGirDir = fileSystem.Directory.Exists(fileSystem.Path.Combine(currentDirectory, ".git"));
            
            if (hasGirDir)
                topmostGitDirectory = currentDirectory;
            
            currentDirectory = fileSystem.Directory.GetParent(currentDirectory)
                ?.FullName;
        }
        
        if (topmostGitDirectory is null)
            throw new InvalidOperationException(
                "Could not find the repository root. Ensure that a .git directory exists in the root directory.");
        
        return _repoRoot = new(fileSystem, topmostGitDirectory);
    }
    
    public static AbsolutePath ArtifactDirectory(this IFileSystem fileSystem) =>
        Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null
            ? fileSystem.RepoRoot() / ".github" / "artifacts"
            : fileSystem.SolutionRoot() / "publish";
    
    public static AbsolutePath PublishDirectory(this IFileSystem fileSystem) =>
        Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null
            ? fileSystem.RepoRoot() / ".github" / "publish"
            : fileSystem.SolutionRoot() / "publish";
}