#pragma warning disable CA1822, RCS1163

using JetBrains.Annotations;

namespace DecSm.Atom.Tool;

[PublicAPI]
internal sealed class CommandModel
{
    /// <summary>
    ///     Runs the specified atom project with the given arguments.
    /// </summary>
    /// <param name="context">The console app context.</param>
    /// <param name="runArgs">Arguments to pass to the atom project.</param>
    /// <param name="project">-p, The atom project to run.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The command exit code.</returns>
    [Command("")]
#pragma warning disable CA1822
    public Task<int> Root(
        ConsoleAppContext context,
        [Argument] string[]? runArgs = null,
        string project = "_atom",
        CancellationToken cancellationToken = default) =>
        RunCommand.Handle(context
                .Arguments
                .Skip(context.EscapeIndex)
                .ToArray(),
            project,
            cancellationToken);

    /// <summary>
    ///     Adds a NuGet package source with the specified name and URL.
    /// </summary>
    /// <param name="name">The name of the NuGet source to add.</param>
    /// <param name="url">The URL of the NuGet source to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The command exit code.</returns>
    [Command("nuget-add")]
    public Task<int> NugetAdd(string name, string url, CancellationToken cancellationToken = default) =>
        NugetAddCommand.Handle(name, url, cancellationToken);
}
