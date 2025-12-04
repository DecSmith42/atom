namespace DecSm.Atom.Workflows.Definition.Options;

/// <summary>
///     Similar to <see cref="WorkflowSecretInjection" />, but prevents <see cref="ISecretsProvider" /> instances from
///     looking up the secret
///     value.
///     Typically used for secrets required by <see cref="ISecretsProvider" /> implementations themselves.
/// </summary>
[PublicAPI]
public sealed record WorkflowSecretsSecretInjection : WorkflowOption<string, WorkflowSecretsSecretInjection>
{
    public override bool AllowMultiple => true;
}
