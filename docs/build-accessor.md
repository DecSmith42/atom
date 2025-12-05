# Build Accessor (`IBuildAccessor`)

The `IBuildAccessor` interface is a foundational component in Atom, providing a convenient way for your build targets
and helper classes to interact with core services and retrieve parameters. When your build definition interfaces
implement `IBuildAccessor`, you gain access to essential functionalities without needing to manually inject them
everywhere.

## `IBuildAccessor` Interface

This interface acts as a central hub for common build-related services and parameter retrieval. It's designed to
simplify the development of your build logic by making frequently used services readily available.

### Key Properties and Methods:

* **`Services`**:
  Provides access to the underlying `IServiceProvider`. This allows you to resolve any service registered in the
  dependency injection container.

  **When to use it:**
  When you need to resolve a service that isn't directly exposed by `IBuildAccessor` or when you need to resolve
  multiple instances of a service.

* **`Logger`**:
  A pre-configured `ILogger` instance specific to the type implementing `IBuildAccessor`. This allows for structured and
  contextual logging within your targets and helpers.

  **When to use it:**
  For all logging needs within your build logic.

  **How to use it:**
  ```csharp
  // Inside a target definition interface
  Target MyTarget => t => t.Executes(() =>
  {
      Logger.LogInformation("Starting MyTarget...");
  });
  ```

* **`FileSystem`**:
  Provides access to `IAtomFileSystem`, Atom's abstraction over file system operations. This includes access to
  important build-specific directories like the root, artifacts, publish, and temporary directories.

  **When to use it:**
  For any file or directory operations, especially when dealing with build outputs or inputs.

  **How to use it:**
  ```csharp
  // Inside a target definition interface
  Target CleanArtifacts => t => t.Executes(() =>
  {
      if (FileSystem.AtomArtifactsDirectory.DirectoryExists)
      {
          FileSystem.Directory.Delete(FileSystem.AtomArtifactsDirectory, true);
          Logger.LogInformation("Cleaned artifacts directory.");
      }
  });
  ```

* **`ProcessRunner`**:
  Provides access to `IProcessRunner`, Atom's service for executing external processes. This offers standardized
  logging, error handling, and result capture for command-line tools.

  **When to use it:**
  When you need to run external commands like `dotnet build`, `git clone`, `npm install`, etc.

  **How to use it:**
  ```csharp
  // Inside a target definition interface
  Target RunDotnetBuild => t => t.Executes(async () =>
  {
      await ProcessRunner.RunAsync(new ProcessRunOptions("dotnet", "build"));
  });
  ```

* **`GetService<T>()`**:
  A shortcut method to retrieve a single required service from the `IServiceProvider`. It includes special handling for
  `IBuildDefinition` types to return the current instance.

  **When to use it:**
  When you need a specific service instance that is registered in DI.

* **`GetServices<T>()`**:
  Retrieves all registered services of a specified type from the `IServiceProvider`.

  **When to use it:**
  When you need to work with multiple implementations of an interface (e.g., multiple `ISecretsProvider` instances).

* **
  `GetParam<T>(Expression<Func<T?>> parameterExpression, T? defaultValue = default, Func<string?, T?>? converter = null)`
  **:
  The primary method for retrieving parameter values. It uses an expression to identify the parameter, allowing for
  strongly-typed access and compile-time safety.

  **When to use it:**
  To access any parameter you've defined using `[ParamDefinition]` or `[SecretDefinition]`.

  **How to use it:**
  ```csharp
  // Inside a target definition interface
  [ParamDefinition("build-config", "The build configuration")]
  string BuildConfig => GetParam(() => BuildConfig, "Release");

  Target MyTarget => t => t.Executes(() =>
  {
      Logger.LogInformation($"Current build configuration: {BuildConfig}");
  });
  ```

### How to use `IBuildAccessor`

You don't directly implement `IBuildAccessor` in your build definition class. Instead, your build definition class (
inheriting from `MinimalBuildDefinition` or `BuildDefinition`) will implement interfaces that themselves implement
`IBuildAccessor`. This pattern allows Atom's source generator to provide the concrete implementations for these
properties and methods.

```csharp
// In IMyTargets.cs
[TargetDefinition]
public partial interface IMyTargets : IBuildAccessor // Implement IBuildAccessor here
{
    Target MyTarget => t => t.Executes(() =>
    {
        Logger.LogInformation("Accessing services via IBuildAccessor.");
        // ... use FileSystem, ProcessRunner, GetParam, etc.
    });
}

// In your build definition project (e.g., _atom/Build.cs)
[BuildDefinition]
public partial class MyBuild : BuildDefinition, IMyTargets // MyBuild now implicitly has access to IBuildAccessor members
{
    // ...
}
```

By leveraging `IBuildAccessor`, you ensure that your build logic is clean, testable, and has consistent access to the
necessary services and parameters.
