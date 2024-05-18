namespace DecSm.Atom.Params;

public class ParamService(CommandLineArgs args, IConfiguration config) : IParamService
{
    private readonly Dictionary<ParamDefinition, string?> _cache = [];

    public string? GetParam(ParamDefinition paramDefinition)
    {
        if (_cache.TryGetValue(paramDefinition, out var value))
            return value;

        if (paramDefinition.Attribute.SourceFromCliArguments)
        {
            var matchingArg = args.Params.FirstOrDefault(x => x.ParamName == paramDefinition.Name);

            if (matchingArg is not null)
                _cache[paramDefinition] = matchingArg.ParamValue;
        }

        if (paramDefinition.Attribute.SourceFromEnvironmentVariables)
        {
            var envVar = Environment.GetEnvironmentVariable(paramDefinition.Name);

            if (envVar is not null)
                _cache[paramDefinition] = envVar;
        }

        if (paramDefinition.Attribute.SourceFromConfigurationFiles)
        {
            var configValue = config.GetSection("Params")[paramDefinition.Name];

            if (configValue is not null)
                _cache[paramDefinition] = configValue;
        }

        if (paramDefinition.Attribute.SourceFromSecrets)
        {
            // TODO
        }

        return _cache[paramDefinition] = null;
    }
}