namespace DecSm.Atom.Workflows.Generation;

public sealed class AtomWorkflowBuilder(IAtomBuildDefinition buildDefinition)
{
    public Workflow Build(WorkflowDefinition definition)
    {
        var jobs = new List<WorkflowJob>();

        foreach (var stepDefinition in definition.StepDefinitions)
        {
            var step = stepDefinition.CreateStep(buildDefinition);
            jobs.Add(new(step.Name, [step]));
        }

        return new(definition.Name, jobs);
    }
}