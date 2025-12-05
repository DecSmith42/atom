# Atom File System (`IAtomFileSystem`)

The `IAtomFileSystem` interface provides an abstraction layer over file system operations, specifically tailored for the
Atom build environment. It extends the standard `IFileSystem` interface (from `System.IO.Abstractions`) and adds
Atom-specific concepts like the project root, artifact directories, and a robust path resolution mechanism.

## `IAtomFileSystem` Interface

This interface centralizes access to important directories and file operations, ensuring consistency and testability
across your build logic.

### Key Properties:

* **`ProjectName`**:
  Gets the name of the project associated with the Atom file system, typically derived from the entry assembly.

* **`FileSystem`**:
  Gets the underlying `IFileSystem` instance, providing general file system functionality. You can use this for standard
  file and directory operations.

* **`AtomRootDirectory`**:
  Gets the root directory of the Atom project. This is dynamically determined by Atom, often by locating a directory
  containing project markers like `.git` or `.sln` files.

* **`AtomArtifactsDirectory`**:
  Gets the default directory where Atom stores build artifacts. This is where outputs from targets like `IStoreArtifact`
  are typically placed.

* **`AtomPublishDirectory`**:
  Gets the default directory where Atom publishes final build outputs. This is often the same as
  `AtomArtifactsDirectory` but can be configured differently.

* **`AtomTempDirectory`**:
  Gets a temporary working directory for Atom tasks and operations. This is useful for intermediate files that don't
  need to be persisted.

* **`CurrentDirectory`**:
  Gets the current working directory of the running process.

### Key Methods:

* **`GetPath(string key)`**:
  Resolves a path by its key, using registered path providers and falling back to default logic. This is how Atom
  determines the locations of `AtomRootDirectory`, `AtomArtifactsDirectory`, etc.

* **`GetPath<T>() where T : IFileMarker`**:
  Resolves the path for a given file marker type. This allows you to define specific files or directories using a marker
  interface and retrieve their paths.

* **`CreateRootedPath(string path)`**:
  Creates a new `RootedPath` instance from a string path, associating it with the current `IAtomFileSystem` instance.

## `RootedPath` Record

The `RootedPath` record is Atom's representation of a file system path. It's an immutable record that encapsulates a
path string along with a reference to the `IAtomFileSystem` it belongs to. This allows for fluent, type-safe path
manipulation and operations.

### Key Properties:

* **`Path`**: The absolute path string.
* **`Parent`**: Gets the parent directory of the current path.
* **`PathExists`**: Indicates whether the path exists as either a file or a directory.
* **`FileExists`**: Indicates whether the path exists as a file.
* **`DirectoryExists`**: Indicates whether the path exists as a directory.
* **`FileName`**: Gets the file name from the path.
* **`FileNameWithoutExtension`**: Gets the file name without its extension.
* **`DirectoryName`**: Gets the directory name from the path.

### Operators:

* **`/` operator**: Allows for fluent path concatenation.
  ```csharp
  RootedPath myPath = FileSystem.AtomRootDirectory / "src" / "MyProject";
  ```

* **Implicit conversion to `string`**: A `RootedPath` can be implicitly converted to a `string`.

## How to use `IAtomFileSystem`

You typically access `IAtomFileSystem` through the `FileSystem` property available via `IBuildAccessor` in your target
definitions.

```csharp
// In IMyTargets.cs
[TargetDefinition]
public partial interface IMyTargets : IBuildAccessor
{
    Target CleanBuildOutput => t => t.Executes(() =>
    {
        var buildOutputDirectory = FileSystem.AtomRootDirectory / "bin" / "Release";
        if (buildOutputDirectory.DirectoryExists)
        {
            FileSystem.Directory.Delete(buildOutputDirectory, true);
            Logger.LogInformation($"Cleaned: {buildOutputDirectory}");
        }
    });

    Target CopyArtifact => t => t.Executes(() =>
    {
        var sourceFile = FileSystem.AtomPublishDirectory / "my-artifact.zip";
        var destinationDir = FileSystem.CreateRootedPath("/tmp/artifacts");
        if (!destinationDir.DirectoryExists)
        {
            FileSystem.Directory.CreateDirectory(destinationDir);
        }
        FileSystem.File.Copy(sourceFile, destinationDir / sourceFile.FileName!);
        Logger.LogInformation($"Copied {sourceFile} to {destinationDir}");
    });
}
```

By using `IAtomFileSystem` and `RootedPath`, your build scripts become more robust, testable, and platform-agnostic when
dealing with file system interactions.
