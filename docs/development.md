# Development Guide for DecSm.Atom

This guide provides an overview for developers looking to work with or understand the internal
workings of the DecSm.Atom solution.

## Solution Structure

The `DecSm.Atom` solution is organized into several projects:

* **`DecSm.Atom`**: The core library containing the fundamental abstractions and services.
* **`DecSm.Atom.Module.*`**: A collection of NuGet packages providing integrations with external tools or platforms (
  e.g., Azure Key Vault, GitVersion, GitHub Actions).
* **`DecSm.Atom.Tool`**: The global .NET tool that orchestrates the execution of Atom build definitions.
* **`DecSm.Atom.DotnetCliGenerator`**: A tool used to generate type-safe wrappers for the .NET CLI.
* **`DecSm.Atom.Analyzers`**: Roslyn analyzers to enforce best practices and detect common issues in Atom build
  definitions.
* **`DecSm.Atom.Tests` / `DecSm.Atom.Module.*.Tests` / `DecSm.Atom.Analyzers.Tests`**: Unit and integration test
  projects for the respective components.
* **`Sample_01_HelloWorld` / `Sample_02_Params`**: Sample projects demonstrating basic Atom features.

## Working with the Core Library (`DecSm.Atom`)

The `DecSm.Atom` project is the heart of the framework. It defines interfaces, base classes, and core services that all
other components rely on.

* **Key Areas**:
    * `Build.Definition`: Core abstractions for build definitions, targets, and workflows.
    * `Params`: Parameter handling, including `ParamDefinition` and `SecretDefinition`.
    * `Files`: Abstractions for file system operations (`IAtomFileSystem`, `RootedPath`).
    * `Processes`: Abstractions for running external processes (`IProcessRunner`).
    * `Workflows`: Core workflow engine and YAML generation logic.
    * `Build.Info`: Providers for build-related information (ID, version, timestamp).
* **Development Considerations**:
    * Changes here often have a ripple effect across the entire solution.
    * Ensure all new features or changes are covered by unit tests.
    * Maintain high code quality and adhere to established patterns.

## Developing Modules (`DecSm.Atom.Module.*`)

Modules extend Atom's capabilities by integrating with specific technologies or platforms. Each module is typically a
separate NuGet package.

* **Structure**:
    * A module project (`DecSm.Atom.Module.ModuleName`) contains the integration logic.
    * It often defines interfaces (e.g., `IAzureKeyVault`, `IGitVersion`) that build definitions can implement to enable
      the module's features.
    * It registers services via `ConfigureHostBuilder` attributes or methods.
    * May include helper methods (e.g., `IDotnetPackHelper`).
* **Development Considerations**:
    * Modules should be self-contained and have minimal dependencies on other modules.
    * Follow the pattern of providing interfaces for integration and registering services.
    * Ensure comprehensive XML documentation for all public APIs.
    * Provide clear examples and usage instructions in the module's documentation (`docs/modules/module-name.md`).

## The Atom Tool (`DecSm.Atom.Tool`)

The `DecSm.Atom.Tool` is a global .NET tool that acts as the entry point for executing Atom build definitions.

* **Functionality**:
    * Locates and executes Atom build projects (`dotnet run --project <path>`).
    * Provides utility commands (e.g., `nuget-add`).
    * Handles argument parsing and redirection to the underlying build project.
* **Development Considerations**:
    * Uses `ConsoleAppFramework` for command-line parsing.
    * The `RunArgsFilter` is crucial for correctly passing arguments to the Atom build.
    * Ensure robust error handling and user-friendly messages.
    * Security: Argument sanitization is important when invoking external processes.

## Dotnet CLI Generator Tool (`DecSm.Atom.DotnetCliGenerator`)

This project is a specialized tool within the Atom ecosystem responsible for generating the type-safe C# wrappers for
the .NET CLI.

* **Functionality**:
    * Executes `dotnet --cli-schema` to get the JSON schema of all `dotnet` commands.
    * Parses this schema to understand command structures, arguments, and options.
    * Generates C# interfaces (`IDotnetCli`), implementation classes (`DotnetCli`), and options records for each
      `dotnet` command.
    * These generated files reside in `DecSm.Atom.Module.Dotnet/Cli/Generated`.
* **Development Considerations**:
    * This tool is typically run as part of the Atom solution's own build process (e.g., in `_atom/Build.cs`).
    * Changes to the generator impact the `DecSm.Atom.Module.Dotnet` API.
    * Ensure the parsing logic correctly interprets the `dotnet --cli-schema` output.
    * The `CsharpWriter` class is central to the code generation process.

## Analyzers (`DecSm.Atom.Analyzers`)

The `DecSm.Atom.Analyzers` project contains Roslyn analyzers that provide compile-time feedback for Atom build
definitions.

* **Functionality**:
    * Detects common mistakes or anti-patterns in Atom build code.
    * Enforces best practices (e.g., correct parameter usage).
    * Provides diagnostics and potential code fixes.
* **Development Considerations**:
    * Analyzers are powerful but can be complex to write.
    * Requires understanding of Roslyn APIs (Syntax Trees, Semantic Models).
    * Thorough testing with both valid and invalid code examples is essential (`DecSm.Atom.Analyzers.Tests`).
    * The `DecSm.Atom.Analyzers.Sample` project is used to test analyzer behavior.

## Testing

Each project in the Atom solution has a corresponding test project. Within `DecSm.Atom.Tests` (and module-specific test projects), tests are generally categorized:

*   **Class Tests (Unit Tests)**: These are traditional unit tests that verify the correctness of individual classes, methods, or small components in isolation. They focus on the internal logic of the framework's core library and modules, often using mocking to isolate dependencies.
*   **Build Tests**: These tests validate the end-to-end execution and integration of a complete `DecSm.Atom` build definition. They simulate or actually run an Atom build process, verifying that build definitions, targets, parameters, and workflows interact correctly within the Atom runtime. This often includes checking generated workflow files or asserting on expected side effects.

* **Unit Tests**: Focus on individual components and methods in isolation.
* **Integration Tests**: Verify the interaction between multiple components or with external systems (e.g., actual
  `dotnet` commands, file system operations).
* **Analyzer Tests**: Use the Roslyn `Microsoft.CodeAnalysis.Testing` framework to test analyzer diagnostics and code
  fixes.

## Contributing

Contributions are welcome but may not be accepted if they do not align with the project's goals or if they introduce
significant technical debt. If an issue is found or a feature is missing, please
open an issue in the [GitHub repository](https://github.com/DecSmith42/atom/issues) and submit a pull request with your
changes which will be reviewed.