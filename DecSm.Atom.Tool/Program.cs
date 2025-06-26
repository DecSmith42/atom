return await Model
    .RootCommand
    .Parse(args)
    .InvokeAsync();
