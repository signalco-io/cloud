import * as storage from "@pulumi/azure-native/storage";
import * as resources from "@pulumi/azure-native/resources";
import { MinimumTlsVersion } from "@pulumi/azure-native/storage";
import { getConnectionString } from "./getConnectionString";

export function createStorageAccount(resourceGroup: resources.ResourceGroup, namePrefix: string, protect: boolean) {
    return new storage.StorageAccount(`sa${namePrefix}`, {
        resourceGroupName: resourceGroup.name,
        enableHttpsTrafficOnly: true,
        minimumTlsVersion: MinimumTlsVersion.TLS1_2,
        sku: {
            name: storage.SkuName.Standard_LRS,
        },
        kind: storage.Kind.StorageV2,
    }, {
        protect: protect
    });
}
