# Atom Guides

This section provides detailed guides on various features and concepts within the DecSm.Atom framework. Use these guides
to understand what each component is, what it does, and when and how to use it effectively in your build automation.

## Core Concepts

* [**Getting Started**](./getting-started.md): Your first steps with DecSm.Atom, from project setup to running your
  first build.
* [**Build Definitions**](./build-definitions.md): Understand the core structure of your build, including
  `IBuildDefinition`, `MinimalBuildDefinition`, and `BuildDefinition`.
* [**Targets**](./targets.md): Learn how to define and execute individual units of work using `TargetDefinition` and
  `[TargetDefinition]`.
* [**Parameters**](./parameters.md): Configure your build with external inputs, including `ParamDefinition`,
  `[ParamDefinition]`, `[SecretDefinition]`, and chained parameters.
* [**Workflows**](./workflows.md): Orchestrate your targets for CI/CD platforms using `WorkflowDefinition`,
  `WorkflowTargetDefinition`, and `WorkflowGenerator`.

## Build Infrastructure

* [**Build Accessor (`IBuildAccessor`)**](./build-accessor.md): Access core services and helpers like `Logger`,
  `FileSystem`, and `ProcessRunner` within your targets.
* [**Atom File System (`IAtomFileSystem`)**](./atom-file-system.md): Perform file and directory operations safely with
  Atom's abstraction, including `RootedPath`.
* [**Process Runner (`IProcessRunner`)**](./process-runner.md): Execute external commands with robust logging, error
  handling, and result capture.
* [**Secrets Management**](./secrets.md): Handle sensitive data securely using `ISecretsProvider`, `IDotnetUserSecrets`,
  and `[SecretDefinition]`.
* [**Workflow Variables**](./workflow-variables.md): Share data between targets and jobs using
  `IWorkflowVariableService` and `IVariablesHelper`.
* [**Build Information Providers**](./build-information-providers.md): Customize how build ID, version, and timestamp
  are generated or retrieved.
* [**Rich Console Output**](./console-output.md): Enhance your build's console experience with Spectre.Console
  integration and `LogOptions`.
* [**File Transformation Scopes**](./file-transformation-scopes.md): Temporarily modify files and automatically revert
  changes using `TransformFileScope` and `TransformMultiFileScope`.

## Workflow Configuration

* [**Workflow Options**](./workflow-options.md): Customize workflow behavior with `IWorkflowOption` and its
  implementations like `ToggleWorkflowOption`, `CustomStep`, and various injection options.
* [**Workflow Triggers**](./workflow-triggers.md): Define when your workflows run using `IWorkflowTrigger` and its
  implementations like `GitPushTrigger`, `GitPullRequestTrigger`, `ManualTrigger`, and `GithubScheduleTrigger`.

## Utilities

* [**Semantic Versioning (`SemVer`)**](./semantic-versioning.md): Work with version numbers in a consistent and standard
  way.
* [**Step Failed Exception**](./step-failed-exception.md): Understand how to signal and handle build failures
  gracefully.

## Common Targets

* [**Common Targets**](./common-targets.md): Leverage pre-built targets for common build, test, and deployment
  scenarios.
