# Build Definitions

At the heart of every Atom project is the **Build Definition**. This is where you declare your build's structure, define its targets, configure parameters, and set up workflows. Atom uses a source-generated approach, meaning you define your build using C# interfaces and attributes, and Atom automatically generates the underlying logic.

### `IBuildDefinition`

The `IBuildDefinition` interface is the core contract that every Atom build must adhere to. It outlines the fundamental components of your build process:

* **Targets**: Individual units of work that can be executed.
* **Parameters**: External inputs that customize the build.
* **Workflows**: How your targets are orchestrated, often for CI/CD platforms.
* **Global Options**: Settings that apply across all workflows.

You won't typically implement `IBuildDefinition` directly. Instead, you'll define your build class and mark it with the `[BuildDefinition]` attribute. You must also explicitly inherit from either `BuildDefinition` (recommended) or `MinimalBuildDefinition`. The source generators will then provide the implementation for the abstract members of `IBuildDefinition` based on your defined targets and parameters.

### `MinimalBuildDefinition`

`MinimalBuildDefinition` provides a lean starting point for your build. It implements `IBuildDefinition` with default, often empty, implementations for its properties. This gives you maximum control to define everything from scratch.

**When to use it:**

* When you need absolute control over every aspect of your build.
* When you want to avoid any pre-configured targets or options.
* For very simple builds where you only need a few custom targets.

**How to use it:** Your main build class (often `Build.cs` in your `_atom` project, though any project name is supported) will be marked with the `[BuildDefinition]` attribute. You'll then implement the abstract properties and override virtual ones as needed.

```csharp
// In _atom/Build.cs
[BuildDefinition]
public partial class MyMinimalBuild : MinimalBuildDefinition, IMyCustomTargets
{
    // Implement abstract properties and override virtual ones
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("MyWorkflow")
        {
            Targets = [Targets.MyCustomTarget],
            WorkflowTypes = [Github.WorkflowType]
        }
    ];

    // ... other implementations
}

// In IMyCustomTargets.cs
[TargetDefinition]
public partial interface IMyCustomTargets
{
    Target MyCustomTarget => t => t.DescribedAs("A custom target.").Executes(() => Console.WriteLine("Hello from MyCustomTarget!"));
}
```

### `BuildDefinition`

`BuildDefinition` extends `MinimalBuildDefinition` by providing a more comprehensive set of pre-configured targets and options. It's the recommended starting point for most Atom projects as it includes common functionalities like:

* **Build Info Setup**: `ISetupBuildInfo` for managing build ID, version, and timestamp.
* **User Secrets**: `IDotnetUserSecrets` for integrating with .NET user secrets.

**When to use it:**

* For most typical Atom projects.
* When you want to leverage Atom's built-in targets and helpers for common tasks.
* To quickly get started with a robust build setup.

**How to use it:** Your main build class will be marked with `[BuildDefinition]`. You'll then add your custom targets and parameters by implementing additional interfaces.

```csharp
// In _atom/Build.cs
[BuildDefinition]
public partial class MyBuild : BuildDefinition, IMyCustomTargets
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("MyCIWorkflow")
        {
            Triggers = [GitPushTrigger.ToMain],
            Targets =
            [
                Targets.SetupBuildInfo, // Provided by BuildDefinition
                Targets.MyCustomTarget
            ],
            WorkflowTypes = [Github.WorkflowType]
        }
    ];
}

// In IMyCustomTargets.cs
[TargetDefinition]
public partial interface IMyCustomTargets
{
    Target MyCustomTarget => t => t.DescribedAs("My custom build target.").Executes(() => Console.WriteLine($"Building version {BuildVersion}"));
}
```

By choosing the appropriate base class, you can tailor your Atom build definition to your project's specific needs, balancing between granular control and leveraging Atom's powerful conventions.
