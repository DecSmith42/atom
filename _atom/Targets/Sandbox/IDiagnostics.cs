namespace Atom.Targets.Sandbox;

[TargetDefinition]
internal partial interface IDiagnostics
{
    Target Diagnostics =>
        d => d.Executes(() =>
        {
            Logger.LogInformation("Running diagnostics...");

            var vars = Environment.GetEnvironmentVariables();
            var varsDisplay = (from DictionaryEntry entry in vars select $"{entry.Key}={entry.Value}").ToList();

            Logger.LogInformation("Environment Variables: {@Github}", string.Join(", ", varsDisplay));

            Logger.LogInformation("Diagnostics complete");

            return Task.CompletedTask;
        });
}