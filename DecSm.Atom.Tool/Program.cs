var app = ConsoleApp.Create();
app.Add<CommandModel>();
app.UseFilter<RunArgsFilter>();
app.Run(args);
