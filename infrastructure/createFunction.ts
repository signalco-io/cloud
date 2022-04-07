import { WebApp, AppServicePlan, SupportedTlsVersions } from '@pulumi/azure-native/web';
import { ResourceGroup } from '@pulumi/azure-native/resources';
import { Input, interpolate } from '@pulumi/pulumi';

export function createFunction (resourceGroup: ResourceGroup, namePrefix: string, protect: boolean, domainName?: Input<string>) {
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
                allowedOrigins: domainName
                    ? [
                        'https://localhost:3000',
                        'http://localhost:3000',
                        interpolate`https://${domainName}`
                    ]
                    : ['*'],
                supportCredentials: !!domainName
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
