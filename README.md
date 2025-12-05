# Atom

[![Validate](https://github.com/DecSmith42/atom/actions/workflows/Validate.yml/badge.svg)](https://github.com/DecSmith42/atom/actions/workflows/Validate.yml)
[![Build](https://github.com/DecSmith42/atom/actions/workflows/Build.yml/badge.svg)](https://github.com/DecSmith42/atom/actions/workflows/Build.yml)
[![Dependabot Updates](https://github.com/DecSmith42/atom/actions/workflows/dependabot/dependabot-updates/badge.svg)](https://github.com/DecSmith42/atom/actions/workflows/dependabot/dependabot-updates)

Atom is an opinionated, type-safe build automation framework for .NET. It enables you to define your build logic in C#,
debug it like standard code, and automatically generate CI/CD configuration files for GitHub Actions and Azure DevOps.

## Why Atom?

* **Zero Context Switching**: Write build logic in C# alongside your application code.
* **Intellisense & Debugging**: Step through your build process using your IDE.
* **CI/CD Agnostic**: Define logic once; Atom generates the YAML for GitHub and Azure DevOps.
* **Modular**: Pull in capabilities via NuGet packages (GitVersion, Azure KeyVault, etc.).
* **Source Generators**: Reduces boilerplate by automatically discovering targets and parameters.

## Getting Started

To get started with DecSm.Atom, follow the [Getting Started Guide](./docs/getting-started.md).

## Documentation

For comprehensive guides and detailed information, including specific module documentation and a [Development Guide](./docs/development.md) for contributors, please refer to the [guides](./docs/guides.md).

## License

Atom is released under the [MIT License](https://www.google.com/search?q=LICENSE.txt).