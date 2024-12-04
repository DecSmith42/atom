namespace DecSm.Atom.Tool;

internal static class CommandLineExtensions
{
    public static Command WithHandler<T1, T2>(
        this Command command,
        Func<T1, T2, Task> handle,
        IValueDescriptor<T1> symbol1,
        IValueDescriptor<T2> symbol2)
    {
        command.SetHandler(handle, symbol1, symbol2);

        return command;
    }

    public static Command WithHandler<T1, T2, T3>(
        this Command command,
        Func<T1, T2, T3, Task> handle,
        IValueDescriptor<T1> symbol1,
        IValueDescriptor<T2> symbol2,
        IValueDescriptor<T3> symbol3)
    {
        command.SetHandler(handle, symbol1, symbol2, symbol3);

        return command;
    }
}
