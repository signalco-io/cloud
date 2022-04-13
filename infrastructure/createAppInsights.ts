import { ResourceGroup } from '@pulumi/azure-native/resources';
import { Component, ApplicationType, ComponentArgs } from '@pulumi/azure-native/insights';

export default function createAppInsights (resourceGroup: ResourceGroup, namePrefix: string, nameSuffix: string, options?: Partial<ComponentArgs>) {
    const component = new Component(`${namePrefix}-ai-${nameSuffix}`, {
        kind: 'web',
        resourceGroupName: resourceGroup.name,
        applicationType: ApplicationType.Web,
        samplingPercentage: 100,
        ...options
    });

    return {
        component
    };
}
