namespace DecSm.Atom.Util;

public static class BuildUtil
{
    public static ExecutionPlan GetExecutionPlan(Dictionary<string, Target> targets)
    {
        var targetList = GetExecutableTargets(targets);
        var targetDependencies = new List<TargetDependency>();
        
        foreach (var target in targetList)
        foreach (var dependency in target.Dependencies)
            targetDependencies.Add(new(target, dependency));
        
        return new();
    }
    
    private sealed record TargetDependency(ExecutableTarget First, ExecutableTarget Second);
    
    public static List<ExecutableTarget> GetExecutableTargets(Dictionary<string, Target> targets)
    {
        var executableTargets = targets
            .Select(target => new ExecutableTarget(target.Value(new()
            {
                Name = target.Key,
            })))
            .ToList();
        
        foreach (var target in executableTargets)
        {
            foreach (var dependency in target.TargetDefinition.Dependencies)
            {
                var dependencyTarget = executableTargets.FirstOrDefault(t => t.TargetDefinition.Name == dependency);
                
                if (dependencyTarget is null)
                    throw new InvalidOperationException($"Dependency target '{dependency}' not found.");
                
                target.Dependencies.Add(dependencyTarget);
            }
        }
        
        foreach (var target in executableTargets)
        {
            target.Dependents.AddRange(executableTargets
                .Where(x => x != target)
                .Where(x => x.TargetDefinition.Dependencies.Contains(target.TargetDefinition.Name)));
        }
        
        return executableTargets;
    }
}