﻿namespace DecSm.Atom.Params;

// NOTE: If the constructor args are modified, the BuildDefinitionSourceGenerator will need to be updated

/// <summary>
///     Attribute to define a parameter for a command.
/// </summary>
/// <remarks>If attributing a secret value, use <see cref="SecretDefinitionAttribute" /> instead.</remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Property)]
public class ParamDefinitionAttribute(
    string argName,
    string description,
    string? defaultValue = null,
    ParamSource sources = ParamSource.All
) : Attribute
{
    public string ArgName => argName;

    public string Description => description;

    public string? DefaultValue => defaultValue;

    public ParamSource Sources => sources;

    public bool IsSecret { get; protected init; }
}
