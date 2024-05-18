namespace DecSm.Atom.Workflows.Generation;

public class AtomWorkflowGenerator(AtomWorkflowBuilder workflowBuilder, IAtomBuild atomBuild, IEnumerable<IAtomWorkflowWriter> writers)
{
    private readonly List<IAtomWorkflowWriter> _writers = writers.ToList();

    public void GenerateWorkflows()
    {
        var workflowDefinitions = ((AtomBuild)atomBuild).Workflows;

        foreach (var workflowDefinition in workflowDefinitions)
        foreach (var workflowType in workflowDefinition.WorkflowTypes)
            _writers.FirstOrDefault(w => w.WorkflowType == workflowType.GetType())?.Generate(workflowBuilder.Build(workflowDefinition));
    }
}