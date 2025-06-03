// Minimal build definition that prints a hello world message.
// In the console, run the following command to execute the build:
// dotnet run -- HelloWorld

using DecSm.Atom.Build.Definition;
using DecSm.Atom.Hosting;

namespace Atom;

[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : BuildDefinition
{
    private Target HelloWorld =>
        t => t
            .DescribedAs("Prints a hello world message")
            .Executes(() =>
            {
                // Standard Logger is automatically injected into the build
                Logger.LogInformation("Hello, World!");

                // All targets executions are tasks
                return Task.CompletedTask;
            });
}
