namespace DecSm.Atom.Build.Model;

public sealed class ExecutableBuild(IAtomBuildDefinition buildDefinition, CommandLineArgs args)
{
    public List<ExecutableTarget> Targets { get; } = [];
    
    public Dictionary<ExecutableTarget, TargetRunState> TargetStates { get; } = [];
    
    public Dictionary<ExecutableTarget, TimeSpan> TargetDurations { get; } = [];
    
    public ExecutableTarget GetTarget(string name) =>
        Targets.FirstOrDefault(t => t.TargetDefinition.Name == name) ?? throw new ArgumentException($"Target '{name}' not found.");
    
    public ExecutableTarget? CurrentTarget => Targets.FirstOrDefault(t => TargetStates[t] is TargetRunState.Running);
    
    public void Init()
    {
        AddOrderedTargets();
        InitTargetStates();
        InitTargetDurations();
    }
    
    private void AddOrderedTargets()
    {
        var allTargets = GetExecutableTargets(buildDefinition.TargetDefinitions);
        var targetDependencies = new List<TargetDependency>();
        
        foreach (var target in allTargets)
        {
            targetDependencies.AddRange(target
                .Dependencies
                .Select(dependencyTarget => new TargetDependency(target, dependencyTarget))
                .Where(dep => !targetDependencies.Contains(dep)));
            
            targetDependencies.AddRange(target
                .Dependents
                .Select(targetDependent => new TargetDependency(targetDependent, target))
                .Where(dep => !targetDependencies.Contains(dep)));
        }
        
        var targetsToAdd = new List<ExecutableTarget>();
        
        // Initial targets are those that have no dependencies
        targetsToAdd.AddRange(allTargets.Where(target => targetDependencies.All(dependency => dependency.To != target)));
        
        var targetCount = targetsToAdd.Count;
        
        var remainingTargets = allTargets
            .Where(x => !targetsToAdd.Contains(x))
            .ToList();
        
        while (remainingTargets.Count > 0)
        {
            // Insert a target's deps before it. Remove before insert if moving forward
            foreach (var remainingTarget in remainingTargets)
            {
                var firstDependentTarget = targetsToAdd.FirstOrDefault(t => t.Dependencies.Contains(remainingTarget));
                
                if (firstDependentTarget is null)
                    continue;
                
                var firstDependentTargetIndex = targetsToAdd.IndexOf(firstDependentTarget);
                
                targetsToAdd.Insert(firstDependentTargetIndex, remainingTarget);
                remainingTargets.Remove(remainingTarget);
                
                break;
            }
            
            if (targetCount == targetsToAdd.Count)
            {
                var orderedTargetsDisplay = string.Join("\n", targetsToAdd.Select(x => $"- {x.TargetDefinition.Name}"));
                
                var remainingTargetsDisplay = string.Join(", ",
                    allTargets
                        .Where(x => !targetsToAdd.Contains(x))
                        .Select(x => $"- {x.TargetDefinition.Name}"));
                
                throw new InvalidOperationException(
                    $"Circular dependency detected. Ordered targets:[\n{orderedTargetsDisplay}]\nRemaining targets:\n[{remainingTargetsDisplay}]");
            }
            
            targetCount = targetsToAdd.Count;
        }
        
        foreach (var target in targetsToAdd)
            AddTarget(target);
    }
    
    private void AddTarget(ExecutableTarget target)
    {
        foreach (var targetDep in target.Dependencies)
            AddTarget(targetDep);
        
        if (Targets.Contains(target))
            return;
        
        Targets.Add(target);
    }
    
    private void InitTargetStates()
    {
        TargetStates.Clear();
        
        foreach (var target in Targets)
            TargetStates[target] = TargetRunState.Uninitialized;
        
        foreach (var command in args.Commands)
            TargetStates[GetTarget(command.Name)] = TargetRunState.PendingRun;
        
        if (args.HasSkip)
        {
            foreach (var target in Targets.Where(x => TargetStates[x] is TargetRunState.Uninitialized))
                TargetStates[target] = TargetRunState.PendingSkip;
            
            return;
        }
        
        var uninitializedCount = Targets.Count(x => TargetStates[x] is TargetRunState.Uninitialized);
        
        while (Targets.Any(x => TargetStates[x] is TargetRunState.Uninitialized))
        {
            var pendingRunTargets = Targets
                .Where(x => TargetStates[x] is TargetRunState.PendingRun)
                .ToList();
            
            foreach (var dependency in pendingRunTargets.SelectMany(pendingRunTarget => pendingRunTarget.Dependencies))
                TargetStates[dependency] = TargetRunState.PendingRun;
            
            var newUninitializedCount = Targets.Count(x => TargetStates[x] is TargetRunState.Uninitialized);
            
            if (newUninitializedCount == uninitializedCount)
                break;
            
            uninitializedCount = newUninitializedCount;
        }
        
        foreach (var target in Targets.Where(target => TargetStates[target] is not TargetRunState.PendingRun))
            TargetStates[target] = TargetRunState.PendingSkip;
    }
    
    private static List<ExecutableTarget> GetExecutableTargets(Dictionary<string, Target> targets)
    {
        var targetDefinitions = targets
            .Select(x => x.Value(new()
            {
                Name = x.Key,
            }))
            .ToList();
        
        // Add deps from consumed artifacts and variables
        foreach (var targetDefinition in targetDefinitions)
        {
            targetDefinition.Dependencies.AddRange(targetDefinition.ConsumedArtifacts.Select(x => x.TargetName));
            targetDefinition.Dependencies.AddRange(targetDefinition.ConsumedVariables.Select(x => x.TargetName));
        }
        
        var executableTargets = targetDefinitions
            .Select(target => new ExecutableTarget(target))
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
        
        return executableTargets;
    }
    
    private void InitTargetDurations()
    {
        TargetDurations.Clear();
        
        foreach (var target in Targets)
            TargetDurations[target] = TimeSpan.Zero;
    }
}

public sealed record TargetDependency(ExecutableTarget From, ExecutableTarget To);