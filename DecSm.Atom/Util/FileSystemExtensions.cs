namespace DecSm.Atom.Util;

public static class FileSystemExtensions
{
    private static AbsolutePath? _solutionRoot;
    private static AbsolutePath? _solutionFile;
    private static AbsolutePath? _repoRoot;

    public static AbsolutePath SolutionRoot(this IFileSystem fileSystem)
    {
        if (_solutionRoot is not null)
            return _solutionRoot;

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

        return _solutionRoot = new(fileSystem, topmostSolutionDirectory);
    }

    public static AbsolutePath SolutionFile(this IFileSystem fileSystem)
    {
        if (_solutionFile is not null)
            return _solutionFile;

        return _solutionFile = new(fileSystem,
            fileSystem
                .Directory
                .GetFiles(fileSystem.SolutionRoot(), "*.sln")
                .Single());
    }

    public static string SolutionName(this IFileSystem fileSystem)
    {
        var solutionFileInfo = fileSystem.FileInfo.New(fileSystem.SolutionFile());

        return solutionFileInfo.Name.Replace(solutionFileInfo.Extension, string.Empty);
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
            : fileSystem.SolutionRoot() / "atom-publish";

    public static AbsolutePath PublishDirectory(this IFileSystem fileSystem) =>
        Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null
            ? fileSystem.RepoRoot() / ".github" / "publish"
            : fileSystem.SolutionRoot() / "atom-publish";

    public static AbsolutePath TempDirectory(this IFileSystem fileSystem) =>
        new(fileSystem, fileSystem.Path.GetTempPath());

    public static void Copy(this IDirectory directory, AbsolutePath source, AbsolutePath destination)
    {
        // Get all files in the directory
        var files = directory.GetFiles(source, "*", SearchOption.TopDirectoryOnly);

        // Copy each file to the new directory
        foreach (var file in files)
        {
            var filePath = source / file;
            var destinationPath = destination / file;

            directory.FileSystem.File.Copy(filePath, destinationPath, true);
        }

        // Get all directories in the directory
        var directories = directory.GetDirectories(source, "*", SearchOption.TopDirectoryOnly);

        // Copy each directory to the new directory
        foreach (var dir in directories)
        {
            var dirPath = source / dir;
            var destinationPath = destination / dir;

            directory.Copy(dirPath, destinationPath);
        }
    }
}