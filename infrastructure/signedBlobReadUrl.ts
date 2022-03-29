import * as resources from "@pulumi/azure-native/resources";
import * as storage from "@pulumi/azure-native/storage";
import * as pulumi from "@pulumi/pulumi";

export function signedBlobReadUrl(blob: storage.Blob,
    container: storage.BlobContainer,
    account: storage.StorageAccount,
    resourceGroup: resources.ResourceGroup): pulumi.Output<string> {
    const sasStartDate = new Date();
    sasStartDate.setUTCDate(sasStartDate.getUTCDate() - 1); // Start from yesterday
    const sasStartDateString = `${sasStartDate.getUTCFullYear()}-${sasStartDate.getUTCMonth().toString().padStart(2, '0')}-${sasStartDate.getUTCDate().toString().padStart(2, '0')}`;
    const sasEndDateString = `${sasStartDate.getUTCFullYear() + 5}-01-01`;

    const blobSAS = storage.listStorageAccountServiceSASOutput({
        accountName: account.name,
        protocols: storage.HttpProtocol.Https,
        sharedAccessExpiryTime: sasEndDateString,
        sharedAccessStartTime: sasStartDateString,
        resourceGroupName: resourceGroup.name,
        resource: storage.SignedResource.C,
        permissions: storage.Permissions.R,
        canonicalizedResource: pulumi.interpolate`/blob/${account.name}/${container.name}`,
        contentType: "application/json",
        cacheControl: "max-age=5",
        contentDisposition: "inline",
        contentEncoding: "deflate",
    });
    return pulumi.interpolate`https://${account.name}.blob.core.windows.net/${container.name}/${blob.name}?${blobSAS.serviceSasToken}`;
}
