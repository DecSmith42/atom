// Build definition that prints a hello message based on the provided parameter.

// In the console, run the following command to execute the build:
// dotnet run -- Hello

// This will present the following error message:
// Missing required parameter 'my-name' for target Hello

// Run the following command to execute the build with a parameter:
// dotnet run -- Hello --my-name World

// Run one of the following commands to have atom interactively prompt for the required params
// dotnet run -- Hello --interactive
// or
// dotnet run -- Hello -i

using DecSm.Atom.Build.Definition;
using DecSm.Atom.Params;

namespace Atom;

[DefaultBuildDefinition]
internal partial class Build : DefaultBuildDefinition
{
    // This property defines a parameter that can be set when executing the build.
    [ParamDefinition("my-name", "Name to greet")]
    private string? MyName => GetParam(() => MyName);

    // This property matches the one listed in the appsettings.json file, so it will be populated
    [ParamDefinition("config-item-1", "Configuration item 1")]
    private string? ConfigItem1 => GetParam(() => ConfigItem1);

    // This property has a default value, which will be used if the parameter is not provided.
    [ParamDefinition("config-item-2", "Configuration item 2")]
    private string ConfigItem2 => GetParam(() => ConfigItem2, "Default Value");

    // The build will fail immediately if any of the required parameters are not provided.
    private Target Hello =>
        t => t
            .DescribedAs("Prints some messages based on provided parameters")
            .RequiresParam(nameof(MyName))
            .RequiresParam(nameof(ConfigItem1))
            .RequiresParam(nameof(ConfigItem2))
            .Executes(() =>
            {
                // Standard Logger is automatically injected into TargetDefinition
                Logger.LogInformation("Hello, {Name}!", MyName);
                Logger.LogInformation("Config Item 1: {ConfigItem1}", ConfigItem1);
                Logger.LogInformation("Config Item 2: {ConfigItem2}", ConfigItem2);

                return Task.CompletedTask;
            });
}
