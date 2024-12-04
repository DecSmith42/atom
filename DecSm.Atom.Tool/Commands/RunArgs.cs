namespace DecSm.Atom.Tool.Commands;

internal sealed record RunArgs(string[] Args);

internal sealed class RunArgsBinder : BinderBase<RunArgs>
{
    public static RunArgsBinder Instance { get; } = new();

    protected override RunArgs GetBoundValue(BindingContext bindingContext)
    {
        var tokens = bindingContext.ParseResult.Tokens;

        if (tokens.Count > 0 && string.Equals(tokens[0].Value, "run", StringComparison.OrdinalIgnoreCase))
            tokens = tokens
                .Skip(1)
                .ToList();

        return new(tokens
            .Select(t => t.Value)
            .ToArray());
    }
}
