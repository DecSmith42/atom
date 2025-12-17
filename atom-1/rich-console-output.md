# Rich Console Output

Atom leverages [Spectre.Console](https://spectreconsole.net/) to provide rich, formatted, and interactive console output. This enhances the user experience by making logs more readable, reports more structured, and interactions more intuitive.

### `IAnsiConsole` Service

The `IAnsiConsole` interface (from Spectre.Console) is Atom's primary mechanism for writing to the console. Atom automatically registers and configures an `IAnsiConsole` instance in its dependency injection container.

**What it is:** A powerful abstraction for writing formatted text, tables, trees, prompts, and more to the terminal, with support for ANSI escape codes for colors and styles.

**When to use it:**

* Whenever you need to output information to the console in your targets or helpers.
* To create visually appealing and easy-to-read logs and reports.
* For interactive prompts (though Atom's parameter system often handles this automatically).

**How to use it:** You can access the `IAnsiConsole` service by injecting it into your classes or by using `GetService<IAnsiConsole>()` via `IBuildAccessor`.

```csharp
// In IMyTargets.cs
[TargetDefinition]
public partial interface IMyTargets : IBuildAccessor
{
    Target ShowWelcomeMessage => t => t
        .DescribedAs("Displays a welcome message to the console.")
        .Executes(() =>
        {
            // Access IAnsiConsole via GetService
            var console = GetService<IAnsiConsole>();

            console.WriteLine();
            console.Write(new FigletText("Atom Build")
                .LeftJustified()
                .Color(Color.Blue));
            console.WriteLine();
            console.MarkupLine("[green]Welcome to your Atom-powered build![/]");
            console.WriteLine();

            // You can also use Logger, which uses Spectre.Console internally
            Logger.LogInformation("This message is logged via ILogger, but formatted by Spectre.Console.");
        });

    Target ShowBuildSummary => t => t
        .DescribedAs("Displays a summary table.")
        .Executes(() =>
        {
            var console = GetService<IAnsiConsole>();

            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("[bold blue]Metric[/]")
                .AddColumn("[bold green]Value[/]");

            table.AddRow("Build ID", "12345");
            table.AddRow("Version", "1.0.0");
            table.AddRow("Status", "[green]SUCCESS[/]");

            console.Write(table);
        });
}
```

### Logging Integration

Atom's logging system (based on `Microsoft.Extensions.Logging`) is integrated with Spectre.Console. This means that when you use `ILogger` (accessible via `IBuildAccessor.Logger`), your log messages will automatically be formatted and colored by Spectre.Console.

Additionally, Atom's `MaskingAnsiConsoleOutput` ensures that any sensitive information (secrets) is automatically masked with `*****` before being written to the console, even when using raw `IAnsiConsole` writes.

### `LogOptions`

The `LogOptions` static class provides a simple way to control logging verbosity.

* **`LogOptions.IsVerboseEnabled`**: A static boolean property that, when `true`, enables more detailed diagnostic information in logs (e.g., `LogLevel.Debug` and `LogLevel.Trace` messages will be shown). This is typically controlled by the `--verbose` command-line flag.

**When to use it:** To control the level of detail in your console output, especially for debugging.

**How to use it:** You usually don't set this directly in your build script; it's controlled by the `--verbose` command-line argument. However, you can check its state if your custom logging logic needs to adapt to verbosity.

```csharp
// Example of checking verbosity
Target DebugInfo => t => t.Executes(() =>
{
    if (LogOptions.IsVerboseEnabled)
    {
        Logger.LogDebug("Verbose logging is enabled. Showing extra debug info.");
    }
});
```

By leveraging Spectre.Console through `IAnsiConsole` and `ILogger`, Atom provides a powerful and visually appealing way to communicate build progress and results.
