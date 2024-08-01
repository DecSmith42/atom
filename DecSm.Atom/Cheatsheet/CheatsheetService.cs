namespace DecSm.Atom.Cheatsheet;

public interface ICheatsheetService
{
    public void ShowCheatsheet();
}

internal sealed class CheatsheetService(IAnsiConsole console, IBuildDefinition buildDefinition, BuildModel buildModel) : ICheatsheetService
{
    public void ShowCheatsheet()
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
        console.Write(new Markup("  [dim]-s,  --skip[/]      [dim]Skip target execution[/]\n"));
        console.Write(new Markup("  [dim]-hl, --headless[/]  [dim]Run in headless mode[/]\n"));
        console.WriteLine();

        console.Write(new Markup("[bold]Commands[/]\n"));
        console.WriteLine();

        foreach (var target in buildModel.Targets)
            WriteCommand(target);
    }

    private void WriteCommand(TargetModel target)
    {
        var title = target.Description is { Length: > 0 }
            ? $"[bold navy]{target.Name}[/] [dim]| {target.Description}[/]"
            : $"[bold navy]{target.Name}[/]";

        var tree = new Tree(title);

        var dependencies = target.Dependencies;

        if (dependencies.Count > 0)
        {
            var depTree = tree.AddNode("[dim bold yellow]Depends on[/]");

            foreach (var dependency in dependencies)
                depTree.AddNode($"[dim]{dependency.Name}[/]");
        }

        var allParams = target
            .RequiredParams
            .Select(x => buildDefinition.ParamDefinitions[x])
            .ToList();

        var secrets = allParams
            .Where(x => x.Attribute.IsSecret)
            .ToList();

        var optionalParams = allParams
            .Except(secrets)
            .Where(x => x.Attribute.DefaultValue is { Length: > 0 })
            .ToList();

        var requiredParams = allParams
            .Except(secrets)
            .Except(optionalParams)
            .ToList();

        if (requiredParams.Count > 0)
        {
            var reqTree = tree.AddNode("[dim bold red]Requires[/]");

            foreach (var requiredParam in requiredParams)
                reqTree.AddNode($"--{requiredParam.Attribute.ArgName} [dim][[{requiredParam.Attribute.Description}]][/]");
        }

        if (optionalParams.Count > 0)
        {
            var optTree = tree.AddNode("[dim bold green]Options[/]");

            foreach (var optionalParam in optionalParams)
                optTree.AddNode($"--{optionalParam.Attribute.ArgName} [dim][[Default: {optionalParam.Attribute.DefaultValue}]][/]");
        }

        if (secrets.Count > 0)
        {
            var secTree = tree.AddNode("[dim bold purple]Secrets[/]");

            foreach (var secret in secrets)
                secTree.AddNode($"--{secret.Attribute.ArgName}");
        }

        console.Write(tree);
        console.WriteLine();
    }
}