﻿namespace DecSm.Atom.Extensions.Dotnet;

public sealed record DotnetPackOptions(string ProjectName)
{
    public bool AutoSetVersion { get; init; } = true;

    public string Configuration { get; init; } = "Release";
}
