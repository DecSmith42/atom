# Build Information Providers

Atom's build information system relies on a set of provider interfaces to dynamically determine critical metadata about
your build. These providers allow you to customize how the build ID, version, and timestamp are generated or retrieved,
ensuring flexibility and integration with various versioning strategies or CI/CD environments.

## `IBuildIdProvider`

The `IBuildIdProvider` interface defines how Atom obtains a unique identifier for each build run.

**What it is:**
A contract for a service that supplies a unique string representing the current build's ID.

**What it does:**

* **`BuildId`**: A string property that returns the unique build identifier. This ID should be consistent within a
  single build run but unique across different builds.
* **`GetBuildIdGroup(string buildId)`**: An optional method to return a grouping key for a given build ID. This can be
  used to organize artifacts or logs (e.g., grouping by month for date-based IDs).

**When to use it:**

* To integrate with CI/CD systems that provide their own unique build IDs (e.g., GitHub Actions `GITHUB_RUN_ID`, Azure
  DevOps `BUILD_BUILDID`).
* To implement custom build ID generation logic (e.g., based on Git commit hashes, sequential numbers).

**Default Implementation (`DefaultBuildIdProvider`):**
Atom's default provider combines the build version and timestamp (e.g., `1.2.3-1672531200`).

## `IBuildVersionProvider`

The `IBuildVersionProvider` interface defines how Atom obtains the semantic version for your build.

**What it is:**
A contract for a service that supplies a `SemVer` object representing the current build's version.

**What it does:**

* **`Version`**: A `SemVer` property that returns the semantic version of the build.

**When to use it:**

* To integrate with versioning tools like GitVersion.
* To implement custom versioning logic (e.g., reading from a specific file, API).

**Default Implementation (`DefaultBuildVersionProvider`):**
Atom's default provider attempts to extract the version from your project's `Directory.Build.props` file, looking for
common MSBuild properties like `InformationalVersion`, `PackageVersion`, `Version`, `VersionPrefix`, and
`VersionSuffix`. It falls back to `1.0.0` if no version can be determined.

## `IBuildTimestampProvider`

The `IBuildTimestampProvider` interface defines how Atom obtains a consistent timestamp for your build.

**What it is:**
A contract for a service that supplies a Unix epoch timestamp (seconds since UTC epoch) for the current build.

**What it does:**

* **`Timestamp`**: A `long` property that returns the build timestamp. This value is typically generated once at the
  start of the build and remains consistent throughout.

**When to use it:**

* To ensure all parts of your build use the exact same timestamp, which is crucial for reproducibility and caching.
* To integrate with CI/CD systems that provide a specific build start time.

**Default Implementation (`DefaultBuildTimestampProvider`):**
Atom's default provider generates a timestamp based on the current UTC time upon first access and caches it for
consistency within a single process.

## How to use and customize Build Information Providers

You typically don't interact with these providers directly in your targets. Instead, the `ISetupBuildInfo` target (which
is part of `BuildDefinition`) uses these providers to resolve the `BuildId`, `BuildVersion`, and `BuildTimestamp`
parameters.

To customize how these values are determined, you can register your own implementations of these interfaces in your
build definition's `ConfigureServices` method.

```csharp
// In _atom/Build.cs
[BuildDefinition]
public partial class MyBuild : BuildDefinition
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        // Register a custom build ID provider
        services.AddSingleton<IBuildIdProvider, MyCustomBuildIdProvider>();

        // Register a custom build version provider
        services.AddSingleton<IBuildVersionProvider, MyCustomBuildVersionProvider>();
    }
}

// Example: Custom Build ID Provider
public class MyCustomBuildIdProvider : IBuildIdProvider
{
    public string BuildId => Environment.GetEnvironmentVariable("MY_CUSTOM_BUILD_ID") ?? "local-dev-build";
    public string? GetBuildIdGroup(string buildId) => null;
}

// Example: Custom Build Version Provider
public class MyCustomBuildVersionProvider : IBuildVersionProvider
{
    public SemVer Version => SemVer.Parse(Environment.GetEnvironmentVariable("MY_CUSTOM_VERSION") ?? "0.1.0");
}
```

By leveraging these provider interfaces, Atom allows you to precisely control how your build's fundamental metadata is
generated and consumed, adapting to diverse project and CI/CD requirements.
