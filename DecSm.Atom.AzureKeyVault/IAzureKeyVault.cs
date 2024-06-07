namespace DecSm.Atom.AzureKeyVault;

[TargetDefinition]
public partial interface IAzureKeyVault
{
    [ParamDefinition("azure-vault-address", "Address for the Azure Vault")]
    string AzureVaultAddress => GetParam(() => AzureVaultAddress)!;
    
    [ParamDefinition("azure-vault-tenant-id", "Tenant ID for the Azure Vault")]
    string AzureVaultTenantId => GetParam(() => AzureVaultTenantId)!;
    
    [ParamDefinition("azure-vault-app-id", "Azure ID for App Registration that has access to the Azure Vault")]
    string AzureVaultAppId => GetParam(() => AzureVaultAppId)!;
    
    [ParamDefinition("azure-vault-app-secret", "Azure Secret for App Registration that has access to the Azure Vault")]
    string AzureVaultAppSecret => GetParam(() => AzureVaultAppSecret)!;
}