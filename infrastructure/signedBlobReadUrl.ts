import * as resources from "@pulumi/azure-native/resources";
import * as storage from "@pulumi/azure-native/storage";
import * as pulumi from "@pulumi/pulumi";

export function signedBlobReadUrl(blob: storage.Blob,
    container: storage.BlobContainer,
    account: storage.StorageAccount,
    resourceGroup: resources.ResourceGroup): pulumi.Output<string> {
    const sasStartDate = new Date();

    const blobSAS = storage.listStorageAccountServiceSASOutput({
        accountName: account.name,
        protocols: storage.HttpProtocol.Https,
        sharedAccessExpiryTime: `${sasStartDate.getUTCFullYear() + 5}-01-01`,
        sharedAccessStartTime: `${sasStartDate.getUTCFullYear()}-${sasStartDate.getUTCMonth()}-${sasStartDate.getUTCDate()}`,
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
