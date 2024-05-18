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
        var steps = definition
            .StepDefinitions
            .Select(stepDefinition => stepDefinition.CreateStep(buildDefinition))
            .ToList();
        
        var orderedSteps = steps
            .Where(x => x is not CommandWorkflowStep)
            .ToList();
        
        var unorderedSteps = steps
            .Where(x => x is CommandWorkflowStep)
            .ToList();
        
        foreach (var matchingStep in executableBuild
                     .Targets
                     .Select(target => unorderedSteps.FirstOrDefault(s => s.Name == target.TargetDefinition.Name))
                     .OfType<IWorkflowStep>())
        {
            orderedSteps.Add(matchingStep);
            unorderedSteps.Remove(matchingStep);
        }
        
        var jobs = orderedSteps
            .Select(step => new WorkflowJob(step.Name, [step]))
            .ToList();
        
        return new(definition.Name, jobs);
    }
}