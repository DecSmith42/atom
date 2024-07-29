namespace DecSm.Atom.Process;

[PublicAPI]
[TargetDefinition]
public interface IProcessHelper : IBuildDefinition
{
    string RunProcess(string name, string args, string? workingDirectory = null, bool logInvocation = true, bool suppressError = false) =>
        Services
            .GetRequiredService<IProcessRunner>()
            .RunProcess(name, args, workingDirectory, logInvocation, suppressError);

    string RunProcess(string name, string[] args, string? workingDirectory = null, bool logInvocation = true, bool suppressError = false) =>
        Services
            .GetRequiredService<IProcessRunner>()
            .RunProcess(name, args, workingDirectory, logInvocation, suppressError);

    async Task<string> RunProcessAsync(
        string name,
        string args,
        string? workingDirectory = null,
        bool logInvocation = true,
        bool suppressError = false) =>
        await Services
            .GetRequiredService<IProcessRunner>()
            .RunProcessAsync(name, args, workingDirectory, logInvocation, suppressError);

    async Task<string> RunProcessAsync(
        string name,
        string[] args,
        string? workingDirectory = null,
        bool logInvocation = true,
        bool suppressError = false) =>
        await Services
            .GetRequiredService<IProcessRunner>()
            .RunProcessAsync(name, args, workingDirectory, logInvocation, suppressError);
}