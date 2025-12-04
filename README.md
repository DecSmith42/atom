# Atom

[![Validate](https://github.com/DecSmith42/atom/actions/workflows/Validate.yml/badge.svg)](https://github.com/DecSmith42/atom/actions/workflows/Validate.yml)
[![Build](https://github.com/DecSmith42/atom/actions/workflows/Build.yml/badge.svg)](https://github.com/DecSmith42/atom/actions/workflows/Build.yml)
[![Dependabot Updates](https://github.com/DecSmith42/atom/actions/workflows/dependabot/dependabot-updates/badge.svg)](https://github.com/DecSmith42/atom/actions/workflows/dependabot/dependabot-updates)

Atom is an opinionated, type-safe build automation framework for .NET. It enables you to define your build logic in C#, debug it like standard code, and automatically generate CI/CD configuration files for GitHub Actions and Azure DevOps.

## Why Atom?

* **Zero Context Switching**: Write build logic in C# alongside your application code.
* **Intellisense & Debugging**: Step through your build process using your IDE.
* **CI/CD Agnostic**: Define logic once; Atom generates the YAML for GitHub and Azure DevOps.
* **Modular**: Pull in capabilities via NuGet packages (GitVersion, Azure KeyVault, etc.).
* **Source Generators**: Reduces boilerplate by automatically discovering targets and parameters.

## Getting Started

### 1. Prerequisites

* .NET SDK 8.0 or later.

### 2. Setup the Build Project

It is recommended to place your build project in a folder named `_atom` at the root of your repository.

> [!NOTE]
> The folder name is arbitrary and there can be multiple build projects in a repository.

```bash
# Create a generic worker (console) application
dotnet new worker -n _atom

# Add the Atom core package
dotnet add package DecSm.Atom
```

### 3. Define Your Build

Replace the contents of `Program.cs` (or create `Build.cs`) with a basic definition. The `[GenerateEntryPoint]` attribute handles the `Main` method for you.

```csharp
namespace Atom;

[BuildDefinition]
internal partial class Build
{
    private Target HelloWorld => t => t
        .DescribedAs("The hello world target")
        .Executes(() => Logger.LogInformation("Hello, Atom!"));
}
```

### 4. Run the Build

You can run the build directly using the `dotnet run` command.

```bash
# Run the HelloWorld target
dotnet run --project _atom -- HelloWorld

# List all available targets
dotnet run --project _atom -- --help
```

### 5. Use the Atom tool

The tool can be installed locally or globally to simplify running the build from the command line.

```bash
# Install
dotnet tool install --global DecSm.Atom.Tool

# Run
atom HelloWorld

# List all available targets
atom --help
```

Using .NET 10+ SDK, you can also run the tool without installing.

```bash
dnx decsm.atom.tool
```

## Core Concepts

### Targets & Dependencies

Targets are properties that return a `Target` delegate. You can chain dependencies to ensure execution order.

```csharp
Target Clean => t => t.Executes(() => FileSystem.Directory.Delete("bin", recursive: true));

Target Restore => t => t
    .DependsOn(Clean)
    .Executes(() => ProcessRunner.Run(new("dotnet", "restore")));

Target Compile => t => t
    .DependsOn(Restore)
    .Executes(() => ProcessRunner.Run(new("dotnet", "build --no-restore")));
```

### Parameters & Secrets

Define parameters using attributes. Atom automatically parses command-line arguments (e.g., `--configuration Release`) and environment variables.

```csharp
[ParamDefinition("configuration", "Build configuration")]
string Configuration => GetParam(() => Configuration, "Release");

[SecretDefinition("api-key", "API Key for publishing")]
string ApiKey => GetParam(() => ApiKey);
```

## Conventions & Best Practices

To maintain consistency and leverage Atom's source generators effectively, follow these standard conventions:

* **Directory Structure**: Place your build project in a dedicated `_atom` folder at the root of your repository.
* **Class Inheritance**: Your main build class should marked with the `[BuildDefinition]` attribute.
* **Partial Classes**: Declare your build class as `partial`. Atom's source generators use this to inject boilerplate code for parameter and target discovery.
* **Target Definition**: Define targets as properties returning a `Target` delegate.
* **Modular Interfaces**: Use interfaces that inherit `IBuildAccessor` to access common build functionality (e.g., `Logger`, `FileSystem`).
* **Parameter Naming**: Use kebab-case for the parameter name in the attribute (e.g., `[ParamDefinition("my-param", ...)]`) and PascalCase for the C# property.
* **Helpers**: Interfaces that provide params and methods that aren't targets themselves should be suffixed with `Helper` to make the intent clear.

## Modules & Integrations

Atom functionality is distributed across several NuGet packages. Mix and match what you need.

### 1. .NET Tooling (DecSm.Atom.Module.Dotnet)

Provides helpers for `dotnet` CLI commands (Restore, Build, Test, Pack, Publish).

**Setup:**

```bash
dotnet add package DecSm.Atom.Module.Dotnet
```

**Usage:**
Implement `IDotnetCliHelper` or specific helpers like `IDotnetTestHelper`.

```csharp
[BuildDefinition]
partial class Build : IDotnetBuildHelper
{
    Target Compile => t => t
        .Executes(async cancellationToken => 
        {
            // Use the auto-generated typed CLI wrapper
            // The `Projects` class contains source-generated strongly typed references to all projects in the solution
            await DotnetCli.Build(Projects.MyCompany_Project, new BuildOptions 
            { 
                Configuration = "Release" 
            }, cancellationToken: cancellationToken);
        });
}
```

### 2. Versioning (DecSm.Atom.Module.GitVersion)

Automatically calculates Semantic Versioning using Git history.

**Setup:**

```bash
dotnet add package DecSm.Atom.Module.GitVersion
```

**Usage:**
Implement `IGitVersion` and enable the global option `UseGitVersionForBuildId`.

```csharp
[BuildDefinition]
partial class Build : BuildDefinition, IGitVersion
{
    // Enable GitVersion globally
    public override IReadOnlyList<IWorkflowOption> GlobalWorkflowOptions => 
    [ 
        UseGitVersionForBuildId.Enabled 
    ];

    Target PrintVersion => t => t
        .Executes(() => Logger.LogInformation("Version: {Version}", BuildVersion));
}
```

### 3. Azure Key Vault (DecSm.Atom.Module.AzureKeyVault)

Securely fetch secrets from Key Vault during local runs or CI pipelines.

**Setup:**

```bash
dotnet add package DecSm.Atom.Module.AzureKeyVault
```

**Usage:**
Implement `IAzureKeyVault`. Secrets defined with `[SecretDefinition]` will attempt to resolve from the vault if not found locally.

Build.cs:
```csharp
[BuildDefinition]
partial class Build : BuildDefinition, IAzureKeyVault
{
    public override IReadOnlyList<IWorkflowOption> GlobalWorkflowOptions => 
    [ 
        UseAzureKeyVault.Enabled 
    ];
    
    [SecretDefinition("my-secret", "A secret stored in Azure Key Vault")]
    string MySecret => GetParam(() => MySecret);
    
    Target PrintSecret => t => t
        .RequiresParam(nameof(MySecret))
        .Executes(() => 
        {
            // For safety, all params marked with `[SecretDefinition]` are automatically redacted from logs and console output
            // However, it is still recommended to avoid logging secrets
            Logger.LogInformation("MySecret: {SecretValue}", MySecret);
        });
}
```

AppSettings.json:
```json
{
  "AzureKeyVault": {
    "VaultUri": "https://my-vault.vault.azure.net/"
  }
}
```

### 4. Artifacts (DecSm.Atom.Module.AzureStorage)

Store artifacts in Azure Blob Storage, abstracting away the underlying CI provider's artifact mechanism.

**Usage:**
Enable `UseCustomArtifactProvider` in your workflow options.

## CI/CD Generation

Atom generates workflow YAML files by introspecting your C# code.

### Example: Build, Test, & Publish (GitHub & Azure DevOps)

This example demonstrates a complete lifecycle build that runs on both platforms.

**Required Packages:**

* `DecSm.Atom.Module.GithubWorkflows`
* `DecSm.Atom.Module.DevopsWorkflows`

**Build.cs:**

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.DevopsWorkflows;
using DecSm.Atom.Module.GithubWorkflows;
using DecSm.Atom.Workflows.Definition;
using DecSm.Atom.Workflows.Definition.Triggers;

namespace Atom;

[GenerateEntryPoint]
[BuildDefinition]
internal partial class Build : BuildDefinition, 
    IGithubWorkflows, // Adds GitHub Actions support
    IDevopsWorkflows  // Adds Azure DevOps support
{
    // Define the targets
    
    Target Compile => t => t
        .DescribedAs("Compiles the solution")
        .DependsOn(SetupBuildInfo)
        .Executes(() => ProcessRunner.Run(new("dotnet", "build -c Release")));

    Target Test => t => t
        .DependsOn(Compile)
        .Executes(() => ProcessRunner.Run(new("dotnet", "test -c Release --no-build")));

    Target Pack => t => t
        .DependsOn(Test)
        .Executes(() => ProcessRunner.Run(new("dotnet", "pack -c Release --no-build")));

    // Define the Workflows
    
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("ContinuousIntegration")
        {
            // Trigger on Push to Main or Pull Requests
            Triggers = 
            [
                GitPushTrigger.ToMain,
                GitPullRequestTrigger.IntoMain 
            ],
            
            // The sequence of targets to run
            Targets = 
            [
                Targets.Compile,
                Targets.Test,
                Targets.Pack
            ],
            
            // Generate YAML for BOTH platforms
            WorkflowTypes = 
            [
                Github.WorkflowType, 
                Devops.WorkflowType 
            ]
        }
    ];
}
```

### Generating the Workflows

Run the generation command:

```bash
# Dotnet cli
dotnet run --project _atom -- -g

# Or atom tool
atom -g
```

Atom will create:

1.  `.github/workflows/ContinuousIntegration.yml`
2.  `.devops/workflows/ContinuousIntegration.yml`

These files are "dirty-checked". If you change your C# logic, Atom will warn you (or fail in CI) if the YAML files are out of date.

## Advanced Examples

### Build Matrix

Run tests across multiple Operating Systems.

```csharp
public override IReadOnlyList<WorkflowDefinition> Workflows =>
[
    new("MatrixTest")
    {
        Triggers = [GitPushTrigger.ToMain],
        Targets = 
        [
            // Creates a matrix strategy for this target
            Targets.Test.WithGithubRunnerMatrix([IJobRunsOn.WindowsLatest, IJobRunsOn.UbuntuLatest])
        ],
        WorkflowTypes = [Github.WorkflowType]
    }
];
```

### Secret Injection

Inject secrets into the workflow environment securely.

```csharp
new("Deploy")
{
    Targets = [Targets.Publish],
    // Inject the 'nuget-key' secret from the provider into the environment
    Options = [WorkflowSecretInjection.Create("nuget-key")], 
    WorkflowTypes = [Github.WorkflowType]
}
```

## License

Atom is released under the [MIT License](https://www.google.com/search?q=LICENSE.txt).