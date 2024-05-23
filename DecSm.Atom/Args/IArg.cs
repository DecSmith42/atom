namespace DecSm.Atom.Args;

public interface IArg;

public sealed record CommandArg(string Name) : IArg;

public sealed record GenArg : IArg;

public sealed record HelpArg : IArg;

public sealed record ParamArg(string ArgName, string ParamName, string ParamValue) : IArg;

public sealed record SkipArg : IArg;

public sealed record HeadlessArg : IArg;