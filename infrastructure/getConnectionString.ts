import * as pulumi from "@pulumi/pulumi";
import * as storage from "@pulumi/azure-native/storage";


export function getConnectionString(resourceGroupName: pulumi.Input<string>, accountName: pulumi.Input<string>): pulumi.Output<string> {
    // Retrieve the primary storage account key.
    const storageAccountKeys = storage.listStorageAccountKeysOutput({ resourceGroupName, accountName });
    const primaryStorageKey = storageAccountKeys.keys[0].value;

    // Build the connection string to the storage account.
    return pulumi.interpolate`DefaultEndpointsProtocol=https;AccountName=${accountName};AccountKey=${primaryStorageKey}`;
}
