namespace DecSm.Atom.Module.Dotnet;

[PublicAPI]
public static class DotnetFileUtils
{
    /// <summary>
    ///     Finds the Directory.Build.props files that apply to the given project file.
    /// </summary>
    /// <param name="projectFilePath">The full path to the project file.</param>
    /// <param name="rootPath">The root path for the operation. Directories above this path will not be searched.</param>
    /// <returns>The project file as well as any Directory.Build.props files that apply to it.</returns>
    public static IEnumerable<RootedPath> GetPropertyFilesForProject(RootedPath projectFilePath, RootedPath rootPath)
    {
        List<RootedPath> filesToTransform = [projectFilePath];

        var dir = projectFilePath;

        do
        {
            dir = dir.Parent;

            if (dir is null)
                break;

            var file = dir / "Directory.Build.props";

            if (file.FileExists)
                filesToTransform.Add(file);
        } while (dir != rootPath);

        return filesToTransform;
    }

    /// <summary>
    ///     Gets the full path to a project file by its name.
    /// </summary>
    /// <param name="fileSystem">The file system to use.</param>
    /// <param name="projectName">The name of the project.</param>
    /// <returns>The full path to the project file.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the project file does not exist.</exception>
    /// <remarks>
    ///     The project file is expected to be in a directory with the same name as the project.
    ///     E.g. for a project named "MyProject", the project file is expected to be in a directory named "MyProject",
    ///     which itself is expected to be in the Atom root directory.
    /// </remarks>
    public static RootedPath GetProjectFilePathByName(IAtomFileSystem fileSystem, string projectName)
    {
        var project = fileSystem.FileInfo.New(fileSystem.AtomRootDirectory / projectName / $"{projectName}.csproj");
        var projectPath = new RootedPath(fileSystem, project.FullName);

        return !project.Exists
            ? throw new InvalidOperationException($"Project file {project.FullName} does not exist.")
            : projectPath;
    }
}
