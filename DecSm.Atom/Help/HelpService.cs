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
            .Params
            .Where(x => x.Param.IsSecret)
            .ToList();

        var optionalParams = target
            .Params
            .Except(secrets)
            .Where(x => !x.Required)
            .ToList();

        var requiredParams = target
            .Params
            .Except(secrets)
            .Except(optionalParams)
            .ToList();

        if (requiredParams.Count > 0)
        {
            var nodes = new List<(string Name, string Value, bool IsSupplied)>(requiredParams.Count);

            foreach (var requiredParam in requiredParams)
            {
                var defaultValue = requiredParam.Param.DefaultValue ?? string.Empty;

                var configuredValue = config
                                          .GetSection("Params")[requiredParam.Param.ArgName] ??
                                      string.Empty;

                var suppliedDisplay = (defaultValue, configuredValue) switch
                {
                    ({ Length: > 0 }, { Length: > 0 }) when defaultValue == configuredValue =>
                        $"[dim] | [/][dim green][[✔ Default/Configured: {defaultValue.EscapeMarkup()}]][/]",
                    ({ Length: > 0 }, { Length: > 0 }) =>
                        $"[dim] | [/][dim green][[Default: {defaultValue.EscapeMarkup()}]][/][dim] [[✔ Configured: {configuredValue.EscapeMarkup()}]][/]",
                    ({ Length: > 0 }, { Length: 0 }) => $"[dim] | [/][dim green][[✔ Default: {defaultValue.EscapeMarkup()}]][/]",
                    ({ Length: 0 }, { Length: > 0 }) => $"[dim] | [/][dim green][[✔ Configured: {configuredValue.EscapeMarkup()}]][/]",
                    _ => "[dim] | [/][dim yellow][[⚠ None]][/]",
                };

                var descriptionDisplay = requiredParam.Param.Description is { Length: > 0 }
                    ? $"[dim] | {requiredParam.Param.Description.EscapeMarkup()}[/]"
                    : string.Empty;

                var nameDisplay = $"--{requiredParam.Param.ArgName.EscapeMarkup()}";

                nodes.Add((requiredParam.Param.ArgName, $"{nameDisplay}{suppliedDisplay}{descriptionDisplay}",
                    defaultValue is { Length: > 0 } || configuredValue is { Length: > 0 }));
            }

            var reqTree = tree.AddNode("[bold red]Requires[/]");

            reqTree.AddNodes(nodes
                .Where(x => !x.IsSupplied)
                .OrderBy(x => x.Name)
                .Select(x => x.Value));

            reqTree.AddNodes(nodes
                .Where(x => x.IsSupplied)
                .OrderBy(x => x.Name)
                .Select(x => x.Value));
        }

        if (optionalParams.Count > 0)
        {
            var nodes = new List<(string Name, string Value, bool IsSupplied)>(optionalParams.Count);

            foreach (var optionalParam in optionalParams)
            {
                var defaultValue = optionalParam.Param.DefaultValue ?? string.Empty;

                var configuredValue = config
                                          .GetSection("Params")[optionalParam.Param.ArgName] ??
                                      string.Empty;

                var suppliedDisplay = (defaultValue, configuredValue) switch
                {
                    ({ Length: > 0 }, { Length: > 0 }) when defaultValue == configuredValue =>
                        $"[dim] | [/][dim green][[✔ Default/Configured: {defaultValue.EscapeMarkup()}]][/]",
                    ({ Length: > 0 }, { Length: > 0 }) =>
                        $"[dim] | [/][dim green][[Default: {defaultValue.EscapeMarkup()}]][/][dim] [[✔ Configured: {configuredValue.EscapeMarkup()}]][/]",
                    ({ Length: > 0 }, { Length: 0 }) => $"[dim] | [/][dim green][[✔ Default: {defaultValue.EscapeMarkup()}]][/]",
                    ({ Length: 0 }, { Length: > 0 }) => $"[dim] | [/][dim green][[✔ Configured: {configuredValue.EscapeMarkup()}]][/]",
                    _ => "[dim] | [/][dim][[✔ None]][/]",
                };

                var descriptionDisplay = optionalParam.Param.Description is { Length: > 0 }
                    ? $"[dim] | {optionalParam.Param.Description.EscapeMarkup()}[/]"
                    : string.Empty;

                var nameDisplay = $"--{optionalParam.Param.ArgName.EscapeMarkup()}";

                nodes.Add((optionalParam.Param.ArgName, $"{nameDisplay}{suppliedDisplay}{descriptionDisplay}",
                    defaultValue is { Length: > 0 } || configuredValue is { Length: > 0 }));
            }

            var optTree = tree.AddNode("[bold green]Options[/]");

            optTree.AddNodes(nodes
                .Where(x => !x.IsSupplied)
                .OrderBy(x => x.Name)
                .Select(x => x.Value));

            optTree.AddNodes(nodes
                .Where(x => x.IsSupplied)
                .OrderBy(x => x.Name)
                .Select(x => x.Value));
        }

        if (secrets.Count > 0)
        {
            var nodes = new List<(string Name, string Value, bool IsSupplied)>(secrets.Count);

            foreach (var secret in secrets)
            {
                var defaultValue = secret.Param.DefaultValue ?? string.Empty;

                var configuredValue = config
                                          .GetSection("Params")[secret.Param.ArgName] ??
                                      string.Empty;

                var descriptionDisplay = secret.Param.Description is { Length: > 0 }
                    ? $"[dim] | {secret.Param.Description.EscapeMarkup()}[/]"
                    : string.Empty;

                var suppliedDisplay = (defaultValue, configuredValue) switch
                {
                    ({ Length: > 0 }, { Length: > 0 }) when defaultValue == configuredValue =>
                        "[dim] | [/][dim green][[✔ Default/Configured: ****]][/]",
                    ({ Length: > 0 }, { Length: > 0 }) => "[dim] | [/][dim green][[Default: ****]][/][dim][[✔ Configured: ****]][/]",
                    ({ Length: > 0 }, { Length: 0 }) => "[dim] | [/][dim green][[✔ Default: ****]][/]",
                    ({ Length: 0 }, { Length: > 0 }) => "[dim] | [/][dim green][[✔ Configured: ****]][/]",
                    _ => string.Empty,
                };

                var nameDisplay = $"--{secret.Param.ArgName.EscapeMarkup()}";

                nodes.Add((secret.Param.ArgName, $"{nameDisplay}{suppliedDisplay}{descriptionDisplay}",
                    defaultValue is { Length: > 0 } || configuredValue is { Length: > 0 }));
            }

            var secTree = tree.AddNode("[bold purple]Secrets[/]");

            secTree.AddNodes(nodes
                .Where(x => !x.IsSupplied)
                .OrderBy(x => x.Name)
                .Select(x => x.Value));

            secTree.AddNodes(nodes
                .Where(x => x.IsSupplied)
                .OrderBy(x => x.Name)
                .Select(x => x.Value));
        }

        console.Write(tree);
        console.WriteLine();
    }
}
