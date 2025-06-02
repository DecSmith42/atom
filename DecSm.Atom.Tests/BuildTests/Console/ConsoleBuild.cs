namespace DecSm.Atom.Tests.BuildTests.Console;

[BuildDefinition]
public partial class ConsoleBuild : DefaultBuildDefinition, IConsoleTarget;

[TargetDefinition]
public partial interface IConsoleTarget
{
    [ParamDefinition("required-param", "Required param")]
    string RequiredParam => GetParam(() => RequiredParam)!;

    [ParamDefinition("default-param", "Default param", "default-value")]
    string DefaultParam => GetParam(() => DefaultParam, "default-value");

    [SecretDefinition("secret-param", "Secret param")]
    string SecretParam => GetParam(() => SecretParam)!;

    Target ConsoleTarget =>
        d => d
            .DescribedAs("Console target")
            .RequiresParam(nameof(RequiredParam))
            .RequiresParam(nameof(DefaultParam))
            .RequiresParam(nameof(SecretParam))
            .Executes(() => Task.CompletedTask);
}
