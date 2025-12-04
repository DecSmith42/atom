namespace DecSm.Atom.Workflows.Definition.Options;

/// <summary>
///     Provides a base implementation for strongly-typed workflow options that carry data values.
///     This abstract record enables the creation of type-safe workflow options with self-referencing
///     generic patterns for enhanced type safety and fluent APIs.
/// </summary>
/// <typeparam name="TData">
///     The type of data that this workflow option carries. This can be any type including
///     primitives (string, bool, int), complex objects, or custom data structures.
/// </typeparam>
/// <typeparam name="TSelf">
///     The concrete type implementing this workflow option. This self-referencing type parameter
///     enables type-safe operations, static factory methods, and ensures that derived types
///     maintain their specific type identity through inheritance chains.
/// </typeparam>
/// <remarks>
///     <para>
///         This abstract record serves as the foundation for all data-carrying workflow options
///         in the DecSm.Atom workflow system. It implements the <see cref="IWorkflowOption" /> interface
///         while providing strongly-typed data handling and self-referencing capabilities.
///     </para>
///     <para>
///         The self-referencing generic pattern (TSelf) is used to ensure that methods and operations
///         on derived types return the correct concrete type rather than the base type, enabling
///         fluent APIs and type-safe operations.
///     </para>
///     <para>
///         Common implementations include:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///                 <see cref="ToggleWorkflowOption{TSelf}" /> - Boolean toggle options with predefined
///                 Enabled/Disabled states
///             </description>
///         </item>
///         <item>
///             <description><see cref="WorkflowEnvironmentInjection" /> - String-based environment variable injections</description>
///         </item>
///         <item>
///             <description><see cref="WorkflowSecretInjection" /> - String-based secret value injections</description>
///         </item>
///         <item>
///             <description><see cref="WorkflowSecretsEnvironmentInjection" /> - Secure environment variable injections</description>
///         </item>
///     </list>
///     <para>
///         The class provides virtual members that can be overridden in derived types to customize
///         behavior such as the <see cref="AllowMultiple" /> property for controlling instance merging.
///     </para>
/// </remarks>
/// <example>
///     <para>Creating a custom workflow option:</para>
///     <code>
/// // Simple string-based option
/// public sealed record BuildConfiguration : WorkflowOption&lt;string, BuildConfiguration&gt;
/// {
///     public override bool AllowMultiple => false; // Only one build configuration allowed
/// }
/// // Complex data option
/// public sealed record DatabaseSettings : WorkflowOption&lt;DatabaseConfig, DatabaseSettings&gt;
/// {
///     public override bool AllowMultiple => false;
/// }
/// // Usage
/// var config = new BuildConfiguration { Value = "Release" };
/// var dbSettings = new DatabaseSettings
/// {
///     Value = new DatabaseConfig { ConnectionString = "...", Timeout = 30 }
/// };
/// </code>
/// </example>
/// <seealso cref="IWorkflowOption" />
/// <seealso cref="ToggleWorkflowOption{TSelf}" />
/// <seealso cref="WorkflowEnvironmentInjection" />
[PublicAPI]
public abstract record WorkflowOption<TData, TSelf> : IWorkflowOption
    where TSelf : WorkflowOption<TData, TSelf>, new()
{
    /// <summary>
    ///     Gets or initializes the data value carried by this workflow option.
    /// </summary>
    /// <value>
    ///     The data value of type <typeparamref name="TData" />. Can be <c>null</c> if the
    ///     data type is nullable or if no value has been assigned.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         This property holds the actual data that the workflow option carries. The type
    ///         of data is defined by the <typeparamref name="TData" /> generic parameter and
    ///         can represent any kind of configuration, setting, or parameter needed by the workflow.
    ///     </para>
    ///     <para>
    ///         Examples of common data types used:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <c>string</c> - For text-based configurations like environment variable values, file paths, or
    ///                 connection strings
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description><c>bool</c> - For toggle options and feature flags</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <c>int</c>, <c>double</c> - For numeric configurations like timeouts, retry counts, or
    ///                 thresholds
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>Custom objects - For complex structured configuration data</description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         The property is virtual, allowing derived types to provide custom implementations
    ///         if needed, such as validation logic or computed values.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // String value example
    /// var envOption = new WorkflowEnvironmentInjection
    /// {
    ///     Value = "production"
    /// };
    /// // Boolean value example
    /// var toggleOption = new UseAzureKeyVault
    /// {
    ///     Value = true
    /// };
    /// // Access the value
    /// string environment = envOption.Value;
    /// bool useKeyVault = toggleOption.Value;
    /// </code>
    /// </example>
    public virtual TData? Value { get; init; }

    public virtual bool AllowMultiple => false;

    public static TSelf Create(TData value) =>
        new()
        {
            Value = value,
        };
}
