import { ResourceGroup } from '@pulumi/azure-native/resources';
import { Config } from '@pulumi/pulumi';
import { assignCustomDomain } from './assignCustomDomain';
import { createFunction } from './createFunction';

export default function createPublicFunction (resourceGroup: ResourceGroup, namePrefix: string, subDomainName: string, protect: boolean) {
    const config = new Config();
    const fullDomainName = `${subDomainName}.${config.require('domain')}`;
    const pubFunc = createFunction(resourceGroup, namePrefix, protect, fullDomainName);
    const domain = assignCustomDomain(resourceGroup, pubFunc.webApp, pubFunc.servicePlan, namePrefix, subDomainName, protect);

    return {
        ...pubFunc,
        ...domain
    };
}
