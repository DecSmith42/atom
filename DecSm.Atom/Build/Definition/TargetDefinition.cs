namespace DecSm.Atom.Build.Definition;

public sealed class TargetDefinition
{
    public string Name { get; init; } = string.Empty;
    
    public List<Func<Task>> Tasks { get; } = [];
    
    public List<string> Dependencies { get; } = [];
    
    public List<Expression<Func<string?>>> Requirements { get; } = [];
    
    public List<(string TargetName, string ArtifactName)> ConsumedArtifacts { get; } = [];
    
    public List<(string ArtifactName, string? ArtifactPath)> ProducedArtifacts { get; } = [];
    
    public TargetDefinition Executes(Func<Task> task)
    {
        if (Tasks.Contains(task))
            return this;
        
        Tasks.Add(task);
        
        return this;
    }
    
    public TargetDefinition DependsOn<T>()
    {
        var targets = typeof(T)
            .GetProperties()
            .Where(x => x.PropertyType == typeof(Target))
            .ToList();
        
        if (targets.Count != 1)
            throw new InvalidOperationException($"Type '{typeof(T).Name}' must have exactly one property of type 'Target'.");
        
        Dependencies.Add(targets[0].Name);
        
        return this;
    }
    
    public TargetDefinition Requires(Expression<Func<string?>> requirement)
    {
        if (requirement.Body is not MemberExpression)
            throw new ArgumentException("Invalid expression type.");
        
        Requirements.Add(requirement);
        
        return this;
    }
    
    public TargetDefinition Produces(string artifactName, string? artifactPath = null)
    {
        ProducedArtifacts.Add((artifactName, artifactPath));
        
        return this;
    }
    
    public TargetDefinition Consumes<T>(string artifactName)
        where T : IAtomBuildDefinition
    {
        var name = typeof(T).Name;
        
        if (name.Length > 1 && name.StartsWith('I') && char.IsUpper(name[1]))
            name = name[1..];
        
        ConsumedArtifacts.Add((name, artifactName));
        
        return this;
    }
}