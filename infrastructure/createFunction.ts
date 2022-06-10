import { WebApp, AppServicePlan, SupportedTlsVersions } from '@pulumi/azure-native/web';
import { ResourceGroup } from '@pulumi/azure-native/resources';
import { Input, interpolate } from '@pulumi/pulumi';

export function createFunction (resourceGroup: ResourceGroup, namePrefix: string, protect: boolean, cors?: Input<string>[]) {
    const plan = new AppServicePlan(`func-appplan-${namePrefix}`, {
        resourceGroupName: resourceGroup.name,
        sku: {
            name: 'Y1',
            tier: 'Dynamic'
        }
    }, {
        protect
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
                allowedOrigins: cors
                    ? [
                        'https://localhost:3000', // Next.js
                        'http://localhost:3000', // Next.js
                        'https://localhost:6006', // Storybook
                        'http://localhost:6006', // Storybook
                        ...cors.map(c => interpolate`https://${c}`)
                    ]
                    : ['*'],
                supportCredentials: !!cors
            }
        }
    }, {
        protect
        // parent: plan
    });

    return {
        webApp: app,
        servicePlan: plan
    };
}
