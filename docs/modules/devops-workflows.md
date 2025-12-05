# Azure DevOps Workflows Module

The `DecSm.Atom.Module.DevopsWorkflows` module provides deep integration with Azure DevOps Pipelines, enabling you to define, generate, and interact with Azure DevOps workflows directly from your DecSm.Atom build definitions. This module streamlines the process of creating CI/CD pipelines on Azure DevOps, offering C#-based abstractions for common workflow patterns.

## Features

*   **Workflow Generation**: Define Azure DevOps Pipelines in C# and generate the corresponding YAML files.
*   **Azure DevOps Environment Access**: Easily access Azure DevOps environment variables and context information.
*   **Artifact and Publish Path Integration**: Automatically adjusts artifact and publish directories to align with Azure DevOps conventions.
*   **Custom Agent Pool Configuration**: Define custom agent pools or use matrix strategies for job execution.
*   **Build Summary Reporting**: Integrates build outcome reporting directly into Azure DevOps build summaries.
*   **Variable Management**: Supports writing output variables that can be consumed by subsequent steps or jobs.

## Getting Started

To use the Azure DevOps Workflows module, implement the `IDevopsWorkflows` interface in your build definition. This will configure the necessary services for Azure DevOps integration.

### Implementation

Add the `IDevopsWorkflows` interface to your `Build.cs` file:

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.DevopsWorkflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IDevopsWorkflows
{
    // Your build targets and other definitions
}
```

## Defining Workflows

You can define Azure DevOps Pipelines using the `WorkflowDefinition` class and its associated options.

### Example: Basic CI Pipeline

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.DevopsWorkflows;
using DecSm.Atom.Workflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IDevopsWorkflows
{
    private Workflow CiPipeline =>
        new("ci")
            .OnPush(new[] { "main", "develop" })
            .OnPullRequest(new[] { "main", "develop" })
            .Target(Compile); // Assuming 'Compile' is a defined target

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

When you run your build with this definition, a `ci.yml` file will be generated in your `.devops` directory (or wherever your Azure DevOps pipeline definition expects it).

## Azure DevOps Environment Variables

The `Devops.Variables` static class provides strongly-typed access to common Azure DevOps environment variables.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.DevopsWorkflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IDevopsWorkflows
{
    private Target LogDevopsInfo =>
        t => t
            .DescribedAs("Logs Azure DevOps environment information")
            .Executes(() =>
            {
                if (Devops.IsDevopsPipelines)
                {
                    Logger.LogInformation("Running on Azure DevOps Pipelines!");
                    Logger.LogInformation("Build ID: {BuildId}", Devops.Variables.BuildBuildId);
                    Logger.LogInformation("Source Branch: {Branch}", Devops.Variables.BuildSourceBranch);
                    Logger.LogInformation("Requested For: {User}", Devops.Variables.BuildRequestedFor);
                }
                else
                {
                    Logger.LogInformation("Not running on Azure DevOps Pipelines.");
                }
            });
}
```

## Custom Agent Pool Configuration

You can specify custom agent pools or use a matrix strategy for your pipeline jobs.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.DevopsWorkflows;
using DecSm.Atom.Workflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IDevopsWorkflows
{
    private Target TestOnMultipleOs =>
        t => t
            .DescribedAs("Runs tests on multiple operating systems")
            .Executes(() =>
            {
                Logger.LogInformation("Running tests on agent OS: {OS}", Devops.Variables.AgentOs);
            });

    private Workflow CiPipeline =>
        new("ci")
            .OnPush(new[] { "main" })
            .Target(TestOnMultipleOs)
            .WithDevopsPoolMatrix(new[] { "ubuntu-latest", "windows-latest" }); // Run TestOnMultipleOs on both OSes
}
```

## Variable Management

The module allows you to write output variables that can be consumed by subsequent steps or jobs within the pipeline.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.DevopsWorkflows;
using DecSm.Atom.Workflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IDevopsWorkflows
{
    private Target SetOutputVariable =>
        t => t
            .DescribedAs("Sets an output variable")
            .Executes(async () =>
            {
                await WorkflowVariableProvider.WriteVariable("MyOutput", "HelloFromAtom");
                Logger.LogInformation("Set output variable 'MyOutput'.");
            });

    private Target ConsumeOutputVariable =>
        t => t
            .DescribedAs("Consumes an output variable from a previous job")
            .Executes(async () =>
            {
                // In Azure DevOps, output variables are typically accessed via dependencies.
                // The ReadVariable method here primarily indicates if the environment supports reading.
                // Actual consumption in YAML would look like: ${{ dependencies.SetOutputVariableJob.outputs.MyOutput }}
                if (await WorkflowVariableProvider.ReadVariable("SetOutputVariableJob", "MyOutput"))
                {
                    Logger.LogInformation("Successfully indicated ability to read 'MyOutput'.");
                }
            });
}
```

## Checkout Options

You can customize the checkout behavior using `DevopsCheckoutOption`.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.DevopsWorkflows;
using DecSm.Atom.Workflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IDevopsWorkflows
{
    private Workflow CheckoutLfsWorkflow =>
        new("checkout-lfs")
            .OnPush(new[] { "main" })
            .Target(BuildProject)
            .Options(new DevopsCheckoutOption(lfs: true)); // Enable LFS during checkout

    private Target BuildProject =>
        t => t
            .DescribedAs("Builds the project")
            .Executes(() =>
            {
                Logger.LogInformation("Building project with LFS enabled.");
            });
}
```

## Build ID from Azure DevOps Run ID

You can configure DecSm.Atom to use the Azure DevOps `Build.BuildId` as the internal workflow ID.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.DevopsWorkflows;
using DecSm.Atom.Workflows;

namespace Atom;

[BuildDefinition]
internal partial class Build : IDevopsWorkflows
{
    public IReadOnlyList<IWorkflowOption> GlobalWorkflowOptions =>
        new List<IWorkflowOption> { new ProvideDevopsRunIdAsWorkflowId() };

    private Target ShowBuildId =>
        t => t
            .DescribedAs("Displays the build ID from Azure DevOps")
            .Executes(() =>
            {
                Logger.LogInformation("Current Build ID: {BuildId}", BuildIdProvider.BuildId);
            });
}
```
