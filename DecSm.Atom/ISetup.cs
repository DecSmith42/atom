namespace DecSm.Atom;

[TargetDefinition]
public interface ISetup : IBuildDefinition
{
    [ParamDefinition("atom-build-id", "Unique build ID")]
    string AtomBuildId => GetParam(() => AtomBuildId)!;

    IBuildIdProvider BuildIdProvider => Services.GetRequiredService<IBuildIdProvider>();

    Target Setup =>
        d => d
            .ProducesVariable(nameof(AtomBuildId))
            .Executes(() =>
            {
                var buildId = BuildIdProvider.BuildId;

                WriteVariable(nameof(AtomBuildId), buildId.ToString());


                Services
                    .GetRequiredService<ILogger<ISetup>>()
                    .LogInformation("Build ID: {BuildId}, MSBuildSDKsPath: {SdksPath}",
                        buildId,
                        Environment.GetEnvironmentVariable("MSBuildSDKsPath"));

                return Task.CompletedTask;
            });
}