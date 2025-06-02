namespace DecSm.Atom;

[TargetDefinition]
public partial interface ISetupBuildInfo : IBuildInfo, IVariablesHelper, IReportsHelper
{
    IBuildIdProvider BuildIdProvider => GetService<IBuildIdProvider>();

    IBuildVersionProvider BuildVersionProvider => GetService<IBuildVersionProvider>();

    IBuildTimestampProvider BuildTimestampProvider => GetService<IBuildTimestampProvider>();

    Target SetupBuildInfo =>
        d => d
            .WithDescription("Sets up the build ID, version, and timestamp")
            .IsHidden()
            .ProducesVariable(nameof(AtomBuildName))
            .ProducesVariable(nameof(BuildId))
            .ProducesVariable(nameof(BuildVersion))
            .ProducesVariable(nameof(BuildTimestamp))
            .Executes(async () =>
            {
                var atomBuildName = AtomBuildName;
                await WriteVariable(nameof(AtomBuildName), atomBuildName);

                var buildId = BuildIdProvider.BuildId;
                await WriteVariable(nameof(BuildId), buildId);

                var buildVersion = BuildVersionProvider.Version;
                await WriteVariable(nameof(BuildVersion), buildVersion);

                var buildTimestamp = BuildTimestampProvider.Timestamp;
                await WriteVariable(nameof(BuildTimestamp), buildTimestamp.ToString());

                var reportedBuildId = buildId == buildVersion
                    ? buildId
                    : $"{buildVersion} - {buildId} [{buildTimestamp}]";

                AddReportData(new TextReportData($"{AtomBuildName} | {reportedBuildId}")
                {
                    Title = "Run Information",
                    BeforeStandardData = true,
                });

                var logger = Services.GetRequiredService<ILogger<ISetupBuildInfo>>();

                logger.LogInformation("Build ID: {BuildId}", buildId);
                logger.LogInformation("Build Version: {BuildVersion}", buildVersion);
                logger.LogInformation("Build Timestamp: {BuildTimestamp}", buildTimestamp);
            });
}
