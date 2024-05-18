namespace DecSm.Atom.Args.Types;

public sealed record ParamArg(string ArgName, string ParamName, string ParamValue) : IArg;