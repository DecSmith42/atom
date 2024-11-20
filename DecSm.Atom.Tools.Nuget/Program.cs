var feedsOption = new Option<string>("--feed", "The feed to add/remove.");

var cancelTokenValueSource = new CancellationTokenValueSource();

var result = await new CommandLineBuilder(new RootCommand
    {
        new Command("nuget-add")
        {
            feedsOption,
        }.WithHandler(NugetAddHandler.Handle, new NugetAddOptionsBinder(feedsOption), cancelTokenValueSource),
    })
    .UseDefaults()
    .CancelOnProcessTermination()
    .Build()
    .InvokeAsync(args);

Environment.Exit(result);
