# Workflow Triggers

Workflow triggers (`IWorkflowTrigger`) define the events or conditions that initiate your Atom workflows. They allow you to specify exactly when your CI/CD pipelines should run, whether it's on code pushes, pull requests, a schedule, or manual intervention.

### `IWorkflowTrigger` Interface

This is a marker interface for all workflow trigger types. You won't implement it directly, but rather use its concrete implementations.

### Common Workflow Trigger Implementations

Atom provides several built-in implementations of `IWorkflowTrigger` for common scenarios.

#### `GitPushTrigger`

Triggers a workflow when code is pushed to a Git repository.

**What it is:** A trigger that responds to `git push` events. It allows for fine-grained control over which pushes activate the workflow based on branches, file paths, and tags.

**When to use it:**

* For Continuous Integration (CI) workflows that run on every code change.
* To build and test code when new features are pushed.
* To trigger deployments when code is pushed to a release branch.

**Key Properties:**

* **`IncludedBranches` / `ExcludedBranches`**: Patterns for branches to include/exclude.
* **`IncludedPaths` / `ExcludedPaths`**: Patterns for file paths to include/exclude.
* **`IncludedTags` / `ExcludedTags`**: Patterns for tags to include/exclude.
* **`ToMain`**: A static property for a common trigger that activates only on pushes to the `main` branch.

**How to use it:**

```csharp
// In your WorkflowDefinition
new("CI")
{
    Triggers =
    [
        GitPushTrigger.ToMain, // Trigger on pushes to 'main' branch
        new GitPushTrigger // Trigger on pushes to 'feature/*' branches, excluding docs
        {
            IncludedBranches = ["feature/*"],
            ExcludedPaths = ["docs/**", "*.md"]
        }
    ],
    // ...
}
```

#### `GitPullRequestTrigger`

Triggers a workflow when a pull request (PR) event occurs in a Git repository.

**What it is:** A trigger that responds to PR-related events (e.g., PR opened, synchronized, reopened). It allows filtering based on target branches, file paths, and PR event types.

**When to use it:**

* For CI workflows that validate code changes before they are merged.
* To run tests, linting, or static analysis on proposed changes.

**Key Properties:**

* **`IncludedBranches` / `ExcludedBranches`**: Patterns for target branches of the PR.
* **`IncludedPaths` / `ExcludedPaths`**: Patterns for file paths affected by the PR.
* **`Types`**: Specific PR event types (e.g., "opened", "synchronize").
* **`IntoMain`**: A static property for a common trigger that activates on PRs targeting the `main` branch.

**How to use it:**

```csharp
// In your WorkflowDefinition
new("PR_Validation")
{
    Triggers =
    [
        GitPullRequestTrigger.IntoMain, // Trigger on PRs into 'main' branch
        new GitPullRequestTrigger // Trigger on PRs that modify 'src/api' files
        {
            IncludedPaths = ["src/api/**"],
            Types = ["opened", "synchronize"] // Only on PR open or update
        }
    ],
    // ...
}
```

#### `ManualTrigger`

Allows a workflow to be triggered manually, optionally with user-defined inputs.

**What it is:** A trigger that enables users to manually start a workflow, typically from the CI/CD platform's UI. It can be configured to prompt the user for specific input values.

**When to use it:**

* For deployment workflows that require manual approval or specific parameters.
* For maintenance tasks or ad-hoc builds.
* To provide a user-friendly way to run specific parts of your build.

**Key Properties:**

* **`Inputs`**: A list of `ManualInput` objects (`ManualStringInput`, `ManualBoolInput`, `ManualChoiceInput`) that define the parameters the user will be prompted for.
* **`Empty`**: A static property for a simple manual trigger with no inputs.

**How to use it:**

```csharp
// In your WorkflowDefinition
new("Manual_Deployment")
{
    Triggers =
    [
        new ManualTrigger
        {
            Inputs =
            [
                new ManualStringInput("Environment", "The target environment (e.g., 'Staging', 'Production')", true, "Staging"),
                new ManualBoolInput("Approve", "Approve deployment to production?", true, false),
                new ManualChoiceInput("Region", "Deployment region", true, ["EastUS", "WestUS"], "EastUS")
            ]
        }
    ],
    // ...
}
```

#### `GithubScheduleTrigger`

Triggers a workflow based on a defined CRON schedule.

**What it is:** A trigger that automatically runs a workflow at specified times or intervals, using a CRON expression.

**When to use it:**

* For nightly builds, daily reports, or weekly cleanup tasks.
* For any recurring automated process.

**Key Properties:**

* **`CronExpression`**: A string representing the CRON schedule (e.g., `"0 0 * * *"` for daily at midnight UTC).

**How to use it:**

```csharp
// In your WorkflowDefinition
new("Nightly_Build")
{
    Triggers =
    [
        new GithubScheduleTrigger("0 0 * * *") // Run daily at midnight UTC
    ],
    // ...
}
```

By combining these workflow triggers, you can precisely control the automation of your build and deployment pipelines, ensuring they run exactly when and how you need them to.
