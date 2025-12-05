# Atom Host

The `AtomHost` class is the entry point for running any Atom-based application. It provides static methods to set up and
execute your build definition, handling all the underlying infrastructure like dependency injection, configuration
loading, and logging.

## `AtomHost` Class

The `AtomHost` class simplifies the process of getting your Atom build up and running. You typically interact with it
through its `Run` method, which takes your build definition as a type parameter.

### Key Methods:

* **`CreateAtomBuilder<T>(string[] args)`**:
  This method creates and configures a `HostApplicationBuilder` specifically tailored for Atom applications. It sets up
  essential services, loads `appsettings.json` (and environment-specific variants), and integrates your build
  definition.

  **When to use it:**
  If you need more fine-grained control over the host builder before the application runs. You might want to add custom
  services, configure logging further, or modify configuration sources.

  **How to use it:**
  ```csharp
  // In Program.cs
  public class Program
  {
      public static void Main(string[] args)
      {
          var builder = AtomHost.CreateAtomBuilder<MyBuildDefinition>(args);

          // Add custom services or configurations here
          builder.Services.AddSingleton<IMyCustomService, MyCustomService>();

          var host = builder.Build();
          host.UseAtom().Run();
      }
  }
  ```

* **`Run<T>(string[] args)`**:
  This is the most common way to start an Atom application. It's a convenience method that internally calls
  `CreateAtomBuilder`, builds the host, and then runs it.

  **When to use it:**
  For most standard Atom applications where you don't need to customize the host builder before running. This is
  typically what you'll see in the `Program.cs` of an Atom project.

  **How to use it:**
  ```csharp
  // In Program.cs
  public class Program
  {
      public static void Main(string[] args)
      {
          AtomHost.Run<MyBuildDefinition>(args);
      }
  }
  ```

### `[GenerateEntryPoint]` Attribute

Often, you won't even write the `Program.cs` file yourself. Atom provides a source generator that can create it for you.
By applying the `[GenerateEntryPoint]` attribute to your main build definition class, Atom will automatically generate a
`Program.cs` that calls `AtomHost.Run<YourBuildDefinition>(args)`.

**When to use it:**
To simplify your project structure and let Atom handle the boilerplate entry point code.

**How to use it:**

```csharp
// In your build definition project (e.g., _atom/Build.cs)
[BuildDefinition]
[GenerateEntryPoint] // This attribute tells Atom to generate Program.cs
public partial class MyBuild : BuildDefinition
{
    // ... your build definition ...
}
```

By understanding `AtomHost` and the `[GenerateEntryPoint]` attribute, you can effectively launch and manage your Atom
build applications.
