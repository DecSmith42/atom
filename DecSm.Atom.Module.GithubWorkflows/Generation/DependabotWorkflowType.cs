﻿namespace DecSm.Atom.Module.GithubWorkflows.Generation;

public sealed record DependabotWorkflowType : IWorkflowType
{
    public bool IsRunning => !string.IsNullOrWhiteSpace(Github.Variables.Workflow);
}