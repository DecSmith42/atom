﻿namespace DecSm.Atom;

/// <summary>See <see cref="SetupBuildInfo" /></summary>
[TargetDefinition]
public partial interface ISetupBuildInfo : IBuildInfo, IVariablesHelper, IReportsHelper
{
    /// <summary>
    ///     Gets the provider responsible for supplying the build identifier.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="IBuildIdProvider" /> obtained from the service container,
    ///     used to retrieve a unique identifier for the current build.
    /// </value>
    /// <remarks>
    ///     This property relies on service location (via <c>GetService&lt;IBuildIdProvider&gt;()</c>)
    ///     to resolve the appropriate provider. A default implementation of <see cref="IBuildIdProvider" />
    ///     is registered in the Atom framework's dependency injection container.
    /// </remarks>
    /// <seealso cref="IBuildIdProvider" />
    IBuildIdProvider BuildIdProvider => GetService<IBuildIdProvider>();

    /// <summary>
    ///     Gets the provider responsible for supplying the build version.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="IBuildVersionProvider" /> obtained from the service container,
    ///     used to determine the semantic or informational version of the build.
    /// </value>
    /// <remarks>
    ///     This property relies on service location (via <c>GetService&lt;IBuildVersionProvider&gt;()</c>)
    ///     to resolve the appropriate provider. A default implementation of <see cref="IBuildVersionProvider" />
    ///     is registered in the Atom framework's dependency injection container.
    /// </remarks>
    /// <seealso cref="IBuildVersionProvider" />
    IBuildVersionProvider BuildVersionProvider => GetService<IBuildVersionProvider>();

    /// <summary>
    ///     Gets the provider responsible for supplying the build timestamp.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="IBuildTimestampProvider" /> obtained from the service container,
    ///     used to ascertain the date and time at which the build is occurring or was initiated.
    /// </value>
    /// <remarks>
    ///     This property relies on service location (via <c>GetService&lt;IBuildTimestampProvider&gt;()</c>)
    ///     to resolve the appropriate provider. A default implementation of <see cref="IBuildTimestampProvider" />
    ///     is registered in the Atom framework's dependency injection container.
    /// </remarks>
    /// <seealso cref="IBuildTimestampProvider" />
    IBuildTimestampProvider BuildTimestampProvider => GetService<IBuildTimestampProvider>();

    /// <summary>
    ///     Gets the build target responsible for setting up the build ID, version, and timestamp.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <c>SetupBuildInfo</c> target is a foundational step in the Atom build pipeline. It performs several key actions:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     Retrieves the build name (from <see cref="IBuildInfo.BuildName" />) and writes it as a build variable named
    ///                     "BuildName".
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>Uses <see cref="BuildIdProvider" /> to get the build ID and writes it as a build variable named "BuildId".</description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Uses <see cref="BuildVersionProvider" /> to get the build version and writes it as a build variable named
    ///                     "BuildVersion".
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Uses <see cref="BuildTimestampProvider" /> to get the build timestamp, converts it to a string, and writes it
    ///                     as a build variable named "BuildTimestamp".
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>Adds a "Run Information" section to the build report, displaying the build name, version, ID, and timestamp.</description>
    ///             </item>
    ///             <item>
    ///                 <description>Logs the build ID, version, and timestamp using the configured logger for <c>ISetupBuildInfo</c>.</description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         This target is marked as hidden (<c>IsHidden()</c>), meaning it's not typically intended for direct invocation by users but is
    ///         expected to run as a dependency for other targets or as part of an initial setup sequence.
    ///     </para>
    ///     <para>
    ///         It produces the following build variables, which can be consumed by subsequent targets in the build process:
    ///         <list type="bullet">
    ///             <item>
    ///                 <term>
    ///                     <c>BuildName</c>
    ///                 </term>
    ///                 <description>The name of the build.</description>
    ///             </item>
    ///             <item>
    ///                 <term>
    ///                     <c>BuildId</c>
    ///                 </term>
    ///                 <description>The unique identifier for the build execution.</description>
    ///             </item>
    ///             <item>
    ///                 <term>
    ///                     <c>BuildVersion</c>
    ///                 </term>
    ///                 <description>The version of the software being built.</description>
    ///             </item>
    ///             <item>
    ///                 <term>
    ///                     <c>BuildTimestamp</c>
    ///                 </term>
    ///                 <description>The timestamp of the build (as a string).</description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         Successful execution relies on the correct registration and functioning of <see cref="IBuildIdProvider" />,
    ///         <see cref="IBuildVersionProvider" />, and <see cref="IBuildTimestampProvider" /> services within the Atom framework.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <para>
    ///         While <c>SetupBuildInfo</c> is often implicitly part of a default build definition, here's how you might
    ///         conceptualize its use and the consumption of variables it produces. Assume <c>BuildName</c> is set in the
    ///         build definition (e.g., by inheriting from <c>IBuildInfo</c>).
    ///     </para>
    ///     <code lang="csharp">
    /// // In your Atom build definition (e.g., Build.cs)
    /// public class MyBuild : BuildDefinition, ISetupBuildInfo
    /// {
    ///     // ISetupBuildInfo members (BuildIdProvider, etc.) would be implicitly available
    ///     // or explicitly implemented if not using a base class that provides them.
    ///     // The BuildName property is inherited from IBuildInfo
    ///     public override string BuildName => "MyAwesomeProject";
    ///     // A custom target that depends on SetupBuildInfo having run
    ///     Target Package => t => t
    ///         .DependsOn(nameof(ISetupBuildInfo.SetupBuildInfo)) // Ensures build info is set up
    ///         .Executes(async () =>
    ///         {
    ///             // Retrieve variables set by SetupBuildInfo
    ///             var buildId = await GetVariable&lt;string&gt;("BuildId");
    ///             var buildVersion = await GetVariable&lt;string&gt;("BuildVersion");
    ///             var buildTimestamp = await GetVariable&lt;string&gt;("BuildTimestamp");
    ///             // Log or use these variables
    ///             LogInformation($"Packaging {BuildName} version {buildVersion} (Build ID: {buildId}, Timestamp: {buildTimestamp})");
    ///             // ... further packaging logic ...
    ///         });
    ///     // Main build flow
    ///     Target Default => t => t.DependsOn(Package);
    /// }
    /// // To run this build (assuming Atom CLI):
    /// // atom Default
    /// </code>
    /// </example>
    /// <returns>
    ///     A <see cref="Target" /> configured to initialize build information variables and report data.
    /// </returns>
    /// <seealso cref="BuildIdProvider" />
    /// <seealso cref="BuildVersionProvider" />
    /// <seealso cref="BuildTimestampProvider" />
    /// <seealso cref="IBuildInfo.BuildName" />
    /// <seealso cref="IVariablesHelper.WriteVariable(string, string)" />
    /// <seealso cref="IReportsHelper.AddReportData(IReportData)" />
    Target SetupBuildInfo =>
        d => d
            .DescribedAs("Sets up the build ID, version, and timestamp")
            .IsHidden()
            .ProducesVariable(nameof(BuildName))
            .ProducesVariable(nameof(BuildId))
            .ProducesVariable(nameof(BuildVersion))
            .ProducesVariable(nameof(BuildTimestamp))
            .Executes(async () =>
            {
                var atomBuildName = BuildName; // BuildName is from IBuildInfo
                await WriteVariable(nameof(BuildName), atomBuildName);

                var buildId = BuildIdProvider.BuildId;
                await WriteVariable(nameof(BuildId), buildId);

                var buildVersion = BuildVersionProvider.Version;
                await WriteVariable(nameof(BuildVersion), buildVersion);

                var buildTimestamp = BuildTimestampProvider.Timestamp;
                await WriteVariable(nameof(BuildTimestamp), buildTimestamp.ToString());

                var reportedBuildId = buildId == buildVersion
                    ? buildId
                    : $"{buildVersion} - {buildId} [{buildTimestamp}]";

                AddReportData(new TextReportData($"{BuildName} | {reportedBuildId}")
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
