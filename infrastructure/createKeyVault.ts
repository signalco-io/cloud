import * as resources from "@pulumi/azure-native/resources";
import * as azure from "@pulumi/azure";

export function createKeyVault(resourceGroup: resources.ResourceGroup, namePrefix: string, protect: boolean) {
    const current = azure.core.getClientConfig({});
    return new azure.keyvault.KeyVault(`vault-${namePrefix}`, {
        resourceGroupName: resourceGroup.name,
        tenantId: current.then(current => current.tenantId),
        skuName: "standard",
        softDeleteRetentionDays: 7,
        enabledForDiskEncryption: true,
        accessPolicies: [{
            tenantId: current.then(current => current.tenantId),
            objectId: current.then(current => current.objectId),
            keyPermissions: ["Get"],
            secretPermissions: ["Get"],
            storagePermissions: ["Get"],
        }],
    }, {
        protect: protect
    });
}
