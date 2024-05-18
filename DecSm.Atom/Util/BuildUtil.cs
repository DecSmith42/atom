namespace DecSm.Atom.Util;

public static class BuildUtil
{
//     public static ExecutionPlan GetExecutionPlan(Dictionary<string, Target> targets)
//     {
//         var targetList = GetExecutableTargets(targets);
//         var targetDependencies = new List<TargetDependency>();
//         
//         foreach (var target in targetList)
//         {
//             targetDependencies.AddRange(target.Dependencies.Select(dependency => new TargetDependency(target, dependency)));
//             targetDependencies.AddRange(target.Dependents.Select(targetDependent => new TargetDependency(targetDependent, target)));
//         }
//         
//         // Initial targets are those that have no dependencies
//         var orderedTargetList = targetList
//             .Where(target => targetDependencies.All(dependency => dependency.To != target))
//             .ToList();
//         
//         var targetCount = targetList.Count;
//         var orderedTargetCount = orderedTargetList.Count;
//         
//         while (orderedTargetCount < targetCount)
//         {
//             orderedTargetList.AddRange(targetList
//                 .Where(target => !orderedTargetList.Contains(target))
//                 .Where(target => targetDependencies
//                     .Where(dependency => orderedTargetList.Contains(dependency.From))
//                     .All(dependency => dependency.To != target)));
//             
//             if (orderedTargetCount == orderedTargetList.Count)
//                 throw new InvalidOperationException(
//                     $"Circular dependency detected. Ordered targets: [{string.Join(", ", orderedTargetList.Select(x => x.TargetDefinition.Name))}], remaining targets: [{string.Join(", ", targetList.Where(x => !orderedTargetList.Contains(x)).Select(x => x.TargetDefinition.Name))}]");
//             
//             orderedTargetCount = orderedTargetList.Count;
//         }
//     }
    
    private sealed record TargetDependency(ExecutableTarget From, ExecutableTarget To);
    
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