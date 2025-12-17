# Semantic Versioning

Atom provides a first-class `SemVer` type to represent and manipulate semantic versions. Semantic Versioning (SemVer) is a widely adopted standard for versioning software, using a `MAJOR.MINOR.PATCH` format with optional pre-release and build metadata. Atom's `SemVer` type ensures that your build's version information is always consistent and adheres to this standard.

### `SemVer` Record

The `SemVer` record is an immutable representation of a semantic version. It provides methods for parsing, comparing, and manipulating versions according to the SemVer 2.0.0 specification.

**What it is:** A C# record that encapsulates the major, minor, patch, pre-release, and build metadata components of a semantic version.

**When to use it:**

* Whenever you need to work with version numbers in your build scripts.
* To ensure consistency and correctness when incrementing versions, comparing versions, or displaying version information.
* It's automatically used by `IBuildVersionProvider` to supply the `BuildVersion` parameter.

**Key Properties:**

* **`Major`**: The major version number.
* **`Minor`**: The minor version number.
* **`Patch`**: The patch version number.
* **`Prerelease`**: The pre-release identifier (e.g., "alpha", "beta", "rc.1").
* **`Build`**: Build metadata (e.g., "20231225", "githash").
* **`One`**: A static property representing the version `1.0.0`.

**Key Methods:**

* **`Parse(string versionString)`**: Parses a string into a `SemVer` instance. Throws an exception if the string is not a valid semantic version.
* **`TryParse(string? versionString, out SemVer? semVer)`**: Attempts to parse a string into a `SemVer` instance, returning `true` on success and `false` on failure.
* **`IncrementMajor()` / `IncrementMinor()` / `IncrementPatch()`**: Returns a new `SemVer` instance with the respective version component incremented.
* **`WithPrerelease(string prerelease)` / `WithBuild(string build)`**: Returns a new `SemVer` instance with updated pre-release or build metadata.
* **Comparison Operators (`<`, `>`, `<=`, `>=`)**: Allows for easy comparison of `SemVer` instances.

**How to use it:** You typically access the `SemVer` type through the `BuildVersion` parameter (provided by `ISetupBuildInfo`).

```csharp
// In IMyTargets.cs
[TargetDefinition]
public partial interface IMyTargets : IBuildAccessor
{
    // Assuming BuildVersion is defined in IBuildInfo (which BuildDefinition implements)
    // SemVer BuildVersion => GetParam(() => BuildVersion);

    Target ShowVersionInfo => t => t
        .DescribedAs("Displays semantic version information.")
        .DependsOn(SetupBuildInfo) // Ensure BuildVersion is resolved
        .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildVersion))
        .Executes(() =>
        {
            Logger.LogInformation($"Current Version: {BuildVersion}");
            Logger.LogInformation($"Major: {BuildVersion.Major}");
            Logger.LogInformation($"Minor: {BuildVersion.Minor}");
            Logger.LogInformation($"Patch: {BuildVersion.Patch}");
            Logger.LogInformation($"Prerelease: {BuildVersion.Prerelease}");
            Logger.LogInformation($"Build Metadata: {BuildVersion.Build}");

            var nextMinorVersion = BuildVersion.IncrementMinor().WithPrerelease("alpha.1");
            Logger.LogInformation($"Next Minor Alpha: {nextMinorVersion}");

            if (BuildVersion > SemVer.Parse("1.0.0"))
            {
                Logger.LogInformation("Version is greater than 1.0.0.");
            }
        });
}
```

By using Atom's `SemVer` type, you can ensure that your versioning strategy is robust, consistent, and adheres to industry best practices.
