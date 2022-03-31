import { ResourceGroup } from "@pulumi/azure-native/resources";
import { Vault, Secret } from "@pulumi/azure-native/keyvault";
import { Input, interpolate } from "@pulumi/pulumi";

export default function vaultSecret(resourceGroup: ResourceGroup, vault: Vault, namePrefix: string, name: string, value: Input<string>) {
    var secret = new Secret(`secret-${namePrefix}-${name}`, {
        resourceGroupName: resourceGroup.name,
        vaultName: vault.name,
        secretName: name,
        properties: {
            value: value
        }
    });

    return {
        secret: secret
    };
}
