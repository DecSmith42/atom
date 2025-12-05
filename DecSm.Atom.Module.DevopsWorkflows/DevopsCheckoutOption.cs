namespace DecSm.Atom.Module.DevopsWorkflows;

/// <summary>
///     Represents a workflow option for configuring the checkout step in Azure DevOps Pipelines.
/// </summary>
/// <remarks>
///     This option allows customization of the checkout action, such as enabling LFS and handling submodules.
/// </remarks>
[PublicAPI]
public sealed record
    DevopsCheckoutOption : WorkflowOption<DevopsCheckoutOption.DevopsCheckoutOptionValues, DevopsCheckoutOption>
{
    /// <inheritdoc />
    public override bool AllowMultiple => false;

    /// <summary>
    ///     Defines the configurable values for the checkout step.
    /// </summary>
    /// <param name="Lfs">Whether to enable Git LFS support.</param>
    /// <param name="Submodules">How to handle submodules (e.g., "true", "recursive", "false").</param>
    [PublicAPI]
    public sealed record DevopsCheckoutOptionValues(bool Lfs = false, string? Submodules = null);
}
