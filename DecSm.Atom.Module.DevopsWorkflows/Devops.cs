namespace DecSm.Atom.Module.DevopsWorkflows;

[PublicAPI]
public static class Devops
{
    public static bool IsDevopsPipelines => Variables.TfBuild.Equals("true", StringComparison.OrdinalIgnoreCase);

    public static string PipelinePublishDirectory => "$(Build.BinariesDirectory)";

    public static string PipelineArtifactDirectory => "$(Build.ArtifactStagingDirectory)";

    public static DevopsWorkflowType WorkflowType { get; } = new();

    [PublicAPI]
    public static class VariableNames
    {
        public const string SystemDebug = "SYSTEM_DEBUG";
        public const string AgentBuildDirectory = "AGENT_BUILDDIRECTORY";
        public const string AgentHomeDirectory = "AGENT_HOMEDIRECTORY";
        public const string AgentId = "AGENT_ID";
        public const string AgentJobName = "AGENT_JOBNAME";
        public const string AgentMachineName = "AGENT_MACHINENAME";
        public const string AgentName = "AGENT_NAME";
        public const string AgentOs = "AGENT_OS";
        public const string AgentOsArchitecture = "AGENT_OSARCHITECTURE";
        public const string AgentTempDirectory = "AGENT_TEMPDIRECTORY";
        public const string AgentToolsDirectory = "AGENT_TOOLSDIRECTORY";
        public const string AgentWorkFolder = "AGENT_WORKFOLDER";
        public const string BuildArtifactStagingDirectory = "BUILD_ARTIFACTSTAGINGDIRECTORY";
        public const string BuildBuildId = "BUILD_BUILDID";
        public const string BuildBuildNumber = "BUILD_BUILDNUMBER";
        public const string BuildBuildUri = "BUILD_BUILDURI";
        public const string BuildBinariesDirectory = "BUILD_BINARIESDIRECTORY";
        public const string BuildContainerId = "BUILD_CONTAINERID";
        public const string BuildCronScheduleDisplayName = "BUILD_CRONSCHEDULE_DISPLAYNAME";
        public const string BuildDefinitionName = "BUILD_DEFINITIONNAME";
        public const string BuildQueuedBy = "BUILD_QUEUEDBY";
        public const string BuildQueuedById = "BUILD_QUEUEDBYID";
        public const string BuildReason = "BUILD_REASON";
        public const string BuildRepositoryClean = "BUILD_REPOSITORY_CLEAN";
        public const string BuildRepositoryLocalPath = "BUILD_REPOSITORY_LOCALPATH";
        public const string BuildRepositoryId = "BUILD_REPOSITORY_ID";
        public const string BuildRepositoryName = "BUILD_REPOSITORY_NAME";
        public const string BuildRepositoryProvider = "BUILD_REPOSITORY_PROVIDER";
        public const string BuildRepositoryTfvcWorkspace = "BUILD_REPOSITORY_TFVC_WORKSPACE";
        public const string BuildRepositoryUri = "BUILD_REPOSITORY_URI";
        public const string BuildRequestedFor = "BUILD_REQUESTEDFOR";
        public const string BuildRequestedForEmail = "BUILD_REQUESTEDFOREMAIL";
        public const string BuildRequestedForId = "BUILD_REQUESTEDFORID";
        public const string BuildSourceBranch = "BUILD_SOURCEBRANCH";
        public const string BuildSourceBranchName = "BUILD_SOURCEBRANCHNAME";
        public const string BuildSourcesDirectory = "BUILD_SOURCESDIRECTORY";
        public const string BuildSourceVersion = "BUILD_SOURCEVERSION";
        public const string BuildSourceVersionMessage = "BUILD_SOURCEVERSIONMESSAGE";
        public const string BuildStagingDirectory = "BUILD_STAGINGDIRECTORY";
        public const string BuildRepositoryGitSubmoduleCheckout = "BUILD_REPOSITORY_GIT_SUBMODULECHECKOUT";
        public const string BuildSourceTfvcShelveset = "BUILD_SOURCETFVC_SHELVESET";
        public const string BuildTriggeredByBuildId = "BUILD_TRIGGEREDBY_BUILDID";
        public const string BuildTriggeredByDefinitionId = "BUILD_TRIGGEREDBY_DEFINITIONID";
        public const string BuildTriggeredByDefinitionName = "BUILD_TRIGGEREDBY_DEFINITIONNAME";
        public const string BuildTriggeredByBuildNumber = "BUILD_TRIGGEREDBY_BUILDNUMBER";
        public const string BuildTriggeredByProjectId = "BUILD_TRIGGEREDBY_PROJECTID";
        public const string CommonTestResultsDirectory = "COMMON_TESTRESULTSDIRECTORY";
        public const string EnvironmentName = "ENVIRONMENT_NAME";
        public const string EnvironmentId = "ENVIRONMENT_ID";
        public const string EnvironmentResourceName = "ENVIRONMENT_RESOURCENAME";
        public const string EnvironmentResourceId = "ENVIRONMENT_RESOURCEID";
        public const string StrategyName = "STRATEGY_NAME";
        public const string StrategyCycleName = "STRATEGY_CYCLENAME";
        public const string SystemAccessToken = "SYSTEM_ACCESSTOKEN";
        public const string SystemCollectionId = "SYSTEM_COLLECTIONID";
        public const string SystemCollectionUri = "SYSTEM_COLLECTIONURI";
        public const string SystemDefaultWorkingDirectory = "SYSTEM_DEFAULTWORKINGDIRECTORY";
        public const string SystemDefinitionId = "SYSTEM_DEFINITIONID";
        public const string SystemHostType = "SYSTEM_HOSTTYPE";
        public const string SystemJobAttempt = "SYSTEM_JOBATTEMPT";
        public const string SystemJobDisplayName = "SYSTEM_JOBDISPLAYNAME";
        public const string SystemJobId = "SYSTEM_JOBID";
        public const string SystemJobName = "SYSTEM_JOBNAME";
        public const string SystemOidcRequestUri = "SYSTEM_OIDCREQUESTURI";
        public const string SystemPhaseAttempt = "SYSTEM_PHASEATTEMPT";
        public const string SystemPhaseDisplayName = "SYSTEM_PHASEDISPLAYNAME";
        public const string SystemPhaseName = "SYSTEM_PHASENAME";
        public const string SystemPlanId = "SYSTEM_PLANID";
        public const string SystemPullRequestIsFork = "SYSTEM_PULLREQUEST_ISFORK";
        public const string SystemPullRequestPullRequestId = "SYSTEM_PULLREQUEST_PULLREQUESTID";
        public const string SystemPullRequestPullRequestNumber = "SYSTEM_PULLREQUEST_PULLREQUESTNUMBER";
        public const string SystemPullRequestTargetBranchName = "SYSTEM_PULLREQUEST_TARGETBRANCHNAME";
        public const string SystemPullRequestSourceBranch = "SYSTEM_PULLREQUEST_SOURCEBRANCH";
        public const string SystemPullRequestSourceCommitId = "SYSTEM_PULLREQUEST_SOURCECOMMITID";
        public const string SystemPullRequestSourceRepositoryUri = "SYSTEM_PULLREQUEST_SOURCEREPOSITORYURI";
        public const string SystemPullRequestTargetBranch = "SYSTEM_PULLREQUEST_TARGETBRANCH";
        public const string SystemPullRequestStageAttempt = "SYSTEM_PULLREQUEST_STAGEATTEMPT";
        public const string SystemPullRequestStageDisplayName = "SYSTEM_PULLREQUEST_STAGEDISPLAYNAME";
        public const string SystemPullRequestStageName = "SYSTEM_PULLREQUEST_STAGENAME";
        public const string SystemTeamFoundationCollectionUri = "SYSTEM_TEAMFOUNDATIONCOLLECTIONURI";
        public const string SystemTeamProject = "SYSTEM_TEAMPROJECT";
        public const string SystemTeamProjectId = "SYSTEM_TEAMPROJECTID";
        public const string SystemTeamTimelineId = "SYSTEM_TIMELINEID";
        public const string TfBuild = "TF_BUILD";
        public const string ChecksStageAttempt = "CHECKS_STAGEATTEMPT";
    }

    [PublicAPI]
    public static class Variables
    {
        /// <summary>
        ///     SYSTEM_DEBUG
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemDebug { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemDebug) ?? string.Empty;

        /// <summary>
        ///     AGENT_BUILDDIRECTORY
        /// </summary>
        /// <example>
        /// </example>
        public static string AgentBuildDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentBuildDirectory) ?? string.Empty;

        /// <summary>
        ///     AGENT_HOMEDIRECTORY
        /// </summary>
        /// <example>
        /// </example>
        public static string AgentHomeDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentHomeDirectory) ?? string.Empty;

        /// <summary>
        ///     AGENT_ID
        /// </summary>
        /// <example>
        /// </example>
        public static string AgentId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentId) ?? string.Empty;

        /// <summary>
        ///     AGENT_JOBNAME
        /// </summary>
        /// <example>
        /// </example>
        public static string AgentJobName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentJobName) ?? string.Empty;

        /// <summary>
        ///     AGENT_MACHINENAME
        /// </summary>
        /// <example>
        /// </example>
        public static string AgentMachineName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentMachineName) ?? string.Empty;

        /// <summary>
        ///     AGENT_NAME
        /// </summary>
        /// <example>
        /// </example>
        public static string AgentName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentName) ?? string.Empty;

        /// <summary>
        ///     AGENT_OS
        /// </summary>
        /// <example>
        /// </example>
        public static string AgentOs { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentOs) ?? string.Empty;

        /// <summary>
        ///     AGENT_OSARCHITECTURE
        /// </summary>
        /// <example>
        /// </example>
        public static string AgentOsArchitecture { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentOsArchitecture) ?? string.Empty;

        /// <summary>
        ///     AGENT_TEMPDIRECTORY
        /// </summary>
        /// <example>
        /// </example>
        public static string AgentTempDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentTempDirectory) ?? string.Empty;

        /// <summary>
        ///     AGENT_TOOLSDIRECTORY
        /// </summary>
        /// <example>
        /// </example>
        public static string AgentToolsDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentToolsDirectory) ?? string.Empty;

        /// <summary>
        ///     AGENT_WORKFOLDER
        /// </summary>
        /// <example>
        /// </example>
        public static string AgentWorkFolder { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentWorkFolder) ?? string.Empty;

        /// <summary>
        ///     BUILD_ARTIFACTSTAGINGDIRECTORY
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildArtifactStagingDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildArtifactStagingDirectory) ?? string.Empty;

        /// <summary>
        ///     BUILD_BUILDID
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildBuildId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildBuildId) ?? string.Empty;

        /// <summary>
        ///     BUILD_BUILDNUMBER
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildBuildNumber { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildBuildNumber) ?? string.Empty;

        /// <summary>
        ///     BUILD_BUILDURI
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildBuildUri { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildBuildUri) ?? string.Empty;

        /// <summary>
        ///     BUILD_BINARIESDIRECTORY
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildBinariesDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildBinariesDirectory) ?? string.Empty;

        /// <summary>
        ///     BUILD_CONTAINERID
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildContainerId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildContainerId) ?? string.Empty;

        /// <summary>
        ///     BUILD_CRONSCHEDULE_DISPLAYNAME
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildCronScheduleDisplayName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildCronScheduleDisplayName) ?? string.Empty;

        /// <summary>
        ///     BUILD_DEFINITIONNAME
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildDefinitionName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildDefinitionName) ?? string.Empty;

        /// <summary>
        ///     BUILD_QUEUEDBY
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildQueuedBy { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildQueuedBy) ?? string.Empty;

        /// <summary>
        ///     BUILD_QUEUEDBYID
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildQueuedById { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildQueuedById) ?? string.Empty;

        /// <summary>
        ///     BUILD_REASON
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildReason { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildReason) ?? string.Empty;

        /// <summary>
        ///     BUILD_REPOSITORY_CLEAN
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildRepositoryClean { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRepositoryClean) ?? string.Empty;

        /// <summary>
        ///     BUILD_REPOSITORY_LOCALPATH
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildRepositoryLocalPath { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRepositoryLocalPath) ?? string.Empty;

        /// <summary>
        ///     BUILD_REPOSITORY_ID
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildRepositoryId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRepositoryId) ?? string.Empty;

        /// <summary>
        ///     BUILD_REPOSITORY_NAME
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildRepositoryName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRepositoryName) ?? string.Empty;

        /// <summary>
        ///     BUILD_REPOSITORY_PROVIDER
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildRepositoryProvider { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRepositoryProvider) ?? string.Empty;

        /// <summary>
        ///     BUILD_REPOSITORY_TFVC_WORKSPACE
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildRepositoryTfvcWorkspace { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRepositoryTfvcWorkspace) ?? string.Empty;

        /// <summary>
        ///     BUILD_REPOSITORY_URI
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildRepositoryUri { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRepositoryUri) ?? string.Empty;

        /// <summary>
        ///     BUILD_REQUESTEDFOR
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildRequestedFor { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRequestedFor) ?? string.Empty;

        /// <summary>
        ///     BUILD_REQUESTEDFOREMAIL
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildRequestedForEmail { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRequestedForEmail) ?? string.Empty;

        /// <summary>
        ///     BUILD_REQUESTEDFORID
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildRequestedForId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRequestedForId) ?? string.Empty;

        /// <summary>
        ///     BUILD_SOURCEBRANCH
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildSourceBranch { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildSourceBranch) ?? string.Empty;

        /// <summary>
        ///     BUILD_SOURCEBRANCHNAME
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildSourceBranchName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildSourceBranchName) ?? string.Empty;

        /// <summary>
        ///     BUILD_SOURCESDIRECTORY
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildSourcesDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildSourcesDirectory) ?? string.Empty;

        /// <summary>
        ///     BUILD_SOURCEVERSION
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildSourceVersion { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildSourceVersion) ?? string.Empty;

        /// <summary>
        ///     BUILD_SOURCEVERSIONMESSAGE
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildSourceVersionMessage { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildSourceVersionMessage) ?? string.Empty;

        /// <summary>
        ///     BUILD_STAGINGDIRECTORY
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildStagingDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildStagingDirectory) ?? string.Empty;

        /// <summary>
        ///     BUILD_REPOSITORY_GIT_SUBMODULECHECKOUT
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildRepositoryGitSubmoduleCheckout { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRepositoryGitSubmoduleCheckout) ?? string.Empty;

        /// <summary>
        ///     BUILD_SOURCETFVC_SHELVESET
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildSourceTfvcShelveset { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildSourceTfvcShelveset) ?? string.Empty;

        /// <summary>
        ///     BUILD_TRIGGEREDBY_BUILDID
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildTriggeredByBuildId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildTriggeredByBuildId) ?? string.Empty;

        /// <summary>
        ///     BUILD_TRIGGEREDBY_DEFINITIONID
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildTriggeredByDefinitionId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildTriggeredByDefinitionId) ?? string.Empty;

        /// <summary>
        ///     BUILD_TRIGGEREDBY_DEFINITIONNAME
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildTriggeredByDefinitionName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildTriggeredByDefinitionName) ?? string.Empty;

        /// <summary>
        ///     BUILD_TRIGGEREDBY_BUILDNUMBER
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildTriggeredByBuildNumber { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildTriggeredByBuildNumber) ?? string.Empty;

        /// <summary>
        ///     BUILD_TRIGGEREDBY_PROJECTID
        /// </summary>
        /// <example>
        /// </example>
        public static string BuildTriggeredByProjectId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildTriggeredByProjectId) ?? string.Empty;

        /// <summary>
        ///     COMMON_TESTRESULTSDIRECTORY
        /// </summary>
        /// <example>
        /// </example>
        public static string CommonTestResultsDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.CommonTestResultsDirectory) ?? string.Empty;

        /// <summary>
        ///     ENVIRONMENT_NAME
        /// </summary>
        /// <example>
        /// </example>
        public static string EnvironmentName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.EnvironmentName) ?? string.Empty;

        /// <summary>
        ///     ENVIRONMENT_ID
        /// </summary>
        /// <example>
        /// </example>
        public static string EnvironmentId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.EnvironmentId) ?? string.Empty;

        /// <summary>
        ///     ENVIRONMENT_RESOURCENAME
        /// </summary>
        /// <example>
        /// </example>
        public static string EnvironmentResourceName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.EnvironmentResourceName) ?? string.Empty;

        /// <summary>
        ///     ENVIRONMENT_RESOURCEID
        /// </summary>
        /// <example>
        /// </example>
        public static string EnvironmentResourceId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.EnvironmentResourceId) ?? string.Empty;

        /// <summary>
        ///     STRATEGY_NAME
        /// </summary>
        /// <example>
        /// </example>
        public static string StrategyName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.StrategyName) ?? string.Empty;

        /// <summary>
        ///     STRATEGY_CYCLENAME
        /// </summary>
        /// <example>
        /// </example>
        public static string StrategyCycleName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.StrategyCycleName) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_ACCESSTOKEN
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemAccessToken { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemAccessToken) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_COLLECTIONID
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemCollectionId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemCollectionId) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_COLLECTIONURI
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemCollectionUri { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemCollectionUri) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_DEFAULTWORKINGDIRECTORY
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemDefaultWorkingDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemDefaultWorkingDirectory) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_DEFINITIONID
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemDefinitionId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemDefinitionId) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_HOSTTYPE
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemHostType { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemHostType) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_JOBATTEMPT
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemJobAttempt { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemJobAttempt) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_JOBDISPLAYNAME
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemJobDisplayName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemJobDisplayName) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_JOBID
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemJobId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemJobId) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_JOBNAME
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemJobName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemJobName) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_OIDCREQUESTURI
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemOidcRequestUri { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemOidcRequestUri) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_PHASEATTEMPT
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemPhaseAttempt { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPhaseAttempt) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_PHASEDISPLAYNAME
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemPhaseDisplayName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPhaseDisplayName) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_PHASENAME
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemPhaseName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPhaseName) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_PLANID
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemPlanId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPlanId) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_PULLREQUEST_ISFORK
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemPullRequestIsFork { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestIsFork) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_PULLREQUEST_PULLREQUESTID
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemPullRequestPullRequestId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestPullRequestId) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_PULLREQUEST_PULLREQUESTNUMBER
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemPullRequestPullRequestNumber { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestPullRequestNumber) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_PULLREQUEST_TARGETBRANCHNAME
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemPullRequestTargetBranchName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestTargetBranchName) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_PULLREQUEST_SOURCEBRANCH
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemPullRequestSourceBranch { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestSourceBranch) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_PULLREQUEST_SOURCECOMMITID
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemPullRequestSourceCommitId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestSourceCommitId) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_PULLREQUEST_SOURCEREPOSITORYURI
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemPullRequestSourceRepositoryUri { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestSourceRepositoryUri) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_PULLREQUEST_TARGETBRANCH
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemPullRequestTargetBranch { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestTargetBranch) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_PULLREQUEST_STAGEATTEMPT
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemPullRequestStageAttempt { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestStageAttempt) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_PULLREQUEST_STAGEDISPLAYNAME
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemPullRequestStageDisplayName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestStageDisplayName) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_PULLREQUEST_STAGENAME
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemPullRequestStageName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestStageName) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_TEAMFOUNDATIONCOLLECTIONURI
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemTeamFoundationCollectionUri { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemTeamFoundationCollectionUri) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_TEAMPROJECT
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemTeamProject { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemTeamProject) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_TEAMPROJECTID
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemTeamProjectId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemTeamProjectId) ?? string.Empty;

        /// <summary>
        ///     SYSTEM_TIMELINEID
        /// </summary>
        /// <example>
        /// </example>
        public static string SystemTeamTimelineId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemTeamTimelineId) ?? string.Empty;

        /// <summary>
        ///     TF_BUILD
        /// </summary>
        /// <example>
        /// </example>
        public static string TfBuild { get; } =
            Environment.GetEnvironmentVariable(VariableNames.TfBuild) ?? string.Empty;

        /// <summary>
        ///     CHECKS_STAGEATTEMPT
        /// </summary>
        /// <example>
        /// </example>
        public static string ChecksStageAttempt { get; } =
            Environment.GetEnvironmentVariable(VariableNames.ChecksStageAttempt) ?? string.Empty;
    }
}
