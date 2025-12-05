# Getting Started with DecSm.Atom

Welcome to DecSm.Atom! This guide will walk you through setting up your first Atom-powered build project. Atom provides
a powerful, type-safe, and extensible framework for defining your build, test, and deployment pipelines entirely in C#.

## Prerequisites

Before you begin, ensure you have the following installed:

* **.NET SDK 10.0 (recommended, 8.0 or later supported)**: Atom projects are built with .NET.
* **Git**: For version control and triggering workflows.
* **An IDE with C# support**: Visual Studio, Rider, or VS Code are recommended.

## 0. (Optional) Install the Atom Tool

While you can run Atom builds using `dotnet run`, installing the Atom CLI tool provides a more streamlined experience.

```bash
dotnet tool install -g DecSm.Atom.Tool
```

## 1. Set up the Build Definition

1. **Create a new C# project for your build definition**:
    ```bash
    dotnet new worker -n _atom
    ```

2. **Add the `DecSm.Atom` package to your `_atom` project**:
    ```bash
    cd _atom
    dotnet add package DecSm.Atom
    dotnet add package DecSm.Atom.GithubWorkflows 
    ```

3. **Define your `Build` class**:
   Create a file named `Build.cs` inside the `_atom` folder and define your build.

    ```csharp
    // _atom/Build.cs
    
    // Mark this class as the build definition and generate an entry point
    [BuildDefinition]
    public partial class Build : ITargets
    {        
        // Define your targets
        Target MyCustomTarget => t => t
            .DescribedAs("My first custom Atom target.")
            .Executes(() =>
            {
                Logger.LogInformation("Hello from MyCustomTarget!");
            });
        
        // Define your workflows here
        public override IReadOnlyList<WorkflowDefinition> Workflows =>
        [
            new("my-workflow")
            {
                // Defines the conditions under which the workflow should run
                Triggers =
                [
                    GitPushTrigger.ToMain,
                    GitPullRequestTrigger.IntoMain,
                ],
                
                // Defines the targets that should be executed when the workflow runs
                Targets =
                [
                    // Targets.* are source generated from all targets included in the build for convenience
                    Targets.MyCustomTarget,
                ],
                
                // Generate workflows for GitHub Actions
                WorkflowTypes = [Github.WorkflowType],
            }
        ];
    }
    ```

## 2. Run Your Build

Now you can run your Atom build from the command line.

```bash
# Run the build, specifying your _atom project
atom MyCustomTarget

# Or without the tool
dotnet run -- MyCustomTarget
```

You should see output similar to:

```
Executing target MyCustomTarget...

My first custom Atom target.

Hello from MyCustomTarget!
```

## 3. Generate CI/CD Workflows

Atom can automatically generate CI/CD workflow files (e.g., GitHub Actions YAML) based on your `WorkflowDefinition`s.

```bash
# Generate workflows
atom -g

# Or without the tool
dotnet run -- -g
```

This will create a `.github/workflows` directory (if using `Github.WorkflowType`) containing `my-workflow.yml` (or
whatever you named your workflow).

## Next Steps

You've successfully set up and run your first Atom build! To learn more about Atom's capabilities, explore the following
guides:

* [**Build Definitions**](./build-definitions.md): Understand the core structure of your build.
* [**Targets**](./targets.md): Learn how to define and execute units of work.
* [**Parameters**](./parameters.md): Configure your build with external inputs.
* [**Workflows**](./workflows.md): Orchestrate your targets for CI/CD.
* [**Build Accessor**](./build-accessor.md): Access core services and helpers.
* [**Atom File System**](./atom-file-system.md): Perform file operations safely.
* [**Process Runner**](./process-runner.md): Execute external commands.
* [**Secrets Management**](./secrets.md): Handle sensitive data securely.
* [**Workflow Variables**](./workflow-variables.md): Share data between targets and jobs.
* [**Common Targets**](./common-targets.md): Leverage pre-built targets for common tasks.
* [**Build Information Providers**](./build-information-providers.md): Customize build ID, version, and timestamp.
* [**Rich Console Output**](./console-output.md): Enhance your build's console experience.
* [**File Transformation Scopes**](./file-transformation-scopes.md): Temporarily modify files.
* [**Semantic Versioning**](./semantic-versioning.md): Work with `SemVer` types.
* [**Step Failed Exception**](./step-failed-exception.md): Handle build failures gracefully.
