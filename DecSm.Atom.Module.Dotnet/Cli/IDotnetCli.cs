namespace DecSm.Atom.Module.Dotnet.Cli;

[PublicAPI]
public partial interface IDotnetCli;

[PublicAPI]
internal partial class DotnetCli(IProcessRunner processRunner) : IDotnetCli;
