---
icon: flag
---

# Getting Started

####

#### Installation

Atom comes with a .NET tool. You can install the Atom CLI tool, `DecSm.Atom.Tool`, to manage and run your builds.

Bash

```
dotnet tool install --global DecSm.Atom.Tool
```

_(Assuming the tool is published to NuGet. If it's a local tool, you might need to specify the path or use `dotnet tool install --local` commands within the repository context after packing it.)_

{% hint style="info" %}
You don't have to install the tool, it just streamlines the run process.\
`dotnet run --project <atom-project>` will also work.
{% endhint %}

#### Basic Usage

1. **Create a Build Project:** Typically, you'll have a dedicated build project within your solution. In this repository, the `_atom` project serves this purpose. This project will contain your build logic.
2.  **Define Your Build Definition:** In your build project (e.g., `_atom/Build.cs`), create a class that inherits from `DefaultBuildDefinition` (or `BuildDefinition` for more control). This class will house your targets and workflow configurations.

    C#

    ```
    // _atom/Build.cs
    using DecSm.Atom.Build.Definition;
    using DecSm.Atom.Hosting;
    // ... other necessary using statements for your targets

    [BuildDefinition]
    [GenerateEntryPoint] // This generates the Main method for your build project
    internal partial class Build : DefaultBuildDefinition, IMyCustomTarget // Implement interfaces for your targets
    {
        // Define workflows, default options, etc.
        public override IReadOnlyList<WorkflowDefinition> Workflows =>
        [
            new("MyWorkflow")
            {
                Triggers = [GitPushTrigger.ToMain], // Example trigger
                StepDefinitions =
                [
                    Targets.MyCustomTarget, // Refer to your defined targets
                ],
                WorkflowTypes = [Github.WorkflowType], // Example workflow type
            },
        ];
    }
    ```
3.  **Define Targets:** Targets represent individual tasks or operations in your build process. Define them by creating interfaces that inherit from `IBuildAccessor` and are marked with `[TargetDefinition]`. Implement the target logic within these interfaces.

    C#

    ```
    // _atom/Targets/MyCustomTarget.cs
    using DecSm.Atom.Build.Definition;
    using DecSm.Atom.Build; // For IBuildAccessor
    using System.Threading.Tasks;

    [TargetDefinition]
    internal partial interface IMyCustomTarget : IBuildAccessor
    {
        Target MyCustomTarget => d => d
            .DescribedAs("This is my custom build target.")
            .Executes(async () =>
            {
                Logger.LogInformation("Executing MyCustomTarget...");
                // Your target logic here
                await Task.CompletedTask;
            });
    }
    ```

    Make sure your main `Build` class inherits from these target interfaces (e.g., `public partial class Build : DefaultBuildDefinition, IMyCustomTarget`).
4.  **Running Builds:** Once your build project is set up, you can run your build targets using the Atom CLI tool from your repository root:

    Bash

    ```
    atom MyCustomTarget
    ```

    To see available targets and options:

    Bash

    ```
    atom --help
    ```
5.  **Workflow Generation:** Atom can generate CI/CD workflow files (e.g., for GitHub Actions, Azure DevOps). Configure your workflows in the `Workflows` property of your `Build` class. To generate these files, run:

    Bash

    ```
    atom --gen
    ```

    This will create/update files in locations like `.github/workflows/` or `.devops/workflows/` based on your `WorkflowTypes` and the corresponding `WorkflowFileWriter` implementations.

#### Core Concepts

* **Build Definition (`BuildDefinition`, `DefaultBuildDefinition`):** The central class where you define your build process, targets, parameters, and workflows.
* **Targets (`TargetDefinition`, `Target`):** Individual units of work, like compiling code, running tests, or packaging applications.
* **Parameters (`ParamDefinitionAttribute`, `SecretDefinitionAttribute`):** Inputs to your build targets, which can be sourced from command-line arguments, environment variables, configuration files, or secret providers.
* **Workflows (`WorkflowDefinition`, `WorkflowTargetDefinition`):** Define how targets are orchestrated, often for CI/CD pipelines, including triggers (e.g., git push, pull request) and options.
* **Modules:** Atom is extensible through modules that provide integrations and helpers for specific tools or platforms (e.g., `DecSm.Atom.Module.Dotnet`, `DecSm.Atom.Module.GithubWorkflows`, `DecSm.Atom.Module.AzureKeyVault`).

For more detailed examples, refer to the `_atom` project in this repository and the various `*.cs` files defining targets and build logic. The `.github/workflows` and `.devops/workflows` directories contain examples of generated workflow files.
