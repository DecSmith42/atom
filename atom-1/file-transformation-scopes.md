# File Transformation Scopes

Atom provides specialized disposable scopes for temporarily modifying file contents. These scopes are incredibly useful for scenarios where you need to alter a file for a specific operation (e.g., injecting build properties into a project file) but want to ensure the original content is restored afterward. This pattern is often referred to as Resource Acquisition Is Initialization (RAII).

### `TransformFileScope`

The `TransformFileScope` manages the temporary transformation of a single file.

**What it is:** A disposable class that:

1. Reads the original content of a specified file.
2. Applies a transformation function to that content and writes the modified content back to the file.
3. Upon disposal (e.g., at the end of a `using` block), it restores the file to its original content.
4. Provides a mechanism to `CancelRestore()` if you want the changes to be permanent.

**When to use it:**

* To temporarily inject build properties into a `.csproj` or `.props` file.
* To modify configuration files for a specific test run.
* Any scenario where you need to alter a file's content for a short duration and then revert it.

**How to use it:** You create an instance using `TransformFileScope.CreateAsync` (for async operations) or `TransformFileScope.Create` (for sync operations), passing the `RootedPath` to the file and a transformation function.

```csharp
// In IMyTargets.cs
[TargetDefinition]
public partial interface IMyTargets : IBuildAccessor
{
    Target InjectBuildProperties => t => t
        .DescribedAs("Temporarily injects build properties into a project file.")
        .Executes(async () =>
        {
            var projectFile = FileSystem.AtomRootDirectory / "MyProject.csproj";

            // Create a scope to add a property
            await using (var scope = await TransformFileScope.CreateAsync(projectFile, content =>
            {
                // Example: Inject a <Version> property
                return content.Replace("</Project>", $"  <PropertyGroup><Version>1.2.3</Version></PropertyGroup>\n</Project>");
            }))
            {
                Logger.LogInformation("Build properties injected. Performing build...");
                await ProcessRunner.RunAsync(new ProcessRunOptions("dotnet", "build", projectFile));
                Logger.LogInformation("Build complete. Properties will be restored.");
            }
            // After 'using' block, MyProject.csproj is restored to its original content
        });
}
```

### `TransformMultiFileScope`

The `TransformMultiFileScope` is similar to `TransformFileScope` but allows you to manage temporary transformations for multiple files simultaneously.

**What it is:** A disposable class that:

1. Reads the original content of multiple specified files.
2. Applies a transformation function to each file's content and writes the modified content back.
3. Upon disposal, restores all managed files to their original content.
4. Provides a mechanism to `CancelRestore()` for all files.

**When to use it:**

* When you need to apply the same temporary transformation to a set of related files (e.g., multiple project files in a solution).
* To modify several configuration files for a specific environment.

**How to use it:** You create an instance using `TransformMultiFileScope.CreateAsync` or `TransformMultiFileScope.Create`, passing a collection of `RootedPath` instances and a transformation function.

```csharp
// In IMyTargets.cs
[TargetDefinition]
public partial interface IMyTargets : IBuildAccessor
{
    Target InjectPropertiesIntoMultipleProjects => t => t
        .DescribedAs("Temporarily injects properties into multiple project files.")
        .Executes(async () =>
        {
            var projectFiles = new[]
            {
                FileSystem.AtomRootDirectory / "ProjectA.csproj",
                FileSystem.AtomRootDirectory / "ProjectB.csproj"
            };

            // Create a scope to add a property to multiple files
            await using (var scope = await TransformMultiFileScope.CreateAsync(projectFiles, content =>
            {
                return content.Replace("</Project>", $"  <PropertyGroup><CustomFlag>true</CustomFlag></PropertyGroup>\n</Project>");
            }))
            {
                Logger.LogInformation("Properties injected into multiple projects. Performing operations...");
                // ... perform operations that rely on the injected properties ...
                Logger.LogInformation("Operations complete. Projects will be restored.");
            }
            // After 'using' block, ProjectA.csproj and ProjectB.csproj are restored
        });
}
```

### `TransformFileScopeExtensions`

These extension methods provide a fluent API for chaining multiple transformations within a single scope.

**What it is:** Extension methods (`AddAsync`) that allow you to apply additional transformations to an existing `TransformFileScope` or `TransformMultiFileScope` instance.

**When to use it:** When you need to apply several distinct modifications to a file (or files) within the same temporary scope.

**How to use it:** Chain calls to `AddAsync` after creating the initial scope.

```csharp
// In IMyTargets.cs
[TargetDefinition]
public partial interface IMyTargets : IBuildAccessor
{
    Target ChainedTransformations => t => t
        .DescribedAs("Applies multiple transformations to a file.")
        .Executes(async () =>
        {
            var configFile = FileSystem.AtomRootDirectory / "appsettings.json";

            await using (var scope = await TransformFileScope.CreateAsync(configFile, content =>
            {
                return content.Replace("\"Logging\": {", "\"Logging\": {\n    \"LogLevel\": {\n      \"Default\": \"Debug\"");
            })
            .AddAsync(content =>
            {
                return content.Replace("\"Default\": \"Debug\"", "\"Default\": \"Trace\"");
            }))
            {
                Logger.LogInformation("Applied multiple transformations. File will be restored.");
            }
        });
}
```

By using these file transformation scopes, you can manage temporary file changes in a clean, robust, and automatically reversible manner, which is crucial for maintaining a clean working directory and ensuring build reproducibility.
