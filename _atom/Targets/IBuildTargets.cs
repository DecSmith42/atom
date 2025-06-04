namespace Atom.Targets;

internal interface IBuildTargets : IDotnetPackHelper
{
    public const string AtomProjectName = "DecSm.Atom";
    public const string GitVersionModuleProjectName = "DecSm.Atom.Module.GitVersion";
    public const string AtomGithubWorkflowsModuleProjectName = "DecSm.Atom.Module.GithubWorkflows";
    public const string DotnetModuleProjectName = "DecSm.Atom.Module.Dotnet";
    public const string AtomDevopsWorkflowsModuleProjectName = "DecSm.Atom.Module.DevopsWorkflows";
    public const string AzureStorageModuleProjectName = "DecSm.Atom.Module.AzureStorage";
    public const string AzureKeyVaultModuleProjectName = "DecSm.Atom.Module.AzureKeyVault";
    public const string AtomToolProjectName = "DecSm.Atom.Tool";

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
            .Executes(cancellationToken => DotnetPackProject(new(AtomToolProjectName), cancellationToken));
}
