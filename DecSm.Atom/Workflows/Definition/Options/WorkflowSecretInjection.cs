﻿namespace DecSm.Atom.Workflows.Definition.Options;

[PublicAPI]
public sealed record WorkflowSecretInjection : WorkflowOption<string, WorkflowSecretInjection>
{
    public override bool AllowMultiple => true;
}
