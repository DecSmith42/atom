namespace DecSm.Atom.Module.Dotnet;

[TargetDefinition]
public partial interface IDotnetPublishHelper : IVersionHelper
{
    async Task DotnetPublishProject(DotnetPublishOptions options)
    {
        Logger.LogInformation("Publishing Atom project {AtomProjectName}", options.ProjectName);

        var project = FileSystem.FileInfo.New(FileSystem.AtomRootDirectory / options.ProjectName / $"{options.ProjectName}.csproj");
        var projectPath = new AbsolutePath(FileSystem, project.FullName);

        if (!project.Exists)
            throw new InvalidOperationException($"Project file {project.FullName} does not exist.");

        List<AbsolutePath> filesToTransform = [projectPath];

        var dir = projectPath;

        do
        {
            dir = dir.Parent;

            if (dir is null)
                break;

            var file = dir / "Directory.Build.props";

            if (file.FileExists)
                filesToTransform.Add(file);
        } while (dir != FileSystem.AtomRootDirectory);

        var buildVersionProvider = GetService<IBuildVersionProvider>();

        await using var setVersionScope = options.AutoSetVersion
            ? TransformProjectVersionScope.Create(filesToTransform, buildVersionProvider.Version)
            : null;

        var outputDirectory = FileSystem.AtomRootDirectory / options.ProjectName / "bin" / options.Configuration / "publish";

        if (FileSystem.Directory.Exists(outputDirectory))
            FileSystem.Directory.Delete(outputDirectory, true);

        await GetService<IProcessRunner>()
            .RunAsync(new("dotnet", $"publish {project.FullName} -c {options.Configuration} -o {outputDirectory}"));

        var outputArtifactName = options.OutputArtifactName ?? options.ProjectName;
        var publishDir = FileSystem.AtomPublishDirectory / outputArtifactName;

        Logger.LogInformation("Moving publish directory {PublishPath} to {PublishDir}", outputDirectory, publishDir);

        if (FileSystem.Directory.Exists(publishDir))
            FileSystem.Directory.Delete(publishDir, true);

        FileSystem.Directory.Move(outputDirectory, publishDir);

        Logger.LogInformation("Publishing Atom project {AtomProjectName} to {OutputArtifactName} completed",
            options.ProjectName,
            outputArtifactName);
    }
}
