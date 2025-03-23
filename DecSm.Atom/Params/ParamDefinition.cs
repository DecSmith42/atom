﻿namespace DecSm.Atom.Params;

/// <summary>
///     Definition for a parameter that is used at runtime.
/// </summary>
/// <param name="Name">
///     The name of the parameter.
/// </param>
[PublicAPI]
public sealed record ParamDefinition(string Name)
{
    /// <summary>
    ///     The name of the argument that is used to pass the parameter.
    ///     The name will typically be prefixed with '--' when used as a command line argument.
    /// </summary>
    public required string ArgName { get; init; }

    /// <summary>
    ///     The human-readable description of the parameter.
    ///     This is used to generate help text.
    ///     Not required but highly recommended.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    ///     The default value for the parameter.
    ///     This value will be used if the parameter is not provided by the user or other sources.
    /// </summary>
    public required string? DefaultValue { get; init; }

    /// <summary>
    ///     The source/s that this parameter can be provided from.
    ///     Defaults to <see cref="ParamSource.All" />.
    /// </summary>
    public required ParamSource Sources { get; init; }

    /// <summary>
    ///     Whether the parameter is a secret.
    ///     Secrets can be sourced from <see cref="ISecretsProvider" /> implementations,
    ///     and are masked in logs and other output.
    /// </summary>
    public required bool IsSecret { get; init; }
}
