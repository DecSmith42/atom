# GitVersion

The `DecSm.Atom.Module.GitVersion` module integrates [GitVersion](https://gitversion.net/) into your DecSm.Atom build process. GitVersion is a tool that generates a semantic version number based on your Git history, providing consistent and meaningful versioning for your builds and releases.

### Features

* **Automatic Versioning**: Automatically determines the semantic version of your project based on Git tags and branch conventions.
* **Build ID Generation**: Uses the full semantic version as the build ID, ensuring unique and traceable build identifiers.
* **Build Version Provider**: Provides detailed version components (major, minor, patch, pre-release) for use in your build logic.
* **Seamless Integration**: Configures DecSm.Atom to use GitVersion's output for its internal build ID and versioning mechanisms.

### Getting Started

To use the GitVersion module, you need to implement the `IGitVersion` interface in your build definition. This will automatically register the necessary services to use GitVersion for build ID and versioning.

#### Prerequisites

* **GitVersion.Tool**: The GitVersion .NET tool must be installed. The module will attempt to install it automatically if it's not found.
* **Git Repository**: Your project must be a Git repository with a commit history.

#### Implementation

Add the `IGitVersion` interface to your `Build.cs` file:

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.GitVersion;

namespace Atom;

[BuildDefinition]
internal partial class Build : IGitVersion
{
    // Your build targets and other definitions
}
```

### Usage

Once `IGitVersion` is implemented, DecSm.Atom will automatically use GitVersion to determine the build ID and version.

#### Accessing Build ID

The build ID will be the `FullSemVer` output from GitVersion. You can access it via the `BuildId` property of the `IBuildIdProvider` service.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Build.Info; // Required for IBuildIdProvider
using DecSm.Atom.Module.GitVersion;

namespace Atom;

[BuildDefinition]
internal partial class Build : IGitVersion
{
    private Target ShowBuildId =>
        t => t
            .DescribedAs("Displays the build ID determined by GitVersion")
            .Executes(() =>
            {
                Logger.LogInformation("Current Build ID: {BuildId}", BuildIdProvider.BuildId);
            });
}
```

#### Accessing Build Version

The build version, as a `SemVersion` object, can be accessed via the `Version` property of the `IBuildVersionProvider` service.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Build.Info; // Required for IBuildVersionProvider
using DecSm.Atom.Module.GitVersion;

namespace Atom;

[BuildDefinition]
internal partial class Build : IGitVersion
{
    private Target ShowBuildVersion =>
        t => t
            .DescribedAs("Displays the build version determined by GitVersion")
            .Executes(() =>
            {
                var version = BuildVersionProvider.Version;
                Logger.LogInformation("Major: {Major}", version.Major);
                Logger.LogInformation("Minor: {Minor}", version.Minor);
                Logger.LogInformation("Patch: {Patch}", version.Patch);
                Logger.LogInformation("PreRelease: {PreRelease}", version.Prerelease);
                Logger.LogInformation("Full Version: {FullVersion}", version.ToString());
            });
}
```

#### Controlling Build ID Generation

By default, implementing `IGitVersion` enables GitVersion for both build ID and versioning. If you need to explicitly control whether GitVersion is used for the build ID, you can use the `UseGitVersionForBuildId` workflow option.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.GitVersion;
using DecSm.Atom.Workflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IGitVersion
{
    // Globally enable GitVersion for build ID (this is the default when IGitVersion is implemented)
    public IReadOnlyList<IWorkflowOption> GlobalWorkflowOptions =>
        new List<IWorkflowOption> { new UseGitVersionForBuildId() };

    // Or disable it for a specific target if needed (e.g., if another provider should be used)
    private Target MyTargetWithoutGitVersionId =>
        t => t
            .Options(new UseGitVersionForBuildId(false)) // Disable GitVersion for this target's build ID
            .Executes(() =>
            {
                // BuildIdProvider.BuildId will throw an exception if no other IBuildIdProvider is configured
                Logger.LogInformation("This target is not using GitVersion for Build ID.");
            });
}
```

### How GitVersion Works

GitVersion analyzes your Git repository to determine the current version. It looks at:

* **Tags**: Semantic version tags (e.g., `v1.0.0`) are used as base versions.
* **Branches**: Branch names (e.g., `main`, `develop`, `feature/my-feature`) influence the pre-release tag.
* **Commits**: The number of commits since the last tag and the commit SHA are used to increment the patch version and add build metadata.

For more detailed information on how GitVersion calculates versions, refer to the [GitVersion documentation](https://gitversion.net/docs/learn/understanding-versioning).
