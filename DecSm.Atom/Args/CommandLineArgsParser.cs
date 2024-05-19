namespace DecSm.Atom.Args;

public static class CommandLineArgsParser
{
    public static CommandLineArgs Parse(string[] rawArgs, IAtomBuildDefinition buildDefinition)
    {
        List<IArg> args = [];
        
        for (var i = 0; i < rawArgs.Length; i++)
        {
            var rawArg = rawArgs[i];
            
            // Help
            if (string.Equals(rawArg, "-h", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(rawArg, "--help", StringComparison.OrdinalIgnoreCase))
            {
                args.Add(new HelpArg());
                
                continue;
            }
            
            // Gen
            if (string.Equals(rawArg, "-g", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(rawArg, "--gen", StringComparison.OrdinalIgnoreCase))
            {
                args.Add(new GenArg());
                
                continue;
            }
            
            // Skip
            if (string.Equals(rawArg, "-s", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(rawArg, "--skip", StringComparison.OrdinalIgnoreCase))
            {
                args.Add(new SkipArg());
                
                continue;
            }
            
            // Params
            if (rawArg.StartsWith("--"))
            {
                var argParam = rawArg[2..];
                var matchedParam = false;
                
                foreach (var buildParam in buildDefinition.ParamDefinitions.Where(buildParam =>
                             string.Equals(argParam, buildParam.Value.Attribute.ArgName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (i == rawArgs.Length - 1)
                        throw new ArgumentException($"Missing value for parameter '{argParam}'");
                    
                    var nextArg = rawArgs[i + 1];
                    
                    if (nextArg.StartsWith("--") || nextArg.StartsWith("-"))
                        throw new ArgumentException($"Missing value for parameter '{argParam}'");
                    
                    args.Add(new ParamArg(buildParam.Value.Attribute.ArgName, buildParam.Key, nextArg));
                    i++;
                    matchedParam = true;
                    
                    break;
                }
                
                if (matchedParam)
                    continue;
            }
            
            // Commands
            var matchedCommand = false;
            
            var buildTargets = buildDefinition.TargetDefinitions.Where(buildTarget =>
                string.Equals(rawArg, buildTarget.Key, StringComparison.OrdinalIgnoreCase));
            
            foreach (var buildTarget in buildTargets)
            {
                args.Add(new CommandArg(buildTarget.Key));
                matchedCommand = true;
                
                break;
            }
            
            if (matchedCommand)
                continue;
            
            throw new ArgumentException($"Unknown argument '{rawArg}'");
        }
        
        return new(args.ToArray());
    }
}