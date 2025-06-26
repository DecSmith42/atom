namespace DecSm.Atom.Tool;

internal static class CommandExtensions
{
    public static Command WithAction(this Command command, Func<ParseResult, CancellationToken, Task<int>> action)
    {
        command.SetAction(action);

        return command;
    }

    public static RootCommand WithAction(this RootCommand command, Func<ParseResult, CancellationToken, Task<int>> action)
    {
        command.SetAction(action);

        return command;
    }
}
