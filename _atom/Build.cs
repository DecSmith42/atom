namespace Atom;

[BuildDefinition]
internal partial class Build : IPackAtom, IPackAtomGithubWorkflows, IPackAtomSourceGenerators, IPushToNuget, IDiagnostics
{
    public override WorkflowDefinition[] Workflows =>
    [
        new("Validate")
        {
            Triggers = [new GithubManualTrigger(), new GithubPullRequestTrigger(["main"])],
            StepDefinitions =
            [
                new CommandDefinition(nameof(IDiagnostics.Diagnostics)),
                new CommandDefinition(nameof(IPackAtom.PackAtom)),
                new CommandDefinition(nameof(IPackAtomGithubWorkflows.PackAtomGithubWorkflows)),
                new CommandDefinition(nameof(IPackAtomSourceGenerators.PackAtomSourceGenerators)),
            ],
            WorkflowTypes = [new GithubWorkflowType()],
        },
        new("Build")
        {
            Triggers = [new GithubManualTrigger(), new GithubPushTrigger(["main"])],
            Options = [new InjectGithubSecret(nameof(IPushToNuget.NugetApiKey))],
            StepDefinitions =
            [
                new CommandDefinition(nameof(IDiagnostics.Diagnostics)),
                new CommandDefinition(nameof(IPackAtom.PackAtom)),
                new CommandDefinition(nameof(IPackAtomGithubWorkflows.PackAtomGithubWorkflows)),
                new CommandDefinition(nameof(IPackAtomSourceGenerators.PackAtomSourceGenerators)),
                new CommandDefinition(nameof(IPushToNuget.PushToNuget)),
            ],
            WorkflowTypes = [new GithubWorkflowType()],
        },
    ];
}