import { Input, asset } from '@pulumi/pulumi';
import { WebApp, AppServicePlan, SupportedTlsVersions, WebAppApplicationSettings } from '@pulumi/azure-native/web';
import { ResourceGroup } from '@pulumi/azure-native/resources';
import { Blob, BlobContainer } from '@pulumi/azure-native/storage';
import { createStorageAccount } from './createStorageAccount';
import { signedBlobReadUrl } from './signedBlobReadUrl';

export function createFunction (resourceGroup: ResourceGroup, namePrefix: string, codePath: string, protect: boolean, appSettings: Input<{[key: string]: Input<string>}> = {}) {
    const plan = new AppServicePlan(`func-appplan-${namePrefix}`, {
        resourceGroupName: resourceGroup.name,
        sku: {
            name: 'Y1',
            tier: 'Dynamic'
        }
    }, {
        protect: protect
        // parent: resourceGroup
    });

    // TODO: Configure AppSettings via another resoure: https://www.pulumi.com/registry/packages/azure-native/api-docs/web/webappapplicationsettings/
    const app = new WebApp(`func-${namePrefix}`, {
        resourceGroupName: resourceGroup.name,
        serverFarmId: plan.id,
        kind: 'functionapp',
        containerSize: 1536,
        dailyMemoryTimeQuota: 500000,
        httpsOnly: true,
        identity: {
            type: 'SystemAssigned'
        },
        keyVaultReferenceIdentity: 'SystemAssigned',
        siteConfig: {
            http20Enabled: true,
            minTlsVersion: SupportedTlsVersions.SupportedTlsVersions_1_2,
            functionAppScaleLimit: 200,
            cors: {
                allowedOrigins: ['*']
            }
        }
    }, {
        protect: protect
        // parent: plan
    });

    const account = createStorageAccount(resourceGroup, namePrefix, protect, app);
    const { storageAccount, connectionString } = account;

    // Function code archives will be stored in this container.
    const codeContainer = new BlobContainer(`func-zips-${namePrefix}`, {
        containerName: 'zips',
        resourceGroupName: resourceGroup.name,
        accountName: storageAccount.name
    }, {
        // parent: app
    });

    // Upload Azure Function's code as a zip archive to the storage account.
    const codeBlob = new Blob(`func-zip-${namePrefix}`, {
        blobName: 'zip',
        resourceGroupName: resourceGroup.name,
        accountName: storageAccount.name,
        containerName: codeContainer.name,
        source: new asset.FileArchive(codePath)
    }, {
        // parent: app
    });
    const codeBlobUrl = signedBlobReadUrl(codeBlob, codeContainer, storageAccount, resourceGroup);

    const settings = new WebAppApplicationSettings(`func-appsettings-${namePrefix}`, {
        name: app.name,
        resourceGroupName: resourceGroup.name,
        properties: {
            AzureWebJobsStorage: connectionString,
            WEBSITE_RUN_FROM_PACKAGE: codeBlobUrl,
            FUNCTIONS_EXTENSION_VERSION: '~4',
            FUNCTIONS_WORKER_RUNTIME: 'dotnet',
            OpenApi__DocTitle: 'Signalco Cloud API',
            OpenApi__Version: 'v3',
            ...appSettings
        }
    }, {
        protect: protect
        // parent: app
    });

    return {
        webApp: app,
        appSettings: settings,
        servicePlan: plan,
        storageAccount: storageAccount,
        codeContainer: codeContainer,
        codeBlob: codeBlob
    };
}
