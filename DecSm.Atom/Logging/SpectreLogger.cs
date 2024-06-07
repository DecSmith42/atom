namespace DecSm.Atom.Logging;

internal sealed class SpectreLogger(string categoryName, IExternalScopeProvider? scopeProvider) : ILogger
{
    public bool IsEnabled(LogLevel logLevel) =>
        logLevel != LogLevel.None;
    
    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull =>
        scopeProvider?.Push(state) ?? NullScope.Instance;
    
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;
        
        var levelText = string.Empty;
        var levelColour = string.Empty;
        var levelBackground = string.Empty;
        var messageStyle = string.Empty;
        
        switch (logLevel)
        {
            case LogLevel.Trace:
                levelText = "TRC";
                levelColour = "tan";
                messageStyle = "dim";
                
                break;
            
            case LogLevel.Debug:
                levelText = "DBG";
                levelColour = "chartreuse3";
                messageStyle = "dim";
                
                break;
            
            case LogLevel.Information:
                levelText = "INF";
                levelColour = "dodgerblue1";
                
                break;
            
            case LogLevel.Warning:
                levelText = "WRN";
                levelColour = "darkorange";
                
                break;
            
            case LogLevel.Error:
                levelText = "ERR";
                levelColour = "red3_1";
                
                break;
            
            case LogLevel.Critical:
                levelText = "FTL";
                levelColour = "white";
                levelBackground = " on red3_1";
                
                break;
            
            case LogLevel.None:
                break;
            
            default:
                throw new UnreachableException();
        }
        
        var time = DateTimeOffset.Now;
        string? command = null;
        var processOutput = false;
        
        scopeProvider?.ForEachScope((scopeData, logEventData) =>
            {
                if (scopeData is not Dictionary<string, object> scopeValues)
                    return;
                
                if (scopeValues.GetValueOrDefault("Command") is string currentCommand)
                    command = currentCommand;
                
                if (scopeValues.GetValueOrDefault("$ProcessOutput") is true)
                    processOutput = true;
            },
            state);
        
        if (processOutput)
        {
            var message = formatter(state, exception);
            
            if (message is "(null)")
                return;
            
            message = message.EscapeMarkup();
            
            // If the text contains any secrets, we don't want to log it
            message = ParamServiceAccessor.Service?.MaskSecrets(message) ?? message;
            
            messageStyle = Regex.IsMatch(message, "error|exception|fail", RegexOptions.IgnoreCase)
                ? "red"
                : "dim";
            
            var columns = new Columns(new Text("                "),
                new Markup($"[{messageStyle}]{message.EscapeMarkup()}[/]").LeftJustified()).Collapse();
            
            AnsiConsole.Write(columns);
            
            return;
        }
        
        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn("Info")
            .AddColumn("Message")
            .AddRow($"[dim]{time:yy-MM-dd zzz}[/]", $"[dim]{FormatCategoryName(categoryName.EscapeMarkup(), command)}:[/]")
            .AddRow($"[dim]{time:HH:mm:ss.fff}[/] [bold {levelColour}{levelBackground}]{levelText}[/]",
                $"[{messageStyle}]{formatter(state, exception).EscapeMarkup()}[/]")
            .AddRow(string.Empty);
        
        if (exception != null)
        {
            const ExceptionFormats exceptionFormat =
                ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes | ExceptionFormats.ShortenMethods;
            
            table.AddRow(new Text(string.Empty), exception.GetRenderable(exceptionFormat));
            table.AddRow(string.Empty);
        }
        
        AnsiConsole.Write(table);
    }
    
    private string FormatCategoryName(string name, string? command)
    {
        if (name == typeof(AtomService).FullName)
            return "Atom";
        
        if (command is not null)
            return $"{command} | {name}";
        
        return name;
    }
}