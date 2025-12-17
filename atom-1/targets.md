# Targets

In Atom, a **Target** represents a single, cohesive unit of work within your build process. Think of it as a function or a task that performs a specific action, like compiling code, running tests, or publishing artifacts. Targets are the building blocks of your build, and they can depend on other targets, consume parameters, and produce artifacts or variables.

### `TargetDefinition`

`TargetDefinition` is the core class that defines a target's behavior and metadata. You don't instantiate `TargetDefinition` directly; instead, you configure it using a fluent API within a `Target` delegate.

A `TargetDefinition` allows you to specify:

* **Name**: A unique identifier for the target.
* **Description**: A human-readable explanation of what the target does.
* **Visibility**: Whether the target should be hidden from general help output.
* **Tasks**: The actual C# code (actions or async functions) that the target executes.
* **Dependencies**: Other targets that must run before this target.
* **Parameters**: Inputs that the target uses.
* **Consumed/Produced Artifacts**: Files or directories that the target needs or creates.
* **Consumed/Produced Variables**: Data that the target needs or creates and shares with other targets.
* **Extensions**: How this target might extend or inherit behavior from another target.

**How to define a Target:** Targets can be defined as properties of type `Target` within your main build definition class (marked with `[BuildDefinition]`) or within a `partial interface` that is marked with `[TargetDefinition]`.

```csharp
// In IMyTargets.cs
[TargetDefinition]
public partial interface IMyTargets : IBuildAccessor // Inherit IBuildAccessor to get access to Logger, FileSystem, etc.
{
    Target Compile => t => t
        .DescribedAs("Compiles the application.")
        .Executes(() =>
        {
            Logger.LogInformation("Compiling project...");
            // ... actual compilation logic ...
        });

    Target RunTests => t => t
        .DescribedAs("Runs all unit tests.")
        .DependsOn(Compile) // This target depends on the Compile target
        .Executes(async () =>
        {
            Logger.LogInformation("Running tests...");
            await ProcessRunner.RunAsync(new ProcessRunOptions("dotnet", "test"));
        });
}
```

#### Key Methods for Configuring Targets:

* **`.DescribedAs(string description)`**: Adds a human-readable description to the target.
* **`.IsHidden(bool hidden = true)`**: Marks the target as hidden from default help output.
* **`.Executes(Func<CancellationToken, Task> task)` / `.Executes(Func<Task> task)` / `.Executes(Action action)`**: Defines the code to run when the target is executed. You can add multiple execution blocks, and they will run in the order they are defined.
*

    *

    \*`.DependsOn(string targetName)` / `.DependsOn(Target target)` / `.DependsOn(WorkflowTargetDefinition workflowTarget)` \*\*: Specifies that this target requires another target to complete successfully before it can start.
* **`.UsesParam(params IEnumerable<string> paramNames)`**: Declares that this target uses certain parameters, but they are not strictly required.
* **`.RequiresParam(params IEnumerable<string> paramNames)`**: Declares that this target absolutely needs certain parameters to be provided.
* **`.ProducesArtifact(string artifactName, string? buildSlice = null)`**: Declares an artifact that this target will create.
* **`.ConsumesArtifact(string targetName, string artifactName, string? buildSlice = null)`**: Declares an artifact from another target that this target needs.
* **`.ProducesVariable(string variableName)`**: Declares a variable that this target will set.
* **`.ConsumesVariable(string targetName, string outputName)`**: Declares a variable from another target that this target needs.
* **`.Extends<T>(Func<T, Target> targetToExtend, bool runExtensionAfter = false)`**: Allows a target to inherit tasks, dependencies, and other properties from another target.

### `TargetDefinitionAttribute`

The `[TargetDefinition]` attribute is crucial for Atom's source generator. You apply this attribute to `partial interface`s that contain your `Target` properties.

**When to use it:**

* Always, when defining a new set of targets in an interface.

**How it works:** The source generator scans interfaces marked with `[TargetDefinition]` within your build definition. It then automatically discovers all properties of type `Target` and registers them with the build system, making them available for execution and dependency resolution.

```csharp
// In IMyCustomTargets.cs
[TargetDefinition] // This attribute is essential!
public partial interface IMyCustomTargets
{
    Target Clean => t => t
        .DescribedAs("Cleans build outputs.")
        .Executes(() =>
        {
            Logger.LogInformation("Cleaning...");
            // ... cleanup logic ...
        });
}
```

By using `TargetDefinition` and `[TargetDefinition]`, you can modularize your build logic, create reusable target sets, and clearly define the inputs, outputs, and dependencies for each step of your build process.
