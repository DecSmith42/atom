namespace Atom;

[TargetDefinition]
internal partial interface INameMismatch
{
    [ParamDefinition("test-thingy", "A test thingy")]
    string? TestThingy => GetParam(() => TestThingy);

    Target NotMyName =>
        d => d
            .WithDescription("Wololo1")
            .Executes(() =>
            {
                Logger.LogInformation("Not my name but that's ok! {TestThingy}", TestThingy);

                return Task.CompletedTask;
            });

    Target AlsoNotMyName =>
        d => d
            .WithDescription("Wololo2")
            .Executes(() =>
            {
                Logger.LogInformation("Also not my name but that's ok! {TestThingy}", TestThingy);

                return Task.CompletedTask;
            });
}