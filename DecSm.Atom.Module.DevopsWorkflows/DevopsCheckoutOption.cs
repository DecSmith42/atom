namespace DecSm.Atom.Module.DevopsWorkflows;

[PublicAPI]
public sealed record
    DevopsCheckoutOption : WorkflowOption<DevopsCheckoutOption.DevopsCheckoutOptionValues, DevopsCheckoutOption>
{
    public override bool AllowMultiple => false;

    [PublicAPI]
    public sealed record DevopsCheckoutOptionValues(bool Lfs = false, string? Submodules = null);
}
