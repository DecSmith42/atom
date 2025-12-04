# Atom

[![Build](https://github.com/DecSmith42/atom/actions/workflows/Build.yml/badge.svg)](https://github.com/DecSmith42/atom/actions/workflows/Build.yml)

Atom is an opinionated task and build automation framework, written in C#.

Inspired by the likes of [NUKE](https://nuke.build/), Atom aims to provide a flexible, extensible framework for defining
and executing build tasks. It leverages .NET and provides a comprehensive set of features for automating your
development workflow with automatic CI/CD pipeline generation e.g. for GitHub Actions and Azure DevOps.

## ✨ Features

- **🎯 Type-Safe Build Definitions**: Define build targets using strongly-typed C# interfaces and classes
- **🔄 Automatic CI/CD Generation**: Generate GitHub Actions and Azure DevOps pipelines from your build definitions
- **📦 Artifact Management**: Built-in support for artifact storage and retrieval with CI/CD host or a custom provider
- **🔐 Secret Management**: Secure secret handling with .NET secrets or custom providers
- **🧩 Modular Architecture**: Extensible module system for different platforms and tools
- **⚙️ Parameter Management**: Flexible parameter system with validation and optional interactive prompting
- **📊 Reporting**: Comprehensive build reporting with CI/CD platform integration
- **🔨 .NET Tooling**: Built-in support for .NET operations (build, test, pack, publish)
- **📝 Source Generation**: Automatic code generation for build definitions

## 🚀 Quick Start

### 1. Create a New Build Project

Create a new console application and add the Atom package:

```bash
dotnet new console -n MyBuild
cd MyBuild
dotnet add package DecSm.Atom
```

### 2. Define Your Build

Create a `Build.cs` file:

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Hosting;

namespace MyBuild;

[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : BuildDefinition
{
    private Target HelloWorld =>
        t => t
            .DescribedAs("Prints a hello world message")
            .Executes(() => Logger.LogInformation("Hello, World!"));
}
```

Targets can also be async:

```csharp
private Target HelloWorldAsync =>
    t => t
        .DescribedAs("Prints a hello world message asynchronously")
        .Executes(async () =>
        {
            await Task.Delay(1000); // Simulate async work
            Logger.LogInformation("Hello, World!");
        });

private Target HelloWorldAsyncWithCancel =>
    t => t
        .DescribedAs("Prints a hello world message asynchronously")
        .Executes(async cancellationToken =>
        {
            await Task.Delay(1000, cancellationToken); // Simulate async work with cancellation
            Logger.LogInformation("Hello, World!");
        });
```

### 3. Run Your Build

```bash
dotnet run -- HelloWorld
```

## 📚 Documentation

### Basic Concepts

#### Build Definitions

Build definitions are classes that inherit from `BuildDefinition` and define targets (build steps) as properties:

```csharp
[BuildDefinition]
internal partial class Build : BuildDefinition
{
    private Target Compile => t => t
        .DescribedAs("Compiles the project")
        .Executes(() =>
        {
            // Your build logic here
        });
}
```

#### Parameters

Define and use parameters in your builds:

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

#### Target Dependencies

Define dependencies between targets:

```csharp
private Target Test => t => t
    .DependsOn(Compile)
    .Executes(() =>
    {
        // Run tests after compilation
    });
```

### Target Interfaces

You can also define targets using interfaces for better organization:

```csharp
using DecSm.Atom.Build.Definition;
[BuildDefinition]
internal partial class Build : BuildDefinition, ICompile, ITest;

public interface ICompile
{
    Target Compile => t => t
        .DescribedAs("Compiles the project")
        .Executes(() =>
        {
            // Your build logic here
        });
}

public interface ITest
{
    Target Test => t => t
        .DependsOn(Compile)
        .Executes(() =>
        {
            // Run tests after compilation
        });
}
```

### Access Build Services

You can access various build services like logging, parameters, and secrets:

```csharp
public interface ICompile : IBuildAccessor
{
    Target Compile => t => t
        .DescribedAs("Compiles the project")
        .Executes(() =>
        {
            Logger.LogInformation("Compiling project...");
            FileSystem.File.Create("output.txt");
            Services.Get<IService>().DoSomething();
        });
}
```

### CI/CD Integration

Atom can automatically generate CI/CD pipelines for your builds:

#### GitHub Actions

> Install the required modules for your CI/CD platform (GitHub Actions, Azure DevOps, etc.):

```bash
dotnet add package DecSm.Atom.Module.GithubWorkflows
```

```csharp
[BuildDefinition]
public partial class Build : BuildDefinition, IDevopsWorkflows
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("ci")
        {
            Triggers = [new GitPullRequestTrigger { IncludedBranches = ["main"] }],
            StepDefinitions = [Targets.Test],
            WorkflowTypes = [Devops.WorkflowType]
        }
    ];
}
```

#### Azure DevOps

> Install the required modules for your CI/CD platform (GitHub Actions, Azure DevOps, etc.):

```bash
dotnet add package DecSm.Atom.Module.DevopsWorkflows
```

```csharp
[BuildDefinition]
public partial class Build : BuildDefinition, IDevopsWorkflows
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("ci")
        {
            Triggers = [new GitPullRequestTrigger { IncludedBranches = ["main"] }],
            StepDefinitions = [Targets.Test],
            WorkflowTypes = [Devops.WorkflowType]
        }
    ];
}
```

## 🧩 Modules

Atom provides several modules for different functionalities:

### Core Modules

- **DecSm.Atom**: Core framework
- **DecSm.Atom.Module.Dotnet**: .NET tooling support
- **DecSm.Atom.Module.GitVersion**: GitVersion integration

### CI/CD Modules

- **DecSm.Atom.Module.GithubWorkflows**: GitHub Actions support
- **DecSm.Atom.Module.DevopsWorkflows**: Azure DevOps support

### Cloud Modules

- **DecSm.Atom.Module.AzureKeyVault**: Azure Key Vault integration
- **DecSm.Atom.Module.AzureStorage**: Azure Blob Storage for artifacts

### Using Modules

Add modules to your build definition:

```csharp
[BuildDefinition]
public partial class Build : BuildDefinition, 
    IDevopsWorkflows,
    IDotnetPackHelper,
    IAzureKeyVault
{
    // Your build targets here
}
```

## 📦 Artifact Management

Atom supports artifact management with automatic upload/download in CI/CD pipelines:

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

## Variable Management

Atom supports variable management for build parameters and secrets:

```csharp
[ParamDefinition("my-name", "Name to greet")]
private string? MyName => GetParam(() => MyName);

private Target Info => t => t
    .ProducesVariable("MyPackage")
    .Executes(() =>
    {
        // Variable writing is done manually
        Services.GetRequiredService<IVariablesHelper>().WriteVariable(nameof(MyName), "Declan");
    });

private Target Print => t => t
    .ConsumesVariable(nameof(Info), "MyPackage") // Workflows automatically inject this variable
    .Executes(() =>
    {
        // Using the variable
        Logger.LogInformation("Hello, {Name}!", MyName);
        // output: Hello, Declan!
    });
```

## 🔧 Advanced Features

### Matrix Builds

Run builds across multiple configurations:

```csharp
public override IReadOnlyList<WorkflowDefinition> Workflows =>
[
    new("build")
    {
		StepDefinitions =
		[
			Targets.Test.WithMatrixDimensions(
				new MatrixDimension("os", ["ubuntu-latest", "windows-latest", "macos-latest"])),
        ],
    }
];

```

### Custom Artifact Providers

Use custom artifact storage backends:

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

Integrate with Azure Key Vault:

> Install the required module for Azure Key Vault:

```bash
dotnet add package DecSm.Atom.Module.AzureKeyVault
```

```csharp
[BuildDefinition]
public partial class Build : BuildDefinition, IAzureKeyVault
{
    [SecretDefinition("my-secret", "Description of the secret")]
    private string MySecret => GetSecret(() => MySecret);
}
```

## 📋 Examples

Check out the sample projects:

- **Sample_01_HelloWorld**: Basic hello world example
- **Sample_02_Params**: Parameter usage and configuration

## 🛠️ Development

### Requirements

- .NET 8.0 or 9.0
- Visual Studio or VS Code or Rider or any other C# IDE

### Building from Source

Atom can build itself!

```bash
git clone https://github.com/DecSmith42/atom.git
cd atom
dotnet run --project _atom -- PackAtom
```

Even faster if you've installed the Atom tool globally:

```bash
atom PackAtom
```

Or the old-fashioned way:

```bash
dotnet build _atom/Atom.csproj
```

### Running Tests

```bash
dotnet run --project _atom -- TestAtom
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

Atom is released under the MIT License. See the [LICENSE](LICENSE.txt) file for details.
