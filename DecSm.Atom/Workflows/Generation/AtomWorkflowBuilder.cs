namespace DecSm.Atom.Workflows.Generation;

public sealed class AtomWorkflowBuilder(IAtomBuild build)
{
    public Workflow Build(WorkflowDefinition definition)
    {
        var jobs = new List<WorkflowJob>();

        foreach (var stepDefinition in definition.StepDefinitions)
        {
            var step = stepDefinition.CreateStep(build);
            jobs.Add(new(step.Name, [step]));
        }

        return new(definition.Name, jobs);
    }
}