namespace DecSm.Atom.Variables;

public interface IVariablesHelper : IBuildAccessor
{
    /// <summary>
    ///     Writes a variable to the workflow context, making it available for later steps.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="value">The value of the variable.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task WriteVariable(string name, string value) =>
        GetService<IWorkflowVariableService>()
            .WriteVariable(name, value);
}
