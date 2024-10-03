namespace DecSm.Atom.Module.DevopsWorkflows.Triggers;

public sealed record DevopsManualBoolInput(string Name, string Description, bool? DefaultValue = null) : ManualInput(Name, Description)
{
    public static DevopsManualBoolInput ForParam(ParamDefinition paramDefinition) =>
        new(paramDefinition.Attribute.ArgName,
            paramDefinition.Attribute.Description,
            paramDefinition.Attribute.DefaultValue is null
                ? null
                : bool.Parse(paramDefinition.Attribute.DefaultValue));
}
