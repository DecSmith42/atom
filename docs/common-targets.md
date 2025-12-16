# Common Targets

Atom provides a set of common, reusable targets that cover typical build, test, and deployment scenarios. These targets
are defined in interfaces that your main build definition can implement, giving you access to pre-configured
functionality with minimal effort.

## `IBuildTargets`

The `IBuildTargets` interface provides targets related to building and packaging your projects. It leverages helpers
like `IDotnetPackHelper` and `IDotnetPublishHelper` to interact with the .NET CLI.

**What it is:**
A collection of targets focused on compiling, packaging, and preparing your application components.

**What it does:**

* **`PackProjects`**: This target is designed to package your core Atom projects (e.g., `DecSm.Atom`, its modules) into
  NuGet packages. It produces artifacts for each specified project.
* **`PackTool`**: This target specifically packages the Atom command-line tool into a NuGet package, handling
  platform-specific and AOT (Ahead-of-Time) compilation considerations.

**When and how to use it:**
Implement `IBuildTargets` in your build definition when you need to create NuGet packages for your libraries or tools.
You'll typically include these targets in your workflows to prepare your application for distribution or deployment.

```csharp
// In your build definition project (e.g., _atom/Build.cs)
[BuildDefinition]
public partial class MyBuild : BuildDefinition, IBuildTargets
{
    // ...
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("BuildAndPack")
        {
            Targets =
            [
                WorkflowTargets.PackProjects, // Use the PackProjects target
                WorkflowTargets.PackTool      // Use the PackTool target
            ],
            // ...
        }
    ];
}
```

## `ITestTargets`

The `ITestTargets` interface provides targets for running unit tests across your projects. It uses the
`IDotnetTestHelper` to execute tests via the .NET CLI.

**What it is:**
A collection of targets for executing automated tests and collecting coverage information.

**What it does:**

* **`TestProjects`**: This target runs all unit tests for a predefined set of Atom-related projects. It can be
  configured with a specific test framework and optionally collects code coverage.

**When and how to use it:**
Implement `ITestTargets` in your build definition when you want to automate the execution of your project's tests.
You'll typically include this target in your CI workflows to ensure code quality.

```csharp
// In your build definition project (e.g., _atom/Build.cs)
[BuildDefinition]
public partial class MyBuild : BuildDefinition, ITestTargets
{
    // ...
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("CI")
        {
            Targets =
            [
                WorkflowTargets.TestProjects // Run all tests
            ],
            // ...
        }
    ];
}
```

## `IDeployTargets`

The `IDeployTargets` interface provides targets for deploying your packaged applications, often to NuGet feeds or GitHub
Releases. It integrates with helpers like `INugetHelper` and `IGithubReleaseHelper`.

**What it is:**
A collection of targets focused on distributing your application components.

**What it does:**

* **`PushToNuget`**: This target pushes your generated NuGet packages to a specified NuGet feed (e.g., `nuget.org`). It
  requires a NuGet feed URL and an API key.
* **`PushToRelease`**: This target pushes your packages and other artifacts to a GitHub Release. It requires a GitHub
  token.

**When and how to use it:**
Implement `IDeployTargets` in your build definition when you need to publish your application packages. These targets
are typically part of your CD (Continuous Deployment) workflows, often triggered after successful builds and tests.

```csharp
// In your build definition project (e.g., _atom/Build.cs)
[BuildDefinition]
public partial class MyBuild : BuildDefinition, IDeployTargets
{
    // ...
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("CD")
        {
            Triggers = [GitPushTrigger.ToMain], // Trigger on push to main
            Targets =
            [
                WorkflowTargets.PushToNuget,   // Push packages to NuGet
                WorkflowTargets.PushToRelease  // Create a GitHub Release
            ],
            // ...
        }
    ];
}
```

By implementing these common target interfaces, you can quickly assemble robust build, test, and deployment pipelines
for your Atom projects.
