namespace DecSm.Atom.Module.DevopsWorkflows.Triggers;

public sealed record DevopsManualStringInput(string Name, string Description, string? DefaultValue = null) : ManualInput(Name, Description)
{
    public static DevopsManualStringInput ForParam(ParamDefinition paramDefinition) =>
        new(paramDefinition.Attribute.ArgName, paramDefinition.Attribute.Description, paramDefinition.Attribute.DefaultValue);
}
