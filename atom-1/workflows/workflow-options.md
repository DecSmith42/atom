# Workflow Options

Workflow options (`IWorkflowOption`) provide a powerful mechanism to customize and control the behavior of your Atom workflows. They allow you to inject specific configurations, enable/disable features, or define custom steps that integrate seamlessly into your CI/CD pipelines.

### `IWorkflowOption` Interface

This is the base interface for all workflow options. It's a marker interface with a single property:

* **`AllowMultiple`**: A boolean indicating whether multiple instances of this option type can be present in a workflow. If `false` (default), only the last instance of the option type will be considered during merging.

Atom automatically merges workflow options from global settings, workflow definitions, and even target-specific configurations. This merging process respects the `AllowMultiple` property to handle duplicates.

### Common Workflow Option Implementations

Atom provides several built-in implementations of `IWorkflowOption` for common scenarios.

#### `ToggleWorkflowOption<TSelf>`

This is an abstract base class for creating boolean toggle options. It provides convenient `Enabled` and `Disabled` static properties.

**What it is:** A type-safe way to define workflow features that can be simply turned on or off.

**When to use it:**

* To enable or disable specific Atom features within a workflow (e.g., `UseCustomArtifactProvider`).
* To create your own boolean flags for custom workflow logic.

**How to use it:** Inherit from `ToggleWorkflowOption<TSelf>` to create your custom toggle. Atom provides several built-in toggles:

* **`UseCustomArtifactProvider`**: When `Enabled`, Atom will use its custom `IArtifactProvider` for artifact management instead of native CI/CD artifact steps.
* **`ProvideGithubRunIdAsWorkflowId`**: When `Enabled`, Atom will use the GitHub Actions run ID as the build ID.
* **`ProvideDevopsRunIdAsWorkflowId`**: When `Enabled`, Atom will use the Azure DevOps run ID as the build ID.
* **`UseGitVersionForBuildId`**: When `Enabled`, Atom will use GitVersion to determine the build version.

```csharp
// Example: Custom toggle option
public sealed record UseFeatureX : ToggleWorkflowOption<UseFeatureX>;

// Usage in a workflow definition
public override IReadOnlyList<WorkflowDefinition> Workflows =>
[
    new("MyWorkflow")
    {
        Options =
        [
            UseCustomArtifactProvider.Enabled, // Enable custom artifact handling
            UseFeatureX.Disabled               // Explicitly disable a custom feature
        ],
        // ...
    }
];

// Checking if an option is enabled
bool useCustomArtifacts = UseCustomArtifactProvider.IsEnabled(workflowOptions);
```

#### `CustomStep`

`CustomStep` is an abstract base class for defining custom workflow steps.

**What it is:** A way to inject custom, platform-agnostic steps into your workflow. These steps are then translated by the `IWorkflowWriter` into platform-specific actions or scripts.

**When to use it:**

* To define reusable steps that are not directly tied to Atom targets (e.g., setting up a database, running a custom script).
* To extend workflow functionality with your own custom logic.

**How to use it:** Inherit from `CustomStep` and add properties to configure your step.

```csharp
// Example: A custom step to run a specific script
public sealed record RunMyScriptStep(string ScriptPath) : CustomStep
{
    public string? Arguments { get; init; }
}

// Usage in a workflow definition
public override IReadOnlyList<WorkflowDefinition> Workflows =>
[
    new("MyWorkflow")
    {
        Options =
        [
            new RunMyScriptStep("scripts/setup.sh") { Arguments = "--verbose" }
        ],
        // ...
    }
];
```

#### `WorkflowParamInjection`

Injects a parameter value into the workflow execution context.

**What it is:** A mechanism to set a specific parameter's value directly within the workflow definition, overriding other parameter sources.

**When to use it:**

* To provide workflow-specific parameter values (e.g., `BuildConfiguration = "Release"` for a release workflow).
* To override default parameter values.

**How to use it:**

```csharp
// Example: Injecting a build configuration parameter
public override IReadOnlyList<WorkflowDefinition> Workflows =>
[
    new("ReleaseBuild")
    {
        Options =
        [
            new WorkflowParamInjection("BuildConfig", "Release")
        ],
        // ...
    }
];
```

#### `WorkflowSecretInjection`

Injects a secret value into the workflow execution context.

**What it is:** A secure way to pass secret values (e.g., API keys) into your workflow, ensuring they are handled securely by the CI/CD platform.

**When to use it:**

* To provide sensitive data to your workflow steps.

**How to use it:**

```csharp
// Example: Injecting a GitHub token
public override IReadOnlyList<WorkflowDefinition> Workflows =>
[
    new("Publish")
    {
        Options =
        [
            WorkflowSecretInjection.Create(nameof(IGithubHelper.GithubToken))
        ],
        // ...
    }
];
```

#### `WorkflowEnvironmentInjection`

Injects an environment variable into the workflow execution context.

**What it is:** A way to set custom environment variables that will be available to all steps within the workflow.

**When to use it:**

* To provide environment-specific settings or flags.

**How to use it:**

```csharp
// Example: Injecting a custom environment variable
public override IReadOnlyList<WorkflowDefinition> Workflows =>
[
    new("TestEnv")
    {
        Options =
        [
            WorkflowEnvironmentInjection.Create("MY_CUSTOM_VAR", "some-value")
        ],
        // ...
    }
];
```

#### `WorkflowSecretsEnvironmentInjection`

Injects a secret as an environment variable into the workflow execution context.

**What it is:** Similar to `WorkflowEnvironmentInjection`, but specifically for sensitive values that should be treated as secrets by the CI/CD platform.

**When to use it:**

* To provide sensitive environment variables (e.g., `AZURE_CLIENT_SECRET`).

**How to use it:**

```csharp
// Example: Injecting a secret as an environment variable
public override IReadOnlyList<WorkflowDefinition> Workflows =>
[
    new("Deploy")
    {
        Options =
        [
            WorkflowSecretsEnvironmentInjection.Create("AZURE_CLIENT_SECRET")
        ],
        // ...
    }
];
```

#### `WorkflowSecretsSecretInjection`

Injects a secret value that is specifically intended for `ISecretsProvider` implementations.

**What it is:** A specialized secret injection that prevents `ISecretsProvider` instances from attempting to look up the secret value themselves. This is useful when the secrets provider itself requires a secret (e.g., a vault access token) to function.

**When to use it:**

* When you need to provide a secret that is consumed by a secrets provider (e.g., a token for Azure Key Vault).

**How to use it:**

```csharp
// Example: Injecting a secret for a secrets provider
public override IReadOnlyList<WorkflowDefinition> Workflows =>
[
    new("ConfigureSecrets")
    {
        Options =
        [
            WorkflowSecretsSecretInjection.Create("AZURE_VAULT_TOKEN")
        ],
        // ...
    }
];
```

By using these workflow options, you can create highly configurable and secure CI/CD pipelines that adapt to various requirements and environments.
