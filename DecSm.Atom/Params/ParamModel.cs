namespace DecSm.Atom.Params;

public sealed record ParamModel(string Name)
{
    public required string ArgName { get; init; }

    public required string Description { get; init; }

    public required string? DefaultValue { get; init; }

    public required ParamSource Sources { get; init; }

    public required bool IsSecret { get; init; }
}
