namespace Atom.Targets;

internal interface IBuildTargets : IDotnetPackHelper
{
    const string AtomProjectName = "DecSm.Atom";
    const string GitVersionModuleProjectName = "DecSm.Atom.Module.GitVersion";
    const string AtomGithubWorkflowsModuleProjectName = "DecSm.Atom.Module.GithubWorkflows";
    const string DotnetModuleProjectName = "DecSm.Atom.Module.Dotnet";
    const string AtomDevopsWorkflowsModuleProjectName = "DecSm.Atom.Module.DevopsWorkflows";
    const string AzureStorageModuleProjectName = "DecSm.Atom.Module.AzureStorage";
    const string AzureKeyVaultModuleProjectName = "DecSm.Atom.Module.AzureKeyVault";
    const string AtomToolProjectName = "DecSm.Atom.Tool";

    Target PackAtom =>
        t => t
            .DescribedAs("Builds the Atom project into a nuget package")
            .ProducesArtifact(AtomProjectName)
            .Executes(cancellationToken => DotnetPackProject(new(AtomProjectName), cancellationToken));

    Target PackGitVersionModule =>
        t => t
            .DescribedAs("Builds the GitVersion extension project into a nuget package")
            .ProducesArtifact(GitVersionModuleProjectName)
            .Executes(cancellationToken => DotnetPackProject(new(GitVersionModuleProjectName), cancellationToken));

    Target PackGithubWorkflowsModule =>
        t => t
            .DescribedAs("Builds the GithubWorkflows extension project into a nuget package")
            .ProducesArtifact(AtomGithubWorkflowsModuleProjectName)
            .Executes(cancellationToken => DotnetPackProject(new(AtomGithubWorkflowsModuleProjectName), cancellationToken));

    Target PackDotnetModule =>
        t => t
            .DescribedAs("Builds the Dotnet extension project into a nuget package")
            .ProducesArtifact(DotnetModuleProjectName)
            .Executes(cancellationToken => DotnetPackProject(new(DotnetModuleProjectName), cancellationToken));

    Target PackDevopsWorkflowsModule =>
        t => t
            .DescribedAs("Builds the DevopsWorkflows extension project into a nuget package")
            .ProducesArtifact(AtomDevopsWorkflowsModuleProjectName)
            .Executes(cancellationToken => DotnetPackProject(new(AtomDevopsWorkflowsModuleProjectName), cancellationToken));

    Target PackAzureStorageModule =>
        t => t
            .DescribedAs("Builds the AzureStorage extension project into a nuget package")
            .ProducesArtifact(AzureStorageModuleProjectName)
            .Executes(cancellationToken => DotnetPackProject(new(AzureStorageModuleProjectName), cancellationToken));

    Target PackAzureKeyVaultModule =>
        t => t
            .DescribedAs("Builds the AzureKeyVault extension project into a nuget package")
            .ProducesArtifact(AzureKeyVaultModuleProjectName)
            .Executes(cancellationToken => DotnetPackProject(new(AzureKeyVaultModuleProjectName), cancellationToken));

    Target PackAtomTool =>
        t => t
            .DescribedAs("Builds the DecSm.Atom.Tool project into a nuget package")
            .ProducesArtifact(AtomToolProjectName)
            .Executes(async cancellationToken =>
            {
                await DotnetPackProject(new(AtomToolProjectName)
                    {
                        Configuration = "Release",
                        RuntimeIdentifier = "win-x64",
                        NativeAot = true,
                        CustomPackageId = "atomtool",
                    },
                    cancellationToken);

                await DotnetPackProject(new(AtomToolProjectName)
                    {
                        Configuration = "Release",
                        CustomPackageId = "atomtool",
                        SuppressClearingPublishDirectory = true,
                    },
                    cancellationToken);
            });
}
