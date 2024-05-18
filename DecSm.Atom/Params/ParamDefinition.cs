namespace DecSm.Atom.Params;

public sealed record ParamDefinition(string Name, Type DeclaringType, ParamAttribute Attribute);