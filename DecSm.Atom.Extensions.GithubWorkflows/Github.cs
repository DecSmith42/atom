namespace DecSm.Atom.Extensions.GithubWorkflows;

public static class Github
{
    public static bool IsGithubActions => Variables.Actions.Equals("true", StringComparison.CurrentCultureIgnoreCase);

    public static string PipelinePublishDirectory => "${{ github.workspace }}/.github/publish";

    public static string PipelineArtifactDirectory => "${{ github.workspace }}/.github/artifacts";

    public static GithubWorkflowType WorkflowType { get; } = new();

    public static WorkflowDefinition DependabotWorkflow(DependabotOptions dependabotOptions) =>
        new("dependabot")
        {
            Options = [dependabotOptions],
            WorkflowTypes = [new DependabotWorkflowType()],
        };

    public static class VariableNames
    {
        public const string Actions = "GITHUB_ACTIONS";
        public const string ActorId = "GITHUB_ACTOR_ID";
        public const string Actor = "GITHUB_ACTOR";
        public const string ApiUrl = "GITHUB_API_URL";
        public const string BaseRef = "GITHUB_BASE_REF";
        public const string Env = "GITHUB_ENV";
        public const string EventName = "GITHUB_EVENT_NAME";
        public const string EventPath = "GITHUB_EVENT_PATH";
        public const string GraphqlUrl = "GITHUB_GRAPHQL_URL";
        public const string HeadRef = "GITHUB_HEAD_REF";
        public const string Job = "GITHUB_JOB";
        public const string Output = "GITHUB_OUTPUT";
        public const string Path = "GITHUB_PATH";
        public const string RefName = "GITHUB_REF_NAME";
        public const string RefProtected = "GITHUB_REF_PROTECTED";
        public const string RefType = "GITHUB_REF_TYPE";
        public const string Ref = "GITHUB_REF";
        public const string RepositoryId = "GITHUB_REPOSITORY_ID";
        public const string RepositoryOwnerId = "GITHUB_REPOSITORY_OWNER_ID";
        public const string RepositoryOwner = "GITHUB_REPOSITORY_OWNER";
        public const string Repository = "GITHUB_REPOSITORY";
        public const string RetentionDays = "GITHUB_RETENTION_DAYS";
        public const string RunAttempt = "GITHUB_RUN_ATTEMPT";
        public const string RunId = "GITHUB_RUN_ID";
        public const string RunNumber = "GITHUB_RUN_NUMBER";
        public const string ServerUrl = "GITHUB_SERVER_URL";
        public const string Sha = "GITHUB_SHA";
        public const string State = "GITHUB_STATE";
        public const string StepSummary = "GITHUB_STEP_SUMMARY";
        public const string TriggeringActor = "GITHUB_TRIGGERING_ACTOR";
        public const string WorkflowRef = "GITHUB_WORKFLOW_REF";
        public const string WorkflowSha = "GITHUB_WORKFLOW_SHA";
        public const string Workflow = "GITHUB_WORKFLOW";
        public const string Workspace = "GITHUB_WORKSPACE";
        public const string RunnerArch = "RUNNER_ARCH";
        public const string RunnerEnvironment = "RUNNER_ENVIRONMENT";
        public const string RunnerName = "RUNNER_NAME";
        public const string RunnerOs = "RUNNER_OS";
        public const string RunnerPerfLog = "RUNNER_PERFLOG";
        public const string RunnerTemp = "RUNNER_TEMP";
        public const string RunnerToolCache = "RUNNER_TOOL_CACHE";
        public const string RunnerTrackingId = "RUNNER_TRACKING_ID";
        public const string RunnerUser = "RUNNER_USER";
        public const string RunnerWorkspace = "RUNNER_WORKSPACE";
    }

    public static class Triggers
    {
        public static GithubManualTrigger Manual { get; } = new();

        public static GithubPushTrigger PushToMain { get; } = new()
        {
            IncludedBranches = ["main"],
        };

        public static GithubPullRequestTrigger PullIntoMain { get; } = new()
        {
            IncludedBranches = ["main"],
        };
    }

    public static class Variables
    {
        /// <summary>
        ///     GITHUB_ACTIONS
        /// </summary>
        /// <example>
        ///     true
        /// </example>
        public static string Actions { get; } = Environment.GetEnvironmentVariable(VariableNames.Actions) ?? string.Empty;

        /// <summary>
        ///     GITHUB_ACTOR_ID
        /// </summary>
        /// <example>
        ///     1234567
        /// </example>
        public static string ActorId { get; } = Environment.GetEnvironmentVariable(VariableNames.ActorId) ?? string.Empty;

        /// <summary>
        ///     GITHUB_ACTOR
        /// </summary>
        /// <example>
        ///     ArthurDent
        /// </example>
        public static string Actor { get; } = Environment.GetEnvironmentVariable(VariableNames.Actor) ?? string.Empty;

        /// <summary>
        ///     GITHUB_API_URL
        /// </summary>
        /// <example>
        ///     https://api.github.com
        /// </example>
        public static string ApiUrl { get; } = Environment.GetEnvironmentVariable(VariableNames.ApiUrl) ?? string.Empty;

        /// <summary>
        ///     GITHUB_BASE_REF
        /// </summary>
        /// <example>
        ///     main
        /// </example>
        public static string BaseRef { get; } = Environment.GetEnvironmentVariable(VariableNames.BaseRef) ?? string.Empty;

        /// <summary>
        ///     GITHUB_ENV
        /// </summary>
        /// <example>
        ///     /home/runner/work/_temp/_runner_file_commands/set_env_[GUID]
        /// </example>
        public static string Env { get; } = Environment.GetEnvironmentVariable(VariableNames.Env) ?? string.Empty;

        /// <summary>
        ///     GITHUB_EVENT_NAME
        /// </summary>
        /// <example>
        ///     pull_request
        /// </example>
        public static string EventName { get; } = Environment.GetEnvironmentVariable(VariableNames.EventName) ?? string.Empty;

        /// <summary>
        ///     GITHUB_EVENT_PATH
        /// </summary>
        /// <example>
        ///     /home/runner/work/_temp/_github_workflow/event.json
        /// </example>
        public static string EventPath { get; } = Environment.GetEnvironmentVariable(VariableNames.EventPath) ?? string.Empty;

        /// <summary>
        ///     GITHUB_GRAPHQL_URL
        /// </summary>
        /// <example>
        ///     https://api.github.com/graphql
        /// </example>
        public static string GraphqlUrl { get; } = Environment.GetEnvironmentVariable(VariableNames.GraphqlUrl) ?? string.Empty;

        /// <summary>
        ///     GITHUB_HEAD_REF
        /// </summary>
        /// <example>
        ///     my-branch
        /// </example>
        public static string HeadRef { get; } = Environment.GetEnvironmentVariable(VariableNames.HeadRef) ?? string.Empty;

        /// <summary>
        ///     GITHUB_JOB
        /// </summary>
        /// <example>
        ///     BuildApp
        /// </example>
        public static string Job { get; } = Environment.GetEnvironmentVariable(VariableNames.Job) ?? string.Empty;

        /// <summary>
        ///     GITHUB_OUTPUT
        /// </summary>
        /// <example>
        ///     /home/runner/work/_temp/_runner_file_commands/set_output_[GUID]
        /// </example>
        public static string Output { get; } = Environment.GetEnvironmentVariable(VariableNames.Output) ?? string.Empty;

        /// <summary>
        ///     GITHUB_PATH
        /// </summary>
        /// <example>
        ///     /home/runner/work/_temp/_runner_file_commands/add_path_[GUID]
        /// </example>
        public static string Path { get; } = Environment.GetEnvironmentVariable(VariableNames.Path) ?? string.Empty;

        /// <summary>
        ///     GITHUB_REF_NAME
        /// </summary>
        /// <example>
        ///     4/my-branch
        /// </example>
        public static string RefName { get; } = Environment.GetEnvironmentVariable(VariableNames.RefName) ?? string.Empty;

        /// <summary>
        ///     GITHUB_REF_PROTECTED
        /// </summary>
        /// <example>
        ///     false
        /// </example>
        public static string RefProtected { get; } = Environment.GetEnvironmentVariable(VariableNames.RefProtected) ?? string.Empty;

        /// <summary>
        ///     GITHUB_REF_TYPE
        /// </summary>
        /// <example>
        ///     branch
        /// </example>
        public static string RefType { get; } = Environment.GetEnvironmentVariable(VariableNames.RefType) ?? string.Empty;

        /// <summary>
        ///     GITHUB_REF
        /// </summary>
        /// <example>
        ///     refs/pull/4/merge
        /// </example>
        public static string Ref { get; } = Environment.GetEnvironmentVariable(VariableNames.Ref) ?? string.Empty;

        /// <summary>
        ///     GITHUB_REPOSITORY_ID
        /// </summary>
        /// <example>
        ///     123456789
        /// </example>
        public static string RepositoryId { get; } = Environment.GetEnvironmentVariable(VariableNames.RepositoryId) ?? string.Empty;

        /// <summary>
        ///     GITHUB_REPOSITORY_OWNER_ID
        /// </summary>
        /// <example>
        ///     1234567
        /// </example>
        public static string RepositoryOwnerId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RepositoryOwnerId) ?? string.Empty;

        /// <summary>
        ///     GITHUB_REPOSITORY_OWNER
        /// </summary>
        /// <example>
        ///     ArthurDent
        /// </example>
        public static string RepositoryOwner { get; } = Environment.GetEnvironmentVariable(VariableNames.RepositoryOwner) ?? string.Empty;

        /// <summary>
        ///     GITHUB_REPOSITORY
        /// </summary>
        /// <example>
        ///     ArthurDent/my-project
        /// </example>
        public static string Repository { get; } = Environment.GetEnvironmentVariable(VariableNames.Repository) ?? string.Empty;

        /// <summary>
        ///     GITHUB_RETENTION_DAYS
        /// </summary>
        /// <example>
        ///     90
        /// </example>
        public static string RetentionDays { get; } = Environment.GetEnvironmentVariable(VariableNames.RetentionDays) ?? string.Empty;

        /// <summary>
        ///     GITHUB_RUN_ATTEMPT
        /// </summary>
        /// <example>
        ///     1
        /// </example>
        public static string RunAttempt { get; } = Environment.GetEnvironmentVariable(VariableNames.RunAttempt) ?? string.Empty;

        /// <summary>
        ///     GITHUB_RUN_ID
        /// </summary>
        /// <example>
        ///     9876543210
        /// </example>
        public static string RunId { get; } = Environment.GetEnvironmentVariable(VariableNames.RunId) ?? string.Empty;

        /// <summary>
        ///     GITHUB_RUN_NUMBER
        /// </summary>
        /// <example>
        ///     1
        /// </example>
        public static string RunNumber { get; } = Environment.GetEnvironmentVariable(VariableNames.RunNumber) ?? string.Empty;

        /// <summary>
        ///     GITHUB_SERVER_URL
        /// </summary>
        /// <example>
        ///     https://github.com
        /// </example>
        public static string ServerUrl { get; } = Environment.GetEnvironmentVariable(VariableNames.ServerUrl) ?? string.Empty;

        /// <summary>
        ///     GITHUB_SHA
        /// </summary>
        /// <example>
        ///     [SHA]
        /// </example>
        public static string Sha { get; } = Environment.GetEnvironmentVariable(VariableNames.Sha) ?? string.Empty;

        /// <summary>
        ///     GITHUB_STATE
        /// </summary>
        /// <example>
        ///     /home/runner/work/_temp/_runner_file_commands/save_state_[GUID]
        /// </example>
        public static string State { get; } = Environment.GetEnvironmentVariable(VariableNames.State) ?? string.Empty;

        /// <summary>
        ///     GITHUB_STEP_SUMMARY
        /// </summary>
        /// <example>
        ///     /home/runner/work/_temp/_runner_file_commands/step_summary_[GUID]
        /// </example>
        public static string StepSummary { get; } = Environment.GetEnvironmentVariable(VariableNames.StepSummary) ?? string.Empty;

        /// <summary>
        ///     GITHUB_TRIGGERING_ACTOR
        /// </summary>
        /// <example>
        ///     ArthurDent
        /// </example>
        public static string TriggeringActor { get; } = Environment.GetEnvironmentVariable(VariableNames.TriggeringActor) ?? string.Empty;

        /// <summary>
        ///     GITHUB_WORKFLOW_REF
        /// </summary>
        /// <example>
        ///     ArthurDent/my-project/.github/workflows/BuildApp.yml@refs/pull/4/merge
        /// </example>
        public static string WorkflowRef { get; } = Environment.GetEnvironmentVariable(VariableNames.WorkflowRef) ?? string.Empty;

        /// <summary>
        ///     GITHUB_WORKFLOW_SHA
        /// </summary>
        /// <example>
        ///     [SHA]
        /// </example>
        public static string WorkflowSha { get; } = Environment.GetEnvironmentVariable(VariableNames.WorkflowSha) ?? string.Empty;

        /// <summary>
        ///     GITHUB_WORKFLOW
        /// </summary>
        /// <example>
        ///     build
        /// </example>
        public static string Workflow { get; } = Environment.GetEnvironmentVariable(VariableNames.Workflow) ?? string.Empty;

        /// <summary>
        ///     GITHUB_WORKSPACE
        /// </summary>
        /// <example>
        ///     /home/runner/work/my-project/my-project
        /// </example>
        public static string Workspace { get; } = Environment.GetEnvironmentVariable(VariableNames.Workspace) ?? string.Empty;

        /// <summary>
        ///     RUNNER_ARCH
        /// </summary>
        /// <example>
        ///     X64
        /// </example>
        public static string RunnerArch { get; } = Environment.GetEnvironmentVariable(VariableNames.RunnerArch) ?? string.Empty;

        /// <summary>
        ///     RUNNER_ENVIRONMENT
        /// </summary>
        /// <example>
        ///     github-hosted
        /// </example>
        public static string RunnerEnvironment { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RunnerEnvironment) ?? string.Empty;

        /// <summary>
        ///     RUNNER_NAME
        /// </summary>
        /// <example>
        ///     GitHubActions 6
        /// </example>
        public static string RunnerName { get; } = Environment.GetEnvironmentVariable(VariableNames.RunnerName) ?? string.Empty;

        /// <summary>
        ///     RUNNER_OS
        /// </summary>
        /// <example>
        ///     Linux
        /// </example>
        public static string RunnerOs { get; } = Environment.GetEnvironmentVariable(VariableNames.RunnerOs) ?? string.Empty;

        /// <summary>
        ///     RUNNER_PERFLOG
        /// </summary>
        /// <example>
        ///     /home/runner/perflog
        /// </example>
        public static string RunnerPerfLog { get; } = Environment.GetEnvironmentVariable(VariableNames.RunnerPerfLog) ?? string.Empty;

        /// <summary>
        ///     RUNNER_TEMP
        /// </summary>
        /// <example>
        ///     /home/runner/work/_temp
        /// </example>
        public static string RunnerTemp { get; } = Environment.GetEnvironmentVariable(VariableNames.RunnerTemp) ?? string.Empty;

        /// <summary>
        ///     RUNNER_TOOL_CACHE
        /// </summary>
        /// <example>
        ///     /opt/hostedtoolcache
        /// </example>
        public static string RunnerToolCache { get; } = Environment.GetEnvironmentVariable(VariableNames.RunnerToolCache) ?? string.Empty;

        /// <summary>
        ///     RUNNER_TRACKING_ID
        /// </summary>
        /// <example>
        ///     github_3cf77bfa-2ce5-4b4c-b736-d6a7b5c16537
        /// </example>
        public static string RunnerTrackingId { get; } = Environment.GetEnvironmentVariable(VariableNames.RunnerTrackingId) ?? string.Empty;

        /// <summary>
        ///     RUNNER_USER
        /// </summary>
        /// <example>
        ///     runner
        /// </example>
        public static string RunnerUser { get; } = Environment.GetEnvironmentVariable(VariableNames.RunnerUser) ?? string.Empty;

        /// <summary>
        ///     RUNNER_WORKSPACE
        /// </summary>
        /// <example>
        ///     /home/runner/work/my-project
        /// </example>
        public static string RunnerWorkspace { get; } = Environment.GetEnvironmentVariable(VariableNames.RunnerWorkspace) ?? string.Empty;
    }
}
