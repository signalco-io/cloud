import { Resource } from '@pulumi/pulumi';
import { StorageAccount, SkuName, Kind, MinimumTlsVersion } from '@pulumi/azure-native/storage';
import { ResourceGroup } from '@pulumi/azure-native/resources';

import { getConnectionString } from './getConnectionString';

export function createStorageAccount (resourceGroup: ResourceGroup, namePrefix: string, protect: boolean, parent?: Resource, sku?: SkuName) {
    const storageAccount = new StorageAccount(`sa${namePrefix}`, {
        resourceGroupName: resourceGroup.name,
        enableHttpsTrafficOnly: true,
        minimumTlsVersion: MinimumTlsVersion.TLS1_2,
        sku: {
            name: sku || SkuName.Standard_LRS
        },
        kind: Kind.StorageV2
    }, {
        protect: protect
        // parent: parent ?? resourceGroup
    });

    const connectionString = getConnectionString(resourceGroup, storageAccount.name);

    return {
        storageAccount,
        connectionString
    };
}
