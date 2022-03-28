import * as pulumi from "@pulumi/pulumi";
import * as storage from "@pulumi/azure-native/storage";
import * as web from "@pulumi/azure-native/web";
import { MinimumTlsVersion } from "@pulumi/azure-native/storage";
import { getConnectionString } from "./getConnectionString";

export function createFunction(resourceGroupName: pulumi.Output<string>, namePrefix: string, protect: boolean) {
    const storageAccount = new storage.StorageAccount(`sa${namePrefix}`, {
        resourceGroupName: resourceGroupName,
        enableHttpsTrafficOnly: true,
        minimumTlsVersion: MinimumTlsVersion.TLS1_2,
        sku: {
            name: storage.SkuName.Standard_LRS,
        },
        kind: storage.Kind.StorageV2,
    }, {
        protect: protect
    });
    const storageConnectionString = getConnectionString(resourceGroupName, storageAccount.name);
    const plan = new web.AppServicePlan(`appplan${namePrefix}`, {
        resourceGroupName: resourceGroupName,
        sku: {
            name: "Y1",
            tier: "Dynamic",
        },
    }, {
        protect: protect
    });
    const app = new web.WebApp(`func${namePrefix}`, {
        resourceGroupName: resourceGroupName,
        serverFarmId: plan.id,
        kind: "functionapp",
        containerSize: 1536,
        httpsOnly: true,
        siteConfig: {
            appSettings: [
                { name: "AzureWebJobsStorage", value: storageConnectionString },
                { name: "FUNCTIONS_EXTENSION_VERSION", value: "~4" },
                { name: "FUNCTIONS_WORKER_RUNTIME", value: "dotnet" },
                // { name: "WEBSITE_RUN_FROM_PACKAGE", value: codeBlobUrl },
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
