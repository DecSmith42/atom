namespace DecSm.Atom.Workflows.Definition.Options;

/// <summary>
///     Provides a base class for workflow options that represent boolean toggle functionality,
///     allowing features to be enabled or disabled within workflow configurations.
/// </summary>
/// <typeparam name="TSelf">
///     The concrete type implementing this toggle option. This type parameter enables
///     the self-referencing pattern for type-safe static factory methods and operations.
/// </typeparam>
/// <remarks>
///     <para>
///         This abstract record serves as a foundation for creating boolean workflow options
///         with predefined enabled/disabled states and convenient utility methods for checking
///         the enabled state across option collections.
///     </para>
///     <para>
///         Common implementations include:
///     </para>
///     <list type="bullet">
///         <item>
///             <description><c>UseAzureKeyVault</c> - Controls Azure Key Vault integration</description>
///         </item>
///         <item>
///             <description><c>UseCustomArtifactProvider</c> - Controls custom artifact provider usage</description>
///         </item>
///         <item>
///             <description><c>UseGitVersionForBuildId</c> - Controls GitVersion usage for build identification</description>
///         </item>
///         <item>
///             <description><c>ProvideDevopsRunIdAsWorkflowId</c> - Controls Azure DevOps run ID usage</description>
///         </item>
///         <item>
///             <description><c>ProvideGithubRunIdAsWorkflowId</c> - Controls GitHub run ID usage</description>
///         </item>
///     </list>
///     <para>
///         The class inherits from <see cref="WorkflowOption{TData, TSelf}" /> with <c>bool</c> as the data type,
///         providing strongly-typed boolean value handling with workflow option semantics.
///     </para>
/// </remarks>
/// <example>
///     <para>Creating a custom toggle workflow option:</para>
///     <code>
/// public sealed record UseCustomFeature : ToggleWorkflowOption&lt;UseCustomFeature&gt;;
/// 
/// // Usage in workflow configuration
/// var enabledOption = UseCustomFeature.Enabled;
/// var disabledOption = UseCustomFeature.Disabled;
/// 
/// // Check if feature is enabled in a collection of options
/// bool isFeatureEnabled = UseCustomFeature.IsEnabled(workflowOptions);
/// </code>
/// </example>
/// <seealso cref="WorkflowOption{TData, TSelf}" />
/// <seealso cref="IWorkflowOption" />
[PublicAPI]
public abstract record ToggleWorkflowOption<TSelf> : WorkflowOption<bool, TSelf>
    where TSelf : WorkflowOption<bool, TSelf>, new()
{
    /// <summary>
    ///     Gets a predefined instance of this toggle option with the value set to <c>true</c>,
    ///     representing the enabled state of the feature.
    /// </summary>
    /// <value>
    ///     A static instance of <typeparamref name="TSelf" /> with <see cref="WorkflowOption{TData, TSelf}.Value" />
    ///     set to <c>true</c>.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         This static field provides a convenient way to reference the enabled state of a toggle option
    ///         without needing to create new instances. It ensures consistency across the application
    ///         when enabling features.
    ///     </para>
    ///     <para>
    ///         This instance can be used directly in workflow configurations, option collections,
    ///         and anywhere an enabled state needs to be specified.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Enable a feature in workflow configuration
    /// var options = new List&lt;IWorkflowOption&gt; { UseAzureKeyVault.Enabled };
    /// 
    /// // Add to existing configuration
    /// workflowDefinition.WithAddedOptions(UseCustomFeature.Enabled);
    /// </code>
    /// </example>
    /// <seealso cref="Disabled" />
    /// <seealso cref="IsEnabled" />
    public static readonly TSelf Enabled = new()
    {
        Value = true,
    };

    /// <summary>
    ///     Gets a predefined instance of this toggle option with the value set to <c>false</c>,
    ///     representing the disabled state of the feature.
    /// </summary>
    /// <value>
    ///     A static instance of <typeparamref name="TSelf" /> with <see cref="WorkflowOption{TData, TSelf}.Value" />
    ///     set to <c>false</c>.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         This static field provides a convenient way to reference the disabled state of a toggle option
    ///         without needing to create new instances. It ensures consistency across the application
    ///         when explicitly disabling features.
    ///     </para>
    ///     <para>
    ///         While the absence of a toggle option typically implies it's disabled, this instance
    ///         can be used to explicitly indicate that a feature should be disabled, which can be
    ///         useful for overriding default behaviors or making intentions clear in configurations.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Explicitly disable a feature in workflow configuration
    /// var options = new List&lt;IWorkflowOption&gt; { UseAzureKeyVault.Disabled };
    /// 
    /// // Override a default enabled state
    /// workflowDefinition.WithAddedOptions(UseCustomFeature.Disabled);
    /// </code>
    /// </example>
    /// <seealso cref="Enabled" />
    /// <seealso cref="IsEnabled" />
    public static readonly TSelf Disabled = new()
    {
        Value = false,
    };

    /// <summary>
    ///     Determines whether this toggle option is enabled within a collection of workflow options.
    /// </summary>
    /// <param name="options">
    ///     The collection of workflow options to search for enabled instances of this toggle option type.
    /// </param>
    /// <returns>
    ///     <c>true</c> if at least one instance of <typeparamref name="TSelf" /> exists in the collection
    ///     with <see cref="WorkflowOption{TData, TSelf}.Value" /> set to <c>true</c>; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         This method provides a convenient way to check if a specific toggle feature is enabled
    ///         within a workflow configuration. It searches through the provided options collection
    ///         for any instances of the current toggle option type that have their value set to <c>true</c>.
    ///     </para>
    ///     <para>
    ///         The method uses LINQ to filter the options by type and then checks if any of them
    ///         have a <c>true</c> value. This approach handles scenarios where multiple instances
    ///         of the same option type might exist (though this is typically not the case for toggle options
    ///         due to their boolean nature and merging behavior).
    ///     </para>
    ///     <para>
    ///         If no instances of the option type are found in the collection, the method returns <c>false</c>,
    ///         indicating the feature is disabled by default.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// var workflowOptions = new List&lt;IWorkflowOption&gt;
    /// {
    ///     UseAzureKeyVault.Enabled,
    ///     UseCustomFeature.Disabled,
    ///     new SomeOtherOption()
    /// };
    /// 
    /// bool azureKeyVaultEnabled = UseAzureKeyVault.IsEnabled(workflowOptions); // Returns true
    /// bool customFeatureEnabled = UseCustomFeature.IsEnabled(workflowOptions); // Returns false
    /// bool undefinedFeatureEnabled = SomeToggleFeature.IsEnabled(workflowOptions); // Returns false
    /// </code>
    /// </example>
    /// <seealso cref="Enabled" />
    /// <seealso cref="Disabled" />
    public static bool IsEnabled(IEnumerable<IWorkflowOption> options) =>
        options
            .OfType<TSelf>()
            .Any(x => x.Value);
}
