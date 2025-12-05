# Azure Storage Module

The `DecSm.Atom.Module.AzureStorage` module provides robust capabilities for managing build artifacts using Azure Blob Storage. This integration allows your DecSm.Atom build processes to securely store and retrieve build outputs, ensuring persistence and accessibility across different build runs and environments.

## Features

*   **Artifact Storage**: Upload build artifacts to a specified Azure Blob Storage container.
*   **Artifact Retrieval**: Download previously stored artifacts from Azure Blob Storage.
*   **Cleanup Functionality**: Remove artifacts associated with specific build runs from storage.
*   **Parameter-driven Configuration**: Configure Azure Storage connection details and container names using build parameters.
*   **Build ID and Slice Integration**: Organizes artifacts within the storage container based on build ID and optional build slices for easy management and retrieval.

## Getting Started

To use the Azure Storage module, you need to implement the `IAzureArtifactStorage` interface in your build definition. This interface provides the necessary parameters for connecting to your Azure Storage account and container.

### Prerequisites

Before you begin, ensure you have:

1.  An Azure Storage account.
2.  A Blob Storage container within that account where artifacts will be stored.
3.  The connection string for your Azure Storage account.

### Implementation

Add the `IAzureArtifactStorage` interface to your `Build.cs` file:

```csharp
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.AzureStorage;

namespace Atom;

[BuildDefinition]
internal partial class Build : IAzureArtifactStorage
{
    // Your build targets and other definitions
}
```

## Configuration Parameters

The `IAzureArtifactStorage` interface exposes the following parameters, which you can set via command-line arguments, environment variables, or `appsettings.json`:

*   **`azurestorage-artifact-connectionstring`**: The connection string for your Azure Storage account. This is a `SecretDefinition` and should be handled securely.
*   **`azurestorage-artifact-container`**: The name of the Azure Blob Storage container where artifacts will be stored (e.g., `build-artifacts`).

### Example `appsettings.json` Configuration

```json
{
  "Params": {
    "azurestorage-artifact-container": "my-build-artifacts",
    
    // Do not hardcode sensitive information in appsettings.json, this is just an example
    "azurestorage-artifact-connectionstring": "DefaultEndpointsProtocol=https;AccountName=youraccountname;AccountKey=YOUR_ACCOUNT_KEY;EndpointSuffix=core.windows.net"
  }
}
```

**Note**: The `azurestorage-artifact-connectionstring` is a sensitive credential and should always be stored securely, preferably in the `Secrets` section of `appsettings.json` or as a secret environment variable in your CI/CD system.

### Command-Line Example

```bash
dotnet run -- MyTarget --azurestorage-artifact-container "my-build-artifacts" --azurestorage-artifact-connectionstring "DefaultEndpointsProtocol=https;AccountName=youraccountname;AccountKey=YOUR_ACCOUNT_KEY;EndpointSuffix=core.windows.net"
```

## Storing Artifacts

To store artifacts, you can use the `StoreArtifacts` method provided by the `IArtifactProvider` interface. The `AzureBlobArtifactProvider` is automatically registered when you implement `IAzureArtifactStorage`.

Artifacts are expected to be located in the `AtomPublishDirectory` (typically `_output/publish` relative to your repository root). Each artifact name you provide to `StoreArtifacts` should correspond to a subdirectory within this publish directory.

```csharp
using DecSm.Atom.Artifacts; // Required for IArtifactProvider
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.AzureStorage;

namespace Atom;

[BuildDefinition]
internal partial class Build : IAzureArtifactStorage
{
    private Target PublishArtifacts =>
        t => t
            .DescribedAs("Publishes build artifacts to Azure Blob Storage")
            .Executes(async () =>
            {
                // Assuming you have directories like _output/publish/MyApp and _output/publish/MyDocs
                await ArtifactProvider.StoreArtifacts(new[] { "MyApp", "MyDocs" });

                Logger.LogInformation("Artifacts published to Azure Blob Storage.");
            });
}
```

Artifacts are stored in a hierarchical structure within the container:
`<buildName>/<buildIdGroup>/<buildId>/<artifactName>/<buildSlice>/<filePath>`

For example: `MyProject/2023/20231027-123456/MyApp/main/app.zip`

## Retrieving Artifacts

To retrieve artifacts, use the `RetrieveArtifacts` method. This will download the specified artifacts into your `AtomArtifactsDirectory` (typically `_output/artifacts`).

```csharp
using DecSm.Atom.Artifacts;
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.AzureStorage;

namespace Atom;

[BuildDefinition]
internal partial class Build : IAzureArtifactStorage
{
    private Target DownloadArtifacts =>
        t => t
            .DescribedAs("Downloads build artifacts from Azure Blob Storage")
            .Executes(async () =>
            {
                // Download artifacts from a specific build ID and slice
                await ArtifactProvider.RetrieveArtifacts(
                    new[] { "MyApp", "MyDocs" },
                    buildId: "20231027-123456",
                    buildSlice: "main");

                Logger.LogInformation("Artifacts downloaded from Azure Blob Storage.");
            });
}
```

## Cleaning Up Artifacts

The `Cleanup` method allows you to remove artifacts associated with one or more build IDs.

```csharp
using DecSm.Atom.Artifacts;
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.AzureStorage;

namespace Atom;

[BuildDefinition]
internal partial class Build : IAzureArtifactStorage
{
    private Target CleanOldArtifacts =>
        t => t
            .DescribedAs("Cleans up old build artifacts from Azure Blob Storage")
            .Executes(async () =>
            {
                // Clean up artifacts for a list of build IDs
                var oldBuildIds = new[] { "20231020-090000", "20231021-100000" };
                await ArtifactProvider.Cleanup(oldBuildIds);

                Logger.LogInformation("Cleaned up artifacts for build IDs: {BuildIds}", string.Join(", ", oldBuildIds));
            });
}
```

## Getting Stored Run Identifiers

You can query for existing build IDs that have stored artifacts using `GetStoredRunIdentifiers`. This can be useful for managing or reporting on available artifacts.

```csharp
using DecSm.Atom.Artifacts;
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Module.AzureStorage;

namespace Atom;

[BuildDefinition]
internal partial class Build : IAzureArtifactStorage
{
    private Target ListArtifacts =>
        t => t
            .DescribedAs("Lists stored artifact run identifiers")
            .Executes(async () =>
            {
                var allRunIds = await ArtifactProvider.GetStoredRunIdentifiers();
                Logger.LogInformation("All stored run IDs: {RunIds}", string.Join(", ", allRunIds));

                var appRunIds = await ArtifactProvider.GetStoredRunIdentifiers(artifactName: "MyApp");
                Logger.LogInformation("Run IDs with 'MyApp' artifacts: {RunIds}", string.Join(", ", appRunIds));
            });
}
```
