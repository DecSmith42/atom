namespace DecSm.Atom.GithubWorkflows.Triggers;

public sealed record GithubManualChoiceInput(
    string Name,
    string Description,
    bool Required,
    IReadOnlyList<string> Choices,
    string? DefaultValue = null
) : ManualInput(Name, Description, Required);