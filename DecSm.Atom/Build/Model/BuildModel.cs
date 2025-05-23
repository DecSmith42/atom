﻿namespace DecSm.Atom.Build.Model;

/// <summary>
///     Contains information about the build run.
///     Generated by Atom on startup, this should be the primary source of information for the build.
///     Immutable by design, except for <see cref="TargetStates" /> values.
/// </summary>
[PublicAPI]
public sealed record BuildModel
{
    /// <summary>
    ///     All targets in the build - this includes targets that are not currently running or even scheduled.
    /// </summary>
    public required IReadOnlyList<TargetModel> Targets { get; init; }

    /// <summary>
    ///     The current state of all targets in the build.
    /// </summary>
    /// <seealso cref="TargetState" />
    public required IReadOnlyDictionary<TargetModel, TargetState> TargetStates { get; init; }

    /// <summary>
    ///     The assembly that declared the build.
    /// </summary>
    public required Assembly DeclaringAssembly { get; init; }

    /// <summary>
    ///     The target that is currently running.
    ///     Returns null if no target is currently running.
    /// </summary>
    public TargetModel? CurrentTarget =>
        Targets.FirstOrDefault(targetModel => TargetStates[targetModel] is { Status: TargetRunState.Running });

    /// <summary>
    ///     Gets a target by name.
    /// </summary>
    /// <param name="name">The name of the target to get.</param>
    /// <returns>The <see cref="TargetModel" /> with the specified name.</returns>
    /// <exception cref="ArgumentException">Thrown if the target with the specified name does not exist.</exception>
    public TargetModel GetTarget(string name) =>
        Targets.FirstOrDefault(t => t.Name == name) ?? throw new ArgumentException($"Target '{name}' not found.");
}
