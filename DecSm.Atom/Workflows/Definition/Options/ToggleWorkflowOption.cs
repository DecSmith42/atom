namespace DecSm.Atom.Workflows.Definition.Options;

/// <summary>
///     An abstract base record for creating workflow options that act as boolean toggles.
/// </summary>
/// <typeparam name="TSelf">The concrete type implementing this toggle option.</typeparam>
/// <remarks>
///     This class provides a foundation for creating boolean workflow options with predefined <see cref="Enabled" />
///     and <see cref="Disabled" /> states, along with a helper method for checking the enabled state.
/// </remarks>
/// <example>
///     <code>
/// public sealed record UseCustomFeature : ToggleWorkflowOption&lt;UseCustomFeature&gt;;
/// 
/// // Usage:
/// var options = new List&lt;IWorkflowOption&gt; { UseCustomFeature.Enabled };
/// bool isEnabled = UseCustomFeature.IsEnabled(options); // true
///     </code>
/// </example>
[PublicAPI]
public abstract record ToggleWorkflowOption<TSelf> : WorkflowOption<bool, TSelf>
    where TSelf : WorkflowOption<bool, TSelf>, new()
{
    /// <summary>
    ///     Gets a predefined instance representing the enabled state (<c>true</c>).
    /// </summary>
    public static readonly TSelf Enabled = new()
    {
        Value = true,
    };

    /// <summary>
    ///     Gets a predefined instance representing the disabled state (<c>false</c>).
    /// </summary>
    public static readonly TSelf Disabled = new()
    {
        Value = false,
    };

    /// <summary>
    ///     Determines whether this toggle option is enabled within a collection of workflow options.
    /// </summary>
    /// <param name="options">The collection of workflow options to check.</param>
    /// <returns><c>true</c> if an enabled instance of this option exists in the collection; otherwise, <c>false</c>.</returns>
#pragma warning disable RCS1158
    public static bool IsEnabled(IEnumerable<IWorkflowOption> options) =>
        options
            .OfType<TSelf>()
            .Any(x => x.Value);
#pragma warning restore RCS1158
}
