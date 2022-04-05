import { Input } from '@pulumi/pulumi';
import { ResourceGroup } from '@pulumi/azure-native/resources';
import { assignCustomDomain } from './assignCustomDomain';
import { createFunction } from './createFunction';

export default function createPublicFunction (resourceGroup: ResourceGroup, namePrefix: string, codePath: string, subDomainName: string, protect: boolean, appSettings: Input<{[key: string]: Input<string>}> = {}) {
    const pubFunc = createFunction(resourceGroup, namePrefix, codePath, protect, appSettings);
    const domain = assignCustomDomain(resourceGroup, pubFunc.webApp, pubFunc.servicePlan, namePrefix, subDomainName, protect);

    return {
        ...pubFunc,
        ...domain
    };
}
