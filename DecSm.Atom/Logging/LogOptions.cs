namespace DecSm.Atom.Logging;

/// <summary>
///     Provides configuration options related to logger verbosity across the Atom application.
/// </summary>
/// <remarks>
///     This class controls logging behaviors such as verbosity for debugging or detailed logs.
///     Adjustments affect logging framework behavior instantly throughout the application.
/// </remarks>
/// <example>
///     Example usage:
///     <code>
/// LogOptions.IsVerboseEnabled = true;
/// </code>
/// </example>
[PublicAPI]
public static class LogOptions
{
    /// <summary>
    ///     Gets or sets a value indicating whether logging verbosity is enabled, typically producing more detailed diagnostic information.
    /// </summary>
    /// <value>
    ///     <c>true</c> if verbose logging is enabled; otherwise, <c>false</c>.
    /// </value>
    public static bool IsVerboseEnabled { get; set; }
}
