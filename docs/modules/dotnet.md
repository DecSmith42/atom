# .NET Module

The `DecSm.Atom.Module.Dotnet` module provides comprehensive integration with the .NET CLI, offering a powerful and type-safe way to interact with `dotnet` commands within your DecSm.Atom build definitions. This module simplifies common .NET development tasks such as building, testing, publishing, packing NuGet packages, and managing .NET tools.

## Features

*   **Type-Safe .NET CLI Interaction**: Execute `dotnet` commands (e.g., `build`, `test`, `publish`, `pack`) with strongly-typed options, reducing errors and improving readability.
*   **Automated Tool Installation**: Automatically installs or updates .NET CLI tools (e.g., `dotnet-reportgenerator-globaltool`) as needed.
*   **Project Versioning**: Seamlessly injects build version information into project files before compilation, packing, or publishing.
*   **Artifact Staging**: Automatically stages build outputs (binaries, test results, NuGet packages) into designated Atom directories for further processing or publishing.
*   **NuGet Helper**: Provides utilities for pushing NuGet packages to feeds and managing `NuGet.Config` files.
*   **Detailed Reporting**: Generates comprehensive test and code coverage reports, integrating them into the build's outcome summary.
*   **Flexible Configuration**: Offers extensive options for customizing command execution, including working directories, logging levels, environment variables, and output transformations.

## Getting Started

To use the .NET module, you typically implement the `IDotnetCliHelper` interface in your build definition. This provides access to the `DotnetCli` service, which is the entry point for most .NET CLI operations. For more specific tasks, you can implement additional helper interfaces like `IDotnetPackHelper`, `IDotnetPublishHelper`, `IDotnetTestHelper`, and `INugetHelper`.

### Implementation

Add the `IDotnetCliHelper` interface to your `Build.cs` file:

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.Dotnet.Helpers;

namespace Atom;

[BuildDefinition]
internal partial class Build : IDotnetCliHelper
{
    // Your build targets and other definitions
}
```

## .NET CLI Commands

The `IDotnetCli` service (accessed via `DotnetCli` property) provides methods for various `dotnet` commands. These methods are generated and offer strongly-typed options for each command.

### Example: Building a Project

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.Dotnet.Helpers;
using DecSm.Atom.Module.Dotnet.Cli.Generated; // For BuildOptions

namespace Atom;

[BuildDefinition]
internal partial class Build : IDotnetCliHelper
{
    private Target BuildProject =>
        t => t
            .DescribedAs("Builds the main project")
            .Executes(async () =>
            {
                await DotnetCli.Build("src/MyProject/MyProject.csproj", new BuildOptions
                {
                    Configuration = "Release",
                    NoRestore = true // Assuming restore was done separately
                });
                Logger.LogInformation("Project built successfully.");
            });
}
```

## Packing and Staging NuGet Packages

The `IDotnetPackHelper` interface provides methods to pack .NET projects into NuGet packages and stage them in the Atom artifacts directory.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.Dotnet.Helpers;
using DecSm.Atom.Module.Dotnet.Cli.Generated; // For PackOptions

namespace Atom;

[BuildDefinition]
internal partial class Build : IDotnetPackHelper
{
    private Target PackProject =>
        t => t
            .DescribedAs("Packs the NuGet project")
            .Executes(async () =>
            {
                await DotnetPackAndStage("MyNugetProject", new DotnetPackAndStageOptions
                {
                    PackOptions = new PackOptions { NoBuild = true }, // Assuming build was done separately
                    SetVersionsFromProviders = true // Injects BuildVersion into project file
                });
                Logger.LogInformation("NuGet package packed and staged.");
            });
}
```

## Testing and Staging Results

The `IDotnetTestHelper` interface allows you to run tests, collect results, and optionally generate code coverage reports.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.Dotnet.Helpers;
using DecSm.Atom.Module.Dotnet.Cli.Generated; // For TestOptions

namespace Atom;

[BuildDefinition]
internal partial class Build : IDotnetTestHelper
{
    private Target RunTests =>
        t => t
            .DescribedAs("Runs unit tests and collects coverage")
            .Executes(async () =>
            {
                var exitCode = await DotnetTestAndStage("MyProject.Tests", new DotnetTestAndStageOptions
                {
                    IncludeCoverage = true,
                    TestOptions = new TestOptions { Configuration = "Debug" }
                });

                if (exitCode != 0)
                {
                    Logger.LogError("Tests failed!");
                }
                else
                {
                    Logger.LogInformation("Tests passed and coverage collected.");
                }
            });
}
```

## Publishing and Staging Applications

The `IDotnetPublishHelper` interface helps publish .NET applications and stage their output.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.Dotnet.Helpers;
using DecSm.Atom.Module.Dotnet.Cli.Generated; // For PublishOptions

namespace Atom;

[BuildDefinition]
internal partial class Build : IDotnetPublishHelper
{
    private Target PublishApp =>
        t => t
            .DescribedAs("Publishes the web application")
            .Executes(async () =>
            {
                await DotnetPublishAndStage("MyWebApp", new DotnetPublishAndStageOptions
                {
                    PublishOptions = new PublishOptions
                    {
                        Configuration = "Release",
                        Runtime = "win-x64",
                        SelfContained = true
                    }
                });
                Logger.LogInformation("Web application published and staged.");
            });
}
```

## NuGet Operations

The `INugetHelper` interface provides methods for interacting with NuGet feeds.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.Dotnet.Helpers;
using DecSm.Atom.Module.Dotnet.Model; // For NugetFeed

namespace Atom;

[BuildDefinition]
internal partial class Build : INugetHelper
{
    private Target PushNugetPackages =>
        t => t
            .DescribedAs("Pushes NuGet packages to a feed")
            .Executes(async () =>
            {
                var myNugetFeed = new NugetFeed(
                    url: "https://api.nuget.org/v3/index.json",
                    name: "NuGet.org",
                    apiKey: "YOUR_NUGET_API_KEY" // Should be sourced from a secret
                );

                await PushProject("MyNugetProject", myNugetFeed.Url, myNugetFeed.Password());
                Logger.LogInformation("NuGet package pushed to feed.");
            });
}
```

## Tool Installation

The `IDotnetToolInstallHelper` interface simplifies the installation and management of .NET CLI tools.

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.Dotnet.Helpers;

namespace Atom;

[BuildDefinition]
internal partial class Build : IDotnetToolInstallHelper
{
    private Target InstallTools =>
        t => t
            .DescribedAs("Installs required .NET CLI tools")
            .Executes(() =>
            {
                InstallTool("dotnet-format", version: "7.0.0", global: true);
                Logger.LogInformation("dotnet-format installed.");
            });
}
```
