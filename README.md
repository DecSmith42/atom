# Atom

[![Build](https://github.com/DecSmith42/atom/actions/workflows/Build.yml/badge.svg)](https://github.com/DecSmith42/atom/actions/workflows/Build.yml)

Atom is a build automation framework for .NET, inspired by tools like NUKE.
It allows developers to define build tasks using strongly-typed C# interfaces and classes, offering a flexible environment for automating development workflows.

A key feature of Atom is its ability to automatically generate CI/CD pipeline configurations for platforms such as GitHub Actions and Azure DevOps based on your C# definitions.

## Features

- **Type-Safe Build Definitions**: Define build targets using C# classes and interfaces.
- **CI/CD Generation**: Automatically generate GitHub Actions and Azure DevOps pipelines.
- **Artifact Management**: Integrated support for storing and retrieving artifacts via CI/CD hosts or custom providers (e.g., Azure Blob Storage).
- **Secret Management**: Secure handling of secrets using .NET user secrets or external providers like Azure Key Vault.
- **Modular Architecture**: Extensible design supporting various platforms and tools through modules.
- **Parameter Management**: System for defining, validating, and injecting build parameters.
- **Reporting**: Detailed build reporting integrated with CI/CD platform summaries.
- **.NET Tooling**: Built-in helpers for standard .NET operations (build, test, pack, publish).
- **Source Generation**: Automatic code generation for target and parameter discovery.

## Quick Start

### 1. Create a New Build Project

Create a new console application and add the Atom package.

```bash
dotnet new worker -n MyBuild

cd MyBuild

dotnet add package DecSm.Atom
````

### 2\. Define Your Build

Create a `Build.cs` file. The `[GenerateEntryPoint]` attribute automatically creates the `Main` method.

```csharp
namespace Build;

[DefaultBuildDefinition]
internal partial class Build : BuildDefinition
{
    private Target HelloWorld =>
        t => t
            .DescribedAs("Prints a hello world message")
            .Executes(() => Logger.LogInformation("Hello, World!"));
}
```

Targets can also be asynchronous:

```csharp
private Target HelloWorldAsync =>
    t => t
        .DescribedAs("Prints a hello world message asynchronously")
        .Executes(async () =>
        {
            await Task.Delay(1000);
            Logger.LogInformation("Hello, World!");
        });

private Target HelloWorldAsyncWithCancel =>
    t => t
        .DescribedAs("Prints a hello world message asynchronously")
        .Executes(async cancellationToken =>
        {
            await Task.Delay(1000, cancellationToken);
            Logger.LogInformation("Hello, World!");
        });
```

### 3\. Run Your Build

Execute the build project passing the target name as an argument.

```bash
dotnet run -- HelloWorld
```

## Documentation

### Build Definitions

Build definitions inherit from `BuildDefinition` (or `DefaultBuildDefinition`) and define targets as properties. `DefaultBuildDefinition` includes standard targets for setup and validation.

```csharp
[BuildDefinition]
internal partial class Build : BuildDefinition
{
    private Target Compile => t => t
        .DescribedAs("Compiles the project")
        .Executes(() =>
        {
            // Build logic
        });
}
```

### Parameters

Parameters allow external configuration of the build. They can be sourced from command-line arguments, environment variables, or configuration files.

```csharp
[ParamDefinition("my-name", "Name to greet")]
private string? MyName => GetParam(() => MyName);

private Target Hello => t => t
    .RequiresParam(nameof(MyName))
    .Executes(() =>
    {
        Logger.LogInformation("Hello, {Name}!", MyName);
    });
```

### Target Dependencies

Use `.DependsOn()` to define execution order requirements.

```csharp
private Target Test => t => t
    .DependsOn(Compile)
    .Executes(() =>
    {
        // Run tests after compilation
    });
```

### Target Interfaces

Targets can be organized into interfaces. The `[TargetDefinition]` attribute is required on the interface.

```csharp
using DecSm.Atom.Build.Definition;

[BuildDefinition]
internal partial class Build : BuildDefinition, ICompile, ITest;

[TargetDefinition]
public partial interface ICompile
{
    Target Compile => t => t
        .DescribedAs("Compiles the project")
        .Executes(() => { /* ... */ });
}

[TargetDefinition]
public partial interface ITest
{
    Target Test => t => t
        .DependsOn(nameof(ICompile.Compile)) // Reference dependency by name
        .Executes(() => { /* ... */ });
}
```

### Access Build Services

Implement `IBuildAccessor` to access services such as `Logger`, `FileSystem`, and `ProcessRunner`.

```csharp
[TargetDefinition]
public partial interface ICompile : IBuildAccessor
{
    Target Compile => t => t
        .DescribedAs("Compiles the project")
        .Executes(() =>
        {
            Logger.LogInformation("Compiling project...");
            FileSystem.File.Create("output.txt");
        });
}
```

### CI/CD Integration

Atom automatically generates CI/CD pipeline configurations based on your build definition. Configure workflows by overriding the `Workflows` property.

#### GitHub Actions

Add the GitHub Workflows module:

```bash
dotnet add package DecSm.Atom.Module.GithubWorkflows
```

Define the workflow:

```csharp
[BuildDefinition]
public partial class Build : BuildDefinition, IGithubWorkflows
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("ci")
        {
            Triggers = [new GitPullRequestTrigger { IncludedBranches = ["main"] }],
            Targets = [Targets.Test],
            WorkflowTypes = [Github.WorkflowType]
        }
    ];
}
```

#### Azure DevOps

Add the Azure DevOps Workflows module:

```bash
dotnet add package DecSm.Atom.Module.DevopsWorkflows
```

Define the workflow:

```csharp
[BuildDefinition]
public partial class Build : BuildDefinition, IDevopsWorkflows
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("ci")
        {
            Triggers = [new GitPullRequestTrigger { IncludedBranches = ["main"] }],
            Targets = [Targets.Test],
            WorkflowTypes = [Devops.WorkflowType]
        }
    ];
}
```

### Artifact Management

Atom supports automatic upload and download of artifacts in CI/CD pipelines.

```csharp
private Target Package => t => t
    .ProducesArtifact("MyPackage") // Workflows automatically upload this artifact
    .Executes(() => {
        // Create your package
        return Task.CompletedTask;
    });

private Target Deploy => t => t
    .ConsumesArtifact(nameof(Package), "MyPackage") // Workflows automatically download this artifact
    .Executes(() =>
    {
        // Deploy the package
    });
```

### Variable Management

Variables allow data sharing between build steps and are persisted in the workflow context.

```csharp
[ParamDefinition("my-name", "Name to greet")]
private string? MyName => GetParam(() => MyName);

private Target Info => t => t
    .ProducesVariable("MyPackage")
    .Executes(async () =>
    {
        // Variable writing is done manually via the helper service
        await Services.GetRequiredService<IVariablesHelper>().WriteVariable(nameof(MyName), "Declan");
    });

private Target Print => t => t
    .ConsumesVariable(nameof(Info), "MyPackage") // Workflows automatically inject this variable
    .Executes(() =>
    {
        Logger.LogInformation("Hello, {Name}!", MyName);
    });
```

## Modules

Atom provides several modules for extended functionality:

- **DecSm.Atom**: Core framework.
- **DecSm.Atom.Module.Dotnet**: Helpers for the .NET CLI.
- **DecSm.Atom.Module.GitVersion**: Integration with GitVersion for semantic versioning.
- **DecSm.Atom.Module.GithubWorkflows**: Support for GitHub Actions generation.
- **DecSm.Atom.Module.DevopsWorkflows**: Support for Azure DevOps Pipelines generation.
- **DecSm.Atom.Module.AzureKeyVault**: Azure Key Vault integration for secrets.
- **DecSm.Atom.Module.AzureStorage**: Azure Blob Storage provider for artifacts.

To use a module, reference the package and implement the corresponding interface in your build definition (e.g., `IDotnetPackHelper`, `IAzureKeyVault`).

## Advanced Features

### Matrix Builds

Run targets across multiple configurations using matrix strategies.

```csharp
public override IReadOnlyList<WorkflowDefinition> Workflows =>
[
    new("build")
    {
        Targets =
        [
            Targets.Test.WithMatrixDimensions(
                new MatrixDimension("os", ["ubuntu-latest", "windows-latest", "macos-latest"])),
        ],
        // ...
    }
];
```

### Custom Artifact Providers

Configure custom artifact storage backends (e.g., Azure Blob Storage) by enabling the `UseCustomArtifactProvider` option in your workflow.

```csharp
public override IReadOnlyList<WorkflowDefinition> Workflows =>
[
    new("build")
    {
        Options = [UseCustomArtifactProvider.Enabled],
        // ... other configuration
    }
];
```

### Secret Management

Integrate with Azure Key Vault for secure parameter resolution.

Install the module:

```bash
dotnet add package DecSm.Atom.Module.AzureKeyVault
```

Implement the interface and define secrets:

```csharp
[BuildDefinition]
public partial class Build : BuildDefinition, IAzureKeyVault
{
    [SecretDefinition("my-secret", "Description of the secret")]
    private string MySecret => GetParam(() => MySecret);
}
```

## Examples

Sample projects are available in the repository:

- **Sample\_01\_HelloWorld**: Basic hello world example.
- **Sample\_02\_Params**: Parameter usage and configuration.

## Development

### Requirements

- .NET 8.0 or later.

### Building from Source

To build the Atom framework itself:

```bash
git clone [https://github.com/DecSmith42/atom.git](https://github.com/DecSmith42/atom.git)
cd atom
dotnet run --project _atom -- PackProjects
```

This executes the `PackProjects` target defined in `_atom/Targets/IBuildTargets.cs`.

### Running Tests

To run the unit tests:

```bash
dotnet run --project _atom -- TestProjects
```

## License

Atom is released under the MIT License. See the [LICENSE](https://www.google.com/search?q=LICENSE.txt) file for details.

```
```