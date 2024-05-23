namespace DecSm.Atom.AzureKeyVault;

public static class AzureKeyVault
{
    public static IAtomConfigurator AddAzureKeyVault(this IAtomConfigurator configurator)
    {
        configurator.Builder.Services.AddSingleton<AzureKeyVaultProvider>();
        configurator.Builder.Services.AddSingleton<IVaultProvider>(x => x.GetRequiredService<AzureKeyVaultProvider>());
        configurator.Builder.Services.AddSingleton<IWorkflowOptionProvider>(x => x.GetRequiredService<AzureKeyVaultProvider>());
        
        return configurator;
    }
    
    public static UseAzureKeyVault UseAzureKeyVault => new();
}