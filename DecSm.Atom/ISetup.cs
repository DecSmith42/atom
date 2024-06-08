namespace DecSm.Atom;

[TargetDefinition]
public interface ISetup : IBuildDefinition
{
    [ParamDefinition("atom-build-id", "Unique build ID")]
    string AtomBuildId => GetParam(() => AtomBuildId)!;
    
    Target Setup =>
        d => d
            .ProducesVariable(nameof(AtomBuildId))
            .Executes(() =>
            {
                var buildId = $"{DateTime.UtcNow:yyyyMMddHHmmss}";
                
                WriteVariable(nameof(AtomBuildId), buildId);
                
                Services
                    .GetRequiredService<ILogger<ISetup>>()
                    .LogInformation("Build ID: {BuildId}", buildId);
                
                return Task.CompletedTask;
            });
}