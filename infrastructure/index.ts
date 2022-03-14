import * as pulumi from "@pulumi/pulumi";
import * as resources from "@pulumi/azure-native/resources";
import * as storage from "@pulumi/azure-native/storage";
import * as web from "@pulumi/azure-native/web";

// Resource group import: 
// pulumi import azure-native:resources:ResourceGroup signal-api /subscriptions/753dc089-5d12-4b87-a9ac-3f58729433b1/resourceGroups/signal-api
const signal_api = new resources.ResourceGroup("signal-api", {
    location: "westeurope",
    resourceGroupName: "signal-api",
}, {
    protect: true,
});

// Storage Account storageaccountsigna885d
//pulumi import azure-native:storage:StorageAccount storageaccountsigna885d /subscriptions/753dc089-5d12-4b87-a9ac-3f58729433b1/resourceGroups/signal-api/providers/Microsoft.Storage/storageAccounts/storageaccountsigna885d
const storageaccountsigna885d = new storage.StorageAccount("storageaccountsigna885d", {
    accountName: "storageaccountsigna885d",
    enableHttpsTrafficOnly: true,
    encryption: {
        keySource: "Microsoft.Storage",
        services: {
            blob: {
                enabled: true,
                keyType: "Account",
            },
            file: {
                enabled: true,
                keyType: "Account",
            },
        },
    },
    kind: "Storage",
    location: "westeurope",
    minimumTlsVersion: "TLS1_2",
    networkRuleSet: {
        bypass: "AzureServices",
        defaultAction: "Allow",
    },
    resourceGroupName: signal_api.name,
    sku: {
        name: "Standard_LRS",
    },
}, {
    protect: true,
});

// Storage Account storageaccountssignabf04
//pulumi import azure-native:storage:StorageAccount storageaccountsignabf04 /subscriptions/753dc089-5d12-4b87-a9ac-3f58729433b1/resourceGroups/signal-api/providers/Microsoft.Storage/storageAccounts/storageaccountsignabf04
const storageaccountsignabf04 = new storage.StorageAccount("storageaccountsignabf04", {
    accountName: "storageaccountsignabf04",
    enableHttpsTrafficOnly: true,
    encryption: {
        keySource: "Microsoft.Storage",
        services: {
            blob: {
                enabled: true,
                keyType: "Account",
            },
            file: {
                enabled: true,
                keyType: "Account",
            },
        },
    },
    kind: "Storage",
    location: "westeurope",
    minimumTlsVersion: "TLS1_2",
    networkRuleSet: {
        bypass: "AzureServices",
        defaultAction: "Allow",
    },
    resourceGroupName: signal_api.name,
    sku: {
        name: "Standard_LRS",
    },
}, {
    protect: true,
});

// App service plan ASP-signalapi-a08f
// pulumi import azure-native:web:AppServicePlan ASP-signalapi-a08f /subscriptions/753dc089-5d12-4b87-a9ac-3f58729433b1/resourceGroups/signal-api/providers/Microsoft.Web/serverfarms/ASP-signalapi-a08f
const ASP_signalapi_a08f = new web.AppServicePlan("ASP-signalapi-a08f", {
    hyperV: false,
    isSpot: false,
    isXenon: false,
    kind: "functionapp",
    location: "West Europe",
    maximumElasticWorkerCount: 1,
    name: "ASP-signalapi-a08f",
    perSiteScaling: false,
    reserved: false,
    resourceGroupName: signal_api.name,
    sku: {
        capacity: 0,
        family: "Y",
        name: "Y1",
        size: "Y1",
        tier: "Dynamic",
    },
    targetWorkerCount: 0,
    targetWorkerSizeId: 0,
}, {
    protect: true,
});

// Azure Function Cloud Primary
// pulumi import azure-native:web:WebApp signalco-cloud-primary /subscriptions/753dc089-5d12-4b87-a9ac-3f58729433b1/resourceGroups/signal-api/providers/Microsoft.Web/sites/signalco-cloud-primary
const signalco_cloud_primary = new web.WebApp("signalco-cloud-primary", {
    clientAffinityEnabled: false,
    clientCertEnabled: false,
    clientCertMode: "Required",
    containerSize: 1536,
    customDomainVerificationId: "EA74BB41447D78B80D9D1A1E7DEEA7D8D5132947774E11F2EC55D60EEF48E953",
    dailyMemoryTimeQuota: 500000,
    enabled: true,
    hostNameSslStates: [
        {
            hostType: "Standard",
            name: "api.signalco.io",
            sslState: "SniEnabled",
            thumbprint: "4E918989DB00B877FD65A0796EDDAEE566859CAC",
        },
        {
            hostType: "Standard",
            name: "signalco-cloud-primary.azurewebsites.net",
            sslState: "Disabled",
        },
        {
            hostType: "Repository",
            name: "signalco-cloud-primary.scm.azurewebsites.net",
            sslState: "Disabled",
        },
    ],
    hostNamesDisabled: false,
    httpsOnly: true,
    hyperV: false,
    identity: {
        type: "SystemAssigned",
    },
    isXenon: false,
    keyVaultReferenceIdentity: "SystemAssigned",
    kind: "functionapp",
    location: "West Europe",
    name: "signalco-cloud-primary",
    redundancyMode: "None",
    reserved: false,
    resourceGroupName: signal_api.name,
    scmSiteAlsoStopped: false,
    serverFarmId: ASP_signalapi_a08f.id,
    siteConfig: {
        acrUseManagedIdentityCreds: false,
        alwaysOn: false,
        functionAppScaleLimit: 200,
        http20Enabled: true,
        linuxFxVersion: "",
        minimumElasticInstanceCount: 1,
        numberOfWorkers: 1,
    },
    storageAccountRequired: false,
}, {
    protect: true,
});

// App service plan ASP-signalapi-a9f0
// pulumi import azure-native:web:AppServicePlan ASP-signalapi-a9f0 /subscriptions/753dc089-5d12-4b87-a9ac-3f58729433b1/resourceGroups/signal-api/providers/Microsoft.Web/serverfarms/ASP-signalapi-a9f0
const ASP_signalapi_a9f0 = new web.AppServicePlan("ASP-signalapi-a9f0", {
    hyperV: false,
    isSpot: false,
    isXenon: false,
    kind: "functionapp",
    location: "West Europe",
    maximumElasticWorkerCount: 1,
    name: "ASP-signalapi-a9f0",
    perSiteScaling: false,
    reserved: false,
    resourceGroupName: signal_api.name,
    sku: {
        capacity: 0,
        family: "Y",
        name: "Y1",
        size: "Y1",
        tier: "Dynamic",
    },
    targetWorkerCount: 0,
    targetWorkerSizeId: 0,
}, {
    protect: true,
});

// Azure Function Cloud Internal
// pulumi import azure-native:web:WebApp signalco-cloud-internal /subscriptions/753dc089-5d12-4b87-a9ac-3f58729433b1/resourceGroups/signal-api/providers/Microsoft.Web/sites/signalco-cloud-internal
const signalco_cloud_internal = new web.WebApp("signalco-cloud-internal", {
    clientAffinityEnabled: false,
    clientCertEnabled: false,
    clientCertMode: "Required",
    containerSize: 1536,
    customDomainVerificationId: "EA74BB41447D78B80D9D1A1E7DEEA7D8D5132947774E11F2EC55D60EEF48E953",
    dailyMemoryTimeQuota: 500000,
    enabled: true,
    hostNameSslStates: [
        {
            hostType: "Standard",
            name: "signalco-cloud-internal.azurewebsites.net",
            sslState: "Disabled",
        },
        {
            hostType: "Repository",
            name: "signalco-cloud-internal.scm.azurewebsites.net",
            sslState: "Disabled",
        },
    ],
    hostNamesDisabled: false,
    httpsOnly: false,
    hyperV: false,
    identity: {
        type: "SystemAssigned",
    },
    isXenon: false,
    keyVaultReferenceIdentity: "SystemAssigned",
    kind: "functionapp",
    location: "West Europe",
    name: "signalco-cloud-internal",
    redundancyMode: "None",
    reserved: false,
    resourceGroupName: signal_api.name,
    scmSiteAlsoStopped: false,
    serverFarmId: ASP_signalapi_a9f0.id,
    siteConfig: {
        acrUseManagedIdentityCreds: false,
        alwaysOn: false,
        functionAppScaleLimit: 200,
        http20Enabled: true,
        linuxFxVersion: "",
        minimumElasticInstanceCount: 1,
        numberOfWorkers: 1,
    },
    storageAccountRequired: false,
}, {
    protect: true,
});