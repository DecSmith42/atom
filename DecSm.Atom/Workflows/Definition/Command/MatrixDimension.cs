namespace DecSm.Atom.Workflows.Definition.Command;

[PublicAPI]
public record MatrixDimension(string Name, IReadOnlyList<string> Values);
