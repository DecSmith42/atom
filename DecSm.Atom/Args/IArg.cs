namespace DecSm.Atom.Args;

[PublicAPI]
public interface IArg;

[PublicAPI]
public sealed record CommandArg(string Name) : IArg;

[PublicAPI]
public sealed record GenArg : IArg;

[PublicAPI]
public sealed record HelpArg : IArg;

[PublicAPI]
public sealed record ParamArg(string ArgName, string ParamName, string ParamValue) : IArg;

[PublicAPI]
public sealed record SkipArg : IArg;

[PublicAPI]
public sealed record HeadlessArg : IArg;

[PublicAPI]
public sealed record VerboseArg : IArg;

[PublicAPI]
public sealed record ProjectArg(string ProjectName) : IArg;
