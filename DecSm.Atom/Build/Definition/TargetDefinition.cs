namespace DecSm.Atom.Build.Definition;

public sealed class TargetDefinition
{
    public string Name { get; init; } = string.Empty;
    
    public List<Func<Task>> Tasks { get; } = [];
    
    public List<string> Dependencies { get; } = [];
    
    public List<string> Requirements { get; } = [];
    
    public List<(string Name, string? Path)> ProducedArtifacts { get; } = [];
    
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
    
    public TargetDefinition Requires<T>(Expression<Func<T>> requirement)
    {
        if (requirement.Body is not MemberExpression memberExpression)
            throw new ArgumentException("Invalid expression type.");
        
        Requirements.Add(memberExpression.Member.Name);
        
        return this;
    }
    
    public TargetDefinition Produces(string artifactName, string? artifactPath = null)
    {
        ProducedArtifacts.Add((artifactName, artifactPath));
        
        return this;
    }
}