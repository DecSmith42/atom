﻿namespace DecSm.Atom.Tests.BuildTests.Targets;

[BuildDefinition]
public partial class TargetOverrideBuild : BuildDefinition, IOverrideTarget
{
    public bool BaseOverrideTargetExecuted { get; set; }

    public bool OverrideOverrideTargetExecuted { get; set; }
}

[TargetDefinition]
public partial interface IBaseOverrideTarget
{
    bool BaseOverrideTargetExecuted { get; set; }

    Target OverrideTarget =>
        d => d.Executes(() =>
        {
            BaseOverrideTargetExecuted = true;

            return Task.CompletedTask;
        });
}

[TargetDefinition]
public partial interface IOverrideTarget : IBaseOverrideTarget
{
    bool OverrideOverrideTargetExecuted { get; set; }

    new Target OverrideTarget =>
        d => d.Executes(() =>
        {
            OverrideOverrideTargetExecuted = true;

            return Task.CompletedTask;
        });
}