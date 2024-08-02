# Atom

Atom is an opinionated task and build automation framework, written in C#.

Heavily inspired by [NUKE](https://nuke.build/), Atom aims to provide a flexible, extensible framework for defining and executing build
tasks.

It leverages .NET and provides a comprehensive set of features for automating your development workflow.

## Project Structure

- `DecSm.Atom`: Core library
- `DecSm.Atom.Tool`: Command-line interface tool
- `DecSm.Atom.SourceGenerators`: Source generators for code generation
- `DecSm.Atom.Extensions.*`: Various extensions
- `DecSm.Atom.Tests` and `DecSm.Atom.SourceGenerators.Tests`: Test projects

## Getting Started

1. Clone the repository
2. Build the solution:
   ```
   dotnet build
   ```
3. Run the Atom tool:
   ```
   dotnet run --project _atom/_atom.csproj
   ```

## Usage

> TODO

Run specific tasks using the Atom CLI:

```
atom [command] [options]
```

For a full list of commands and options, run:

```
atom --help
```

## Extending Atom

Atom is designed to be extensible. Create new extensions by implementing the appropriate interfaces and registering them with the dependency
injection container.

## Contributing

Contributions are welcome! Please read our contributing guidelines and submit pull requests to our GitHub repository.

## License

Atom is released under the MIT License. See the [LICENSE](LICENSE.txt) file for details.

## Build Status

[![Build](https://github.com/YourUsername/Atom/actions/workflows/Build.yml/badge.svg)](https://github.com/YourUsername/Atom/actions/workflows/Build.yml)

## Contact

For questions or support, please open an issue on the GitHub repository.

---

Remember to replace "YourUsername" with the actual GitHub username or organization name where the Atom repository is hosted. You may also
want to add more specific installation instructions, configuration details, and examples of how to use Atom in real-world scenarios.