namespace DecSm.Atom.Tool;

internal class RunArgsFilter(ConsoleAppFilter next) : ConsoleAppFilter(next)
{
    public override async Task InvokeAsync(ConsoleAppContext context, CancellationToken cancellationToken)
    {
        if (context.Arguments.Length is 0 || context.Arguments[0] is "nuget-add")
        {
            await Next.InvokeAsync(context, cancellationToken);

            return;
        }

        await Next.InvokeAsync(new(context.CommandName,
                context.Arguments,
                ReadOnlyMemory<string>.Empty,
                context.State,
                null,
                context.CommandDepth,
                context.Arguments[0] is "-p" or "--project"
                    ? 2
                    : 0),
            cancellationToken);
    }
}
