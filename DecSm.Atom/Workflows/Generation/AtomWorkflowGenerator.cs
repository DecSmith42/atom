namespace DecSm.Atom.Workflows.Generation;

public class AtomWorkflowGenerator(
    IAtomBuildDefinition buildDefinition,
    ExecutableBuild executableBuild,
    IEnumerable<IAtomWorkflowWriter> writers
)
{
    private readonly List<IAtomWorkflowWriter> _writers = writers.ToList();
    
    public void GenerateWorkflows()
    {
        var workflowDefinitions = ((AtomBuildDefinition)buildDefinition).Workflows;
        
        foreach (var workflowDefinition in workflowDefinitions)
        foreach (var workflowType in workflowDefinition.WorkflowTypes)
            _writers
                .FirstOrDefault(w => w.WorkflowType == workflowType.GetType())
                ?.Generate(BuildWorkflow(workflowDefinition));
    }
    
    private Workflow BuildWorkflow(WorkflowDefinition definition)
    {
        if (definition.StepDefinitions.Count == 0)
            return new(definition.Name, definition.Triggers, definition.Options, []);
        
        var definedSteps = definition
            .StepDefinitions
            .Select(stepDefinition => stepDefinition.CreateStep(buildDefinition))
            .ToList();
        
        var definedNonTargetJobs = definedSteps
            .Where(step => step is not CommandWorkflowStep)
            .Select(step => new WorkflowJob(step.Name, [step]))
            .ToList();
        
        var definedTargetJobs = definedSteps
            .OfType<CommandWorkflowStep>()
            .Select(step => new WorkflowJob(step.Name, [step]))
            .ToList();
        
        var commandJobMap = definedTargetJobs.ToDictionary(job => job.Name);
        
        foreach (var targetName in executableBuild.Targets.Select(x => x.TargetDefinition.Name))
            if (!commandJobMap.ContainsKey(targetName))
                commandJobMap[targetName] = new(targetName, [new CommandWorkflowStep(targetName)]);
        
        foreach (var jobName in commandJobMap.Keys)
        {
            var target = executableBuild.Targets.Single(x => x.TargetDefinition.Name == jobName);
            
            var job = commandJobMap[jobName];
            
            job.JobDependencies.AddRange(target.Dependencies.Select(x => x.TargetDefinition.Name));
            
            job.ParamRequirements.AddRange(target.TargetDefinition.Requirements.Select(x =>
                buildDefinition.ParamDefinitions[(x.Body as MemberExpression)!.Member.Name]));
            
            foreach (var consumedVariable in target.TargetDefinition.ConsumedVariables)
            {
                var consumedTarget = executableBuild.Targets.Single(x => x.TargetDefinition.Name == consumedVariable.TargetName);
                var jobOutput = consumedTarget.TargetDefinition.ProducedVariables.SingleOrDefault(x => x == consumedVariable.VariableName);
                
                if (jobOutput is null)
                    throw new InvalidOperationException(
                        $"Target '{jobName}' consumes variable '{consumedVariable.VariableName}' from target '{consumedVariable.TargetName}', which does not produce that variable.");
            }
        }
        
        var commandJobs = commandJobMap.Values.ToList();
        
        while (commandJobs.Count > 0)
        {
            var removedJob = false;
            
            foreach (var job in commandJobs.Where(x => !definedTargetJobs.Contains(x)))
            {
                if (commandJobs
                    .Where(x => x != job)
                    .SelectMany(x => x.JobDependencies)
                    .Any(x => x == job.Name))
                    continue;
                
                commandJobs.Remove(job);
                removedJob = true;
                
                break;
            }
            
            if (!removedJob)
                break;
        }
        
        var jobs = definedNonTargetJobs
            .Concat(OrderJobs(commandJobs, commandJobMap))
            .ToList();
        
        return new(definition.Name, definition.Triggers, definition.Options, jobs);
    }
    
    private static List<WorkflowJob> OrderJobs(List<WorkflowJob> jobs, Dictionary<string, WorkflowJob> nameJobMap)
    {
        var orderedJobs = new List<WorkflowJob>();
        
        foreach (var job in jobs)
            AddJob(job);
        
        return orderedJobs;
        
        void AddJob(WorkflowJob target)
        {
            foreach (var jobDep in target.JobDependencies)
                AddJob(nameJobMap[jobDep]);
            
            if (orderedJobs.Contains(target))
                return;
            
            orderedJobs.Add(target);
        }
    }
}