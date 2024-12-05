var runArgs = new Argument<string[]>();
var projectOption = new Option<string>("--project", "The project to run.");

var nameOption = new Option<string>("--name", "The feed to add/remove.");
var urlOption = new Option<string>("--url", "The url of the feed to add/remove.");

var cancelTokenValueSource = new CancellationTokenValueSource();

var result = await new CommandLineBuilder(new RootCommand
    {
        runArgs,
        projectOption,
        new Command("run")
        {
            runArgs,
            projectOption,
        }.WithHandler(RunHandler.Handle, RunArgsBinder.Instance, new ProjectOptionBinder(projectOption), cancelTokenValueSource),
        new Command("nuget-add")
        {
            nameOption,
            urlOption,
        }.WithHandler(NugetAddHandler.Handle, new NugetAddOptionsBinder(nameOption, urlOption), cancelTokenValueSource),
    }.WithHandler(RunHandler.Handle, RunArgsBinder.Instance, new ProjectOptionBinder(projectOption), cancelTokenValueSource))
    .UseDefaults()
    .CancelOnProcessTermination()
    .Build()
    .InvokeAsync(args);

Environment.Exit(result);
