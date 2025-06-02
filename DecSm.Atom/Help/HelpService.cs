namespace DecSm.Atom.Help;

/// <summary>
///     Interface for providing help-related functionalities for the build system.
/// </summary>
public interface IHelpService
{
    /// <summary>
    ///     Display help information for the Atom build.
    /// </summary>
    void ShowHelp();
}

internal sealed class HelpService(IAnsiConsole console, CommandLineArgs args, BuildModel buildModel, IConfiguration config) : IHelpService
{
    public void ShowHelp()
    {
        console.WriteLine();
        console.Write(new Markup("[bold]Usage[/]\n"));
        console.WriteLine();
        console.Write(new Markup("atom [teal][[options]][/]\n"));
        console.Write(new Markup("atom [blue][[command/s]][/] [fuchsia][[parameters]][/] [teal][[options]][/]\n"));
        console.WriteLine();

        console.Write(new Markup("[bold]Options[/]\n"));
        console.WriteLine();

        console.Write(new Markup("  [dim]-h,  --help[/]      [dim]Show help[/]\n"));
        console.Write(new Markup("  [dim]-g,  --gen[/]       [dim]Generate build script[/]\n"));
        console.Write(new Markup("  [dim]-s,  --skip[/]      [dim]Skip dependency execution[/]\n"));
        console.Write(new Markup("  [dim]-hl, --headless[/]  [dim]Run in headless mode[/]\n"));
        console.Write(new Markup("  [dim]-v,  --verbose[/]   [dim]Show verbose output[/]\n"));
        console.WriteLine();

        var targets = args.HasVerbose
            ? buildModel.Targets
            : args.Commands.Count is 0
                ? buildModel
                    .Targets
                    .Where(x => !x.IsHidden)
                    .ToList()
                : buildModel
                    .Targets
                    .Where(x => args
                        .Commands
                        .Select(command => command.Name)
                        .Contains(x.Name))
                    .ToList();

        var atomAssembly = typeof(HelpService).Assembly;

        var projectAssembly = buildModel.DeclaringAssembly;

        var atomTargets = new List<TargetModel>(targets.Count);
        var libraryTargets = new List<TargetModel>(targets.Count);
        var projectTargets = new List<TargetModel>(targets.Count);

        foreach (var target in targets)
        {
            var assembly = target.DeclaringAssembly;

            if (assembly == atomAssembly)
                atomTargets.Add(target);
            else if (assembly == projectAssembly)
                projectTargets.Add(target);
            else
                libraryTargets.Add(target);
        }

        if (atomTargets.Count > 0)
        {
            console.Write(new Markup("[bold]Atom Commands[/]\n"));
            console.WriteLine();

            foreach (var target in atomTargets)
                WriteCommand(target);
        }

        if (libraryTargets.Count > 0)
        {
            console.Write(new Markup("[bold]Library Commands[/]\n"));
            console.WriteLine();

            foreach (var target in libraryTargets)
                WriteCommand(target);
        }

        // ReSharper disable once InvertIf
        if (projectTargets.Count > 0)
        {
            console.Write(new Markup("[bold]Project Commands[/]\n"));
            console.WriteLine();

            foreach (var target in projectTargets)
                WriteCommand(target);
        }
    }

    private void WriteCommand(TargetModel target)
    {
        var title = target.Description is { Length: > 0 }
            ? $"[bold navy]{target.Name.EscapeMarkup()}[/] [dim]| {target.Description.EscapeMarkup()}[/]"
            : $"[bold navy]{target.Name.EscapeMarkup()}[/]";

        var tree = new Tree(title);

        var dependencies = target.Dependencies;

        if (dependencies.Count > 0 && args.HasVerbose)
        {
            var depTree = tree.AddNode("[dim bold yellow]Depends on[/]");

            foreach (var dependency in dependencies)
                depTree.AddNode($"[dim]{dependency.Name}[/]");
        }

        var secrets = target
            .RequiredParams
            .Where(x => x.IsSecret)
            .ToList();

        var optionalParams = target
            .RequiredParams
            .Except(secrets)
            .Where(x => x.DefaultValue is { Length: > 0 } ||
                        config
                            .GetSection("Params")[x.ArgName] is { Length: > 0 })
            .ToList();

        var requiredParams = target
            .RequiredParams
            .Except(secrets)
            .Except(optionalParams)
            .ToList();

        if (requiredParams.Count > 0)
        {
            var reqTree = tree.AddNode("[dim bold red]Requires[/]");

            foreach (var requiredParam in requiredParams)
            {
                var descriptionDisplay = requiredParam.Description is { Length: > 0 }
                    ? $"[dim] | {requiredParam.Description.EscapeMarkup()}[/]"
                    : string.Empty;

                reqTree.AddNode($"--{requiredParam.ArgName.EscapeMarkup()}{descriptionDisplay}");
            }
        }

        if (optionalParams.Count > 0)
        {
            var optTree = tree.AddNode("[dim bold green]Options[/]");

            foreach (var optionalParam in optionalParams)
            {
                var defaultValue = optionalParam.DefaultValue;

                var configuredValue = config
                    .GetSection("Params")[optionalParam.ArgName];

                var defaultDisplay = (defaultValue, configuredValue) switch
                {
                    ({ Length: > 0 }, { Length: > 0 }) when defaultValue == configuredValue =>
                        $"[dim] [[Default/Configured: {defaultValue.EscapeMarkup()}]][/]",
                    ({ Length: > 0 }, { Length: > 0 }) =>
                        $"[dim] [[Default: {defaultValue.EscapeMarkup()}]][/][dim][[Configured: {configuredValue.EscapeMarkup()}]][/]",
                    ({ Length: > 0 }, { Length: 0 }) => $"[dim] [[Default: {defaultValue.EscapeMarkup()}]][/]",
                    ({ Length: 0 }, { Length: > 0 }) => $"[dim] [[Configured: {configuredValue.EscapeMarkup()}]][/]",
                    _ => string.Empty,
                };

                var descriptionDisplay = optionalParam.Description is { Length: > 0 }
                    ? $"[dim] | {optionalParam.Description.EscapeMarkup()}[/]"
                    : string.Empty;

                optTree.AddNode($"--{optionalParam.ArgName.EscapeMarkup()}{defaultDisplay}{descriptionDisplay}");
            }
        }

        if (secrets.Count > 0)
        {
            var secTree = tree.AddNode("[dim bold purple]Secrets[/]");

            foreach (var secret in secrets)
            {
                var descriptionDisplay = secret.Description is { Length: > 0 }
                    ? $"[dim] | {secret.Description.EscapeMarkup()}[/]"
                    : string.Empty;

                secTree.AddNode($"--{secret.ArgName.EscapeMarkup()}{descriptionDisplay}");
            }
        }

        console.Write(tree);
        console.WriteLine();
    }
}
