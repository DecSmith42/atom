namespace Atom.Targets;

internal interface IBuildTargets : IDotnetPackHelper, IDotnetPackHelper2, IDotnetPublishHelper2
{
    static readonly string[] BuildPlatformNames =
    [
        IJobRunsOn.WindowsLatestTag, IJobRunsOn.UbuntuLatestTag, IJobRunsOn.MacOsLatestTag,
    ];

    List<RootedPath> ProjectsToPack =>
    [
        FileSystem.GetPath<Projects.DecSm_Atom>(),
        FileSystem.GetPath<Projects.DecSm_Atom_Module_AzureKeyVault>(),
        FileSystem.GetPath<Projects.DecSm_Atom_Module_AzureStorage>(),
        FileSystem.GetPath<Projects.DecSm_Atom_Module_DevopsWorkflows>(),
        FileSystem.GetPath<Projects.DecSm_Atom_Module_Dotnet>(),
        FileSystem.GetPath<Projects.DecSm_Atom_Module_GitVersion>(),
        FileSystem.GetPath<Projects.DecSm_Atom_Module_GithubWorkflows>(),
    ];

    Target PackProjects =>
        t => t
            .DescribedAs("Packs the Atom projects (excluding the tool) into nuget packages")
            .ProducesArtifacts(ProjectsToPack.Select(project => project.FileNameWithoutExtension))
            .Executes(async cancellationToken =>
            {
                foreach (var project in ProjectsToPack)
                    await DotnetPackAndStage(project, cancellationToken: cancellationToken);
            });

    Target PackTool =>
        t => t
            .DescribedAs("Packs the Atom tool into a nuget package")
            .ProducesArtifact(Projects.DecSm_Atom_Tool.Name)
            .Executes(async cancellationToken =>
            {
                var runtimeIdentifier = RuntimeInformation.RuntimeIdentifier;

                Logger.LogInformation("Packing AOT Atom tool for runtime {RuntimeIdentifier}", runtimeIdentifier);

                await DotnetPackAndStage(FileSystem.GetPath<Projects.DecSm_Atom_Tool>(),
                    new()
                    {
                        PackOptions = new()
                        {
                            Runtime = runtimeIdentifier,
                            Property = "PublishAot=true",
                        },
                    },
                    cancellationToken);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Logger.LogInformation("Packing Atom tool for non-native AOT");

                    await DotnetPackAndStage(FileSystem.GetPath<Projects.DecSm_Atom_Tool>(),
                        new()
                        {
                            ClearPublishDirectory = false,
                        },
                        cancellationToken);
                }
            });
}
