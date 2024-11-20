namespace DecSm.Atom.Tools.Nuget;

public static class CommandLineExtensions
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
}
