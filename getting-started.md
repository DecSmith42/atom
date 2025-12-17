---
icon: flag
---

# Getting Started

Welcome to DecSm.Atom! This guide will walk you through setting up your first Atom-powered build project. Atom provides\
a powerful, type-safe, and extensible framework for defining your build, test, and deployment pipelines entirely in C#.

### Prerequisites

Before you begin, ensure you have the following installed:

* **.NET SDK 10.0 (recommended, 8.0 or later supported)**: Atom projects are built with .NET.
* **(Optional) Git**: For version control and triggering workflows. However, atom can also be used without Git.
* **(Optional) An IDE with C# support**: Visual Studio, Rider, or VS Code are recommended, though any text editor with the .NET SDK and CLI will work.

### 0. (Optional) Install the Atom Tool

While you can run Atom builds using `dotnet run`, installing the Atom CLI tool provides a more streamlined experience.

```bash
dotnet tool install -g DecSm.Atom.Tool
```

### 1. Set up the Build Definition

**Note:** The following `dotnet` commands should be executed in your terminal.

1.  **Create a new C# project for your build definition**:

    ```bash
    dotnet new worker -n _atom
    ```
2.  **Add the `DecSm.Atom` package to your `_atom` project**:

    ```bash
    cd _atom
    dotnet add package DecSm.Atom
    dotnet add package DecSm.Atom.GithubWorkflows 
    ```
3.  **Define your `Build` class**: Create a file named `Build.cs` inside the `_atom` folder and define your build.

    ```csharp
    // _atom/Build.cs

    // The DecSm.Atom NuGet package provides global usings for most Atom types.

    // Mark this class as the build definition and generate an entry point
    [BuildDefinition]
    [GenerateEntryPoint]
    public partial class Build : BuildDefinition
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
                    // WorkflowTargets.* is a source-generated static class.
                    // Atom automatically creates properties for each defined Target in your Build class,
                    // allowing for convenient access and strong typing.
                    WorkflowTargets.MyCustomTarget,
                ],
                
                // Generate workflows for GitHub Actions
                WorkflowTypes = [Github.WorkflowType],
            }
        ];
    }
    ```

### 2. Run Your Build

**Note:** The following commands should be executed in your terminal from your build project directory (e.g., `_atom`).

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

### 3. Generate CI/CD Workflows

**Note:** The following commands should be executed in your terminal from your build project directory (e.g., `_atom`).

Atom can automatically generate CI/CD workflow files (e.g., GitHub Actions YAML) based on your `WorkflowDefinition`s.

```bash
# Generate workflows
atom -g

# Or without the tool
dotnet run -- -g
```

This will create a `.github/workflows` directory (if using `Github.WorkflowType`) containing `my-workflow.yml` (or whatever you named your workflow).

### Next Steps

You've successfully set up and run your first Atom build! To learn more about Atom's capabilities, explore the following guides:

* **Build Definitions**: Understand the core structure of your build.
* **Targets**: Learn how to define and execute units of work.
* **Parameters**: Configure your build with external inputs.
* **Workflows**: Orchestrate your targets for CI/CD.
* **Build Accessor**: Access core services and helpers.
* **Atom File System**: Perform file operations safely.
* **Process Runner**: Execute external commands.
* **Secrets Management**: Handle sensitive data securely.
* **Workflow Variables**: Share data between targets and jobs.
* **Common Targets**: Leverage pre-built targets for common tasks.
* **Build Information Providers**: Customize build ID, version, and timestamp.
* **Rich Console Output**: Enhance your build's console experience.
* **File Transformation Scopes**: Temporarily modify files.
* **Semantic Versioning**: Work with `SemVer` types.
* **Step Failed Exception**: Handle build failures gracefully.
