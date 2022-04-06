import { ResourceGroup } from '@pulumi/azure-native/resources';
import { assignCustomDomain } from './assignCustomDomain';
import { createFunction } from './createFunction';

export default function createPublicFunction (resourceGroup: ResourceGroup, namePrefix: string, subDomainName: string, protect: boolean) {
    const pubFunc = createFunction(resourceGroup, namePrefix, protect);
    const domain = assignCustomDomain(resourceGroup, pubFunc.webApp, pubFunc.servicePlan, namePrefix, subDomainName, protect);

    return {
        ...pubFunc,
        ...domain
    };
}
