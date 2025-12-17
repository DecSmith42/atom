# Workflow Variables

In complex build workflows, it's often necessary to share data between different targets or even different jobs within a CI/CD pipeline. Atom's Workflow Variable system provides a robust and extensible mechanism for persisting and retrieving these values.

### `IWorkflowVariableService` Interface

The `IWorkflowVariableService` is the central component for managing workflow variables. It acts as an abstraction layer over various `IWorkflowVariableProvider` implementations, allowing you to read and write variables without worrying about the underlying storage mechanism.

**When to use it:**

* When you need to pass a value from one target's execution to another.
* When you need to persist a value across different jobs in a CI/CD workflow.
* To store dynamic data generated during the build (e.g., a build ID, a generated path, a version number).

#### Key Methods:

* **`WriteVariable(string variableName, string variableValue, CancellationToken cancellationToken = default)`**: Persists a variable with the given name and value to the workflow context. The `variableName` should correspond to a parameter defined in your build definition.
* **`ReadVariable(string jobName, string variableName, CancellationToken cancellationToken = default)`**: Retrieves a variable from the workflow context. The `jobName` parameter allows you to specify which job the variable was written from, enabling cross-job data sharing.

### `IVariablesHelper` Interface

The `IVariablesHelper` interface provides a convenient helper method for writing workflow variables. When your build definition interfaces implement `IVariablesHelper`, you get direct access to the `WriteVariable` method.

**How to use it:** Implement `IVariablesHelper` in your target definition interface. Then, within your target's `Executes` block, you can call `WriteVariable`.

```csharp
// In IMyTargets.cs
[TargetDefinition]
public partial interface IMyTargets : IBuildAccessor, IVariablesHelper
{
    Target GenerateBuildId => t => t
        .DescribedAs("Generates and writes a unique build ID.")
        .Executes(async () =>
        {
            var buildId = Guid.NewGuid().ToString();
            await WriteVariable("BuildId", buildId); // "BuildId" should be a defined parameter
            Logger.LogInformation($"Generated Build ID: {buildId}");
        });

    Target UseBuildId => t => t
        .DescribedAs("Consumes the generated Build ID.")
        .DependsOn(GenerateBuildId)
        .ConsumesVariable(nameof(GenerateBuildId), "BuildId") // Declares dependency on the variable
        .Executes(() =>
        {
            // The BuildId parameter will now be resolved from the workflow variable
            Logger.LogInformation($"Using Build ID: {BuildId}");
        });

    // Assuming BuildId is defined as a parameter in IBuildInfo or another interface
    // [ParamDefinition("build-id", "Unique identifier for the build")]
    // string BuildId => GetParam(() => BuildId);
}
```

### `IWorkflowVariableProvider` Interface and `AtomWorkflowVariableProvider`

* **`IWorkflowVariableProvider`**: This interface defines the contract for concrete implementations that handle the actual storage and retrieval of workflow variables. Atom's `IWorkflowVariableService` delegates operations to a chain of these providers.
* **`AtomWorkflowVariableProvider`**: This is Atom's default `IWorkflowVariableProvider`. It stores and retrieves variables from a local JSON file within the Atom temporary directory. This is useful for local builds or simple CI/CD setups.

**When to use it:**

* **`IWorkflowVariableProvider`**: To integrate with external variable storage systems (e.g., CI/CD platform-specific mechanisms like GitHub Actions outputs, Azure DevOps variables).
* **`AtomWorkflowVariableProvider`**: You typically don't interact with this directly, as it's the default fallback.

**How it works:** When you call `WriteVariable` or `ReadVariable`, the `IWorkflowVariableService` iterates through all registered `IWorkflowVariableProvider`s. The first provider that successfully handles the operation (returns `true`) completes the request. This allows you to prioritize custom providers (e.g., a GitHub Actions provider) over the default `AtomWorkflowVariableProvider`.

By using workflow variables, you can create more dynamic and interconnected build processes, enabling seamless data flow between different stages of your automation.
