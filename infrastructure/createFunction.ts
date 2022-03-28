import * as pulumi from "@pulumi/pulumi";
import * as web from "@pulumi/azure-native/web";
import * as resources from "@pulumi/azure-native/resources";
import * as storage from "@pulumi/azure-native/storage";
import { createStorageAccount } from "./createStorageAccount";
import { getConnectionString } from "./getConnectionString";
import { signedBlobReadUrl } from "./signedBlobReadUrl";

export function createFunction(resourceGroup: resources.ResourceGroup, namePrefix: string, codePath: string, protect: boolean) {
    const storageAccount = createStorageAccount(resourceGroup, namePrefix, protect);
    const storageConnectionString = getConnectionString(resourceGroup, storageAccount.name);

    // Function code archives will be stored in this container.
    const codeContainer = new storage.BlobContainer(`zips${namePrefix}`, {
        containerName: "zips",
        resourceGroupName: resourceGroup.name,
        accountName: storageAccount.name,
    });

    // Upload Azure Function's code as a zip archive to the storage account.
    const codeBlob = new storage.Blob(`zip${namePrefix}`, {
        blobName: "zip",
        resourceGroupName: resourceGroup.name,
        accountName: storageAccount.name,
        containerName: codeContainer.name,
        source: new pulumi.asset.FileArchive(codePath),
    });

    const plan = new web.AppServicePlan(`appplan${namePrefix}`, {
        resourceGroupName: resourceGroup.name,
        sku: {
            name: "Y1",
            tier: "Dynamic",
        },
    }, {
        protect: protect
    });

    const codeBlobUrl = signedBlobReadUrl(codeBlob, codeContainer, storageAccount, resourceGroup);

    return new web.WebApp(`func${namePrefix}`, {
        resourceGroupName: resourceGroup.name,
        serverFarmId: plan.id,
        kind: "functionapp",
        containerSize: 1536,
        httpsOnly: true,
        siteConfig: {
            appSettings: [
                { name: "AzureWebJobsStorage", value: storageConnectionString },
                { name: "FUNCTIONS_EXTENSION_VERSION", value: "~4" },
                { name: "FUNCTIONS_WORKER_RUNTIME", value: "dotnet" },
                { name: "WEBSITE_RUN_FROM_PACKAGE", value: codeBlobUrl },
                { name: "OpenApi__DocTitle", value: "Signalco Cloud API" },
                { name: "OpenApi__Version", value: "v3" },
            ],
            http20Enabled: true,
            functionAppScaleLimit: 200,
            cors: {
                allowedOrigins: ["*"],
            },
        },
    }, {
        protect: protect
    });
}
