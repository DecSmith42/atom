namespace Atom.Targets;

[TargetDefinition]
internal partial interface IPrepareRelease : IProcessHelper
{
    [ParamDefinition("prerelease-tag", "Optional prerelease tag")]
    string? PrereleaseTag => GetParam(() => PrereleaseTag);

    Target PrepareRelease =>
        d => d
            .WithDescription("Prepares a release using GitVersioning")
            .RequiresParam(Atom.Build.Params.PrereleaseTag)
            .Executes(async () =>
            {
                if (FileSystem != null)
                {
                    var prepareReleaseOutput = await RunProcessAsync("nbgv", $"prepare-release {PrereleaseTag}", FileSystem.RepoRoot());

                    var outputLines = prepareReleaseOutput.Split(Environment.NewLine);

                    var releaseBranch = outputLines[0]
                        .Split(" ")[0];

                    var currentBranch = outputLines[1]
                        .Split(" ")[0];

                    await RunProcessAsync("git", $"push origin {currentBranch}");
                    await RunProcessAsync("git", $"checkout {releaseBranch}");
                    await RunProcessAsync("git", $"push origin {releaseBranch}");
                }
            });
}