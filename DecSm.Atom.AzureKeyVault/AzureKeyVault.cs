namespace DecSm.Atom.AzureKeyVault;

public static class AzureKeyVault
{
    public static UseAzureKeyVault UseAzureKeyVault => new();

    public static IAtomConfiguration AddAzureKeyVault(this IAtomConfiguration configuration)
    {
        configuration.Builder.Services.AddSingleton<AzureKeyVaultProvider>();
        configuration.Builder.Services.AddSingleton<IVaultProvider>(x => x.GetRequiredService<AzureKeyVaultProvider>());
        configuration.Builder.Services.AddSingleton<IWorkflowOptionProvider>(x => x.GetRequiredService<AzureKeyVaultProvider>());

        return configuration;
    }
}