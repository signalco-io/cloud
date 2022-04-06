import { WebApp, AppServicePlan, SupportedTlsVersions } from '@pulumi/azure-native/web';
import { ResourceGroup } from '@pulumi/azure-native/resources';

export function createFunction (resourceGroup: ResourceGroup, namePrefix: string, protect: boolean) {
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

    return {
        webApp: app,
        servicePlan: plan
    };
}
