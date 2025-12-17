# Github Workflows

The `DecSm.Atom.Module.GithubWorkflows` module provides deep integration with GitHub Actions, enabling you to define, generate, and interact with GitHub workflows directly from your DecSm.Atom build definitions. This module streamlines the process of creating CI/CD pipelines on GitHub Actions, offering C#-based abstractions for common workflow patterns.

### Features

* **Workflow Generation**: Define GitHub Actions workflows (including Dependabot) in C# and generate the corresponding YAML files.
* **GitHub Environment Access**: Easily access GitHub Actions environment variables and context information.
* **Artifact and Publish Path Integration**: Automatically adjusts artifact and publish directories to align with GitHub Actions conventions.
* **GitHub Token Management**: Simplifies the injection of the `GITHUB_TOKEN` for API interactions.
* **Release Management Helpers**: Provides utilities for uploading artifacts to GitHub Releases.
* **Custom Runner Configuration**: Define custom runner images or use matrix strategies for job execution.
* **Step Summary Reporting**: Integrates build outcome reporting directly into GitHub Actions step summaries.
* **Expression Building**: A fluent API for constructing GitHub Actions expressions (e.g., `if` conditions, `with` inputs).

### Getting Started

To use the GitHub Workflows module, implement the `IGithubWorkflows` interface in your build definition. This will configure the necessary services for GitHub Actions integration.

#### Implementation

Add the `IGithubWorkflows` interface to your `Build.cs` file:

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.GithubWorkflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IGithubWorkflows
{
    // Your build targets and other definitions
}
```

### Defining Workflows

You can define GitHub Actions workflows using the `WorkflowDefinition` class and its associated options.

#### Example: Basic CI Workflow

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.GithubWorkflows;
using DecSm.Atom.Workflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IGithubWorkflows
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("ci")
        {
            Triggers =
            [
                new GitPushTrigger { IncludedBranches = ["main", "develop"] },
                new GitPullRequestTrigger { IncludedBranches = ["main", "develop"] }
            ],
            Targets = [WorkflowTargets.Compile], // Assuming 'Compile' is a defined target
            WorkflowTypes = [Github.WorkflowType]
        }
    ];

    private Target Compile =>
        t => t
            .DescribedAs("Compiles the project")
            .Executes(() =>
            {
                Logger.LogInformation("Compiling...");
                // dotnet build logic
            });
}
```

When you run your build with this definition, a `ci.yml` file will be generated in your `.github/workflows` directory.

#### Dependabot Workflows

The module provides helpers for generating Dependabot configuration files.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.GithubWorkflows;
using DecSm.Atom.Workflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IGithubWorkflows
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        Github.DependabotDefaultWorkflow()
    ];
}
```

This will generate a `dependabot.yml` file with default NuGet update settings. You can customize it using `Github.DependabotWorkflow(DependabotOptions options)`.

### GitHub Environment Variables

The `Github.Variables` static class provides strongly-typed access to common GitHub Actions environment variables.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.GithubWorkflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IGithubWorkflows
{
    private Target LogGithubInfo =>
        t => t
            .DescribedAs("Logs GitHub Actions environment information")
            .Executes(() =>
            {
                if (Github.IsGithubActions)
                {
                    Logger.LogInformation("Running on GitHub Actions!");
                    Logger.LogInformation("Repository: {Repo}", Github.Variables.Repository);
                    Logger.LogInformation("Ref: {Ref}", Github.Variables.Ref);
                    Logger.LogInformation("Actor: {Actor}", Github.Variables.Actor);
                }
                else
                {
                    Logger.LogInformation("Not running on GitHub Actions.");
                }
            });
}
```

### GitHub Token and Release Management

The `IGithubHelper` and `IGithubReleaseHelper` interfaces provide functionality for interacting with GitHub.

#### Injecting GitHub Token

Implement `IGithubHelper` to make the `GITHUB_TOKEN` available:

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.GithubWorkflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IGithubWorkflows, IGithubHelper
{
    // GithubToken is now available via this.GithubToken
}
```

You can then use `this.GithubToken` in your build logic to authenticate with the GitHub API. For workflow generation, use the `WithGithubTokenInjection()` extension:

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.GithubWorkflows;
using DecSm.Atom.Workflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IGithubWorkflows, IGithubHelper
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("deploy")
        {
            Triggers = [new GitPushTrigger { IncludedBranches = ["main"] }],
            Targets =
            [
                WorkflowTargets.DeployApp.WithGithubTokenInjection() // Injects GITHUB_TOKEN into the workflow job
            ],
            WorkflowTypes = [Github.WorkflowType]
        }
    ];

    private Target DeployApp =>
        t => t
            .DescribedAs("Deploys the application")
            .Executes(() =>
            {
                // Deployment logic that might use GithubToken
            });
}
```

#### Uploading to GitHub Releases

Implement `IGithubReleaseHelper` to upload artifacts to GitHub Releases:

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.GithubWorkflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IGithubWorkflows, IGithubReleaseHelper
{
    private Target CreateRelease =>
        t => t
            .DescribedAs("Creates a GitHub Release and uploads artifacts")
            .Executes(async () =>
            {
                // Assuming 'MyApp.zip' is in your Atom artifacts directory
                await UploadArtifactToRelease("MyApp.zip", "v1.0.0");
                Logger.LogInformation("Uploaded MyApp.zip to release v1.0.0");
            });
}
```

### Custom Runner Configuration

You can specify custom runners or use a matrix strategy for your workflow jobs.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.GithubWorkflows;
using DecSm.Atom.Workflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IGithubWorkflows
{
    private Target TestOnMultipleOs =>
        t => t
            .DescribedAs("Runs tests on multiple operating systems")
            .Executes(() =>
            {
                Logger.LogInformation("Running tests on {OS}", JobRunsOn); // JobRunsOn will be the current OS from the matrix
            });

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("ci")
        {
            Triggers = [new GitPushTrigger { IncludedBranches = ["main"] }],
            Targets =
            [
                WorkflowTargets.TestOnMultipleOs
                    .WithGithubRunnerMatrix("ubuntu-latest", "windows-latest") // Run TestOnMultipleOs on both OSes
            ],
            WorkflowTypes = [Github.WorkflowType]
        }
    ];
}
```

You can also specify a custom Docker image for your runner using `GithubSnapshotImageOption`:

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.GithubWorkflows;
using DecSm.Atom.Workflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IGithubWorkflows
{
    private Target RunInCustomImage =>
        t => t
            .DescribedAs("Runs a job in a custom Docker image")
            .Executes(() =>
            {
                Logger.LogInformation("Running in custom image.");
            });

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("custom-image")
        {
            Triggers = [new GitPushTrigger { IncludedBranches = ["main"] }],
            Targets = [WorkflowTargets.RunInCustomImage],
            Options = [new GithubSnapshotImageOption(new("my-custom-image", "1.0.0"))],
            WorkflowTypes = [Github.WorkflowType]
        }
    ];
}
```

### GitHub Actions Expressions

The module provides a fluent API to construct GitHub Actions expressions, which are useful for `if` conditions, `with` inputs, and other dynamic parts of your workflow.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.GithubWorkflows;
using DecSm.Atom.Module.GithubWorkflows.Generation.Options; // For GithubIf
using DecSm.Atom.Workflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IGithubWorkflows
{
    private Target ConditionalStep =>
        t => t
            .DescribedAs("A step that runs conditionally")
            .Executes(() =>
            {
                Logger.LogInformation("This step ran conditionally.");
            });

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("conditional")
        {
            Triggers = [new GitPushTrigger { IncludedBranches = ["main"] }],
            Targets =
            [
                WorkflowTargets.ConditionalStep.WithOptions(
                    GithubIf.Create(
                        new PropertyExpression("github", "ref_name").EqualTo(new StringExpression("main"))
                            .And(new PropertyExpression("github", "event_name").EqualTo(new StringExpression("push")))
                    )
                )
            ],
            WorkflowTypes = [Github.WorkflowType]
        }
    ];
}
```

This will generate an `if` condition for the `ConditionalStep` job that checks if the `GITHUB_REF_NAME` is "main" AND the `GITHUB_EVENT_NAME` is "push".
