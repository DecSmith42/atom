﻿namespace Atom.Targets.Test;

[TargetDefinition]
internal partial interface ITestManualParams
{
    [ParamDefinition("test-string-param", "Test string parameter")]
    string TestStringParam => GetParam(() => TestStringParam)!;

    Target TestManualParams =>
        d => d
            .WithDescription("Test manual parameters")
            .RequiresParam(nameof(TestStringParam))
            .Executes(() =>
            {
                Logger.LogInformation("TestStringParam: {TestStringParam}", TestStringParam);

                return Task.CompletedTask;
            });
}
