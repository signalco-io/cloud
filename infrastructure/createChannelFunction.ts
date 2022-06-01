import { ResourceGroup } from '@pulumi/azure-native/resources';
import createPublicFunction from './createPublicFunction';
import { assignFunctionCode } from './assignFunctionCode';
import * as checkly from '@checkly/pulumi';
import { getStack, interpolate } from '@pulumi/pulumi';

export default function createChannelFunction (channelName: string, resourceGroup: ResourceGroup, shouldProtect: boolean) {
    const stack = getStack();
    const publicFunctionPrefix = 'channel' + channelName;
    const publicFunctionSubDomain = channelName + '.channel.api';
    const corsDomains = undefined;

    const channelFunc = createPublicFunction(
        resourceGroup,
        publicFunctionPrefix,
        publicFunctionSubDomain,
        corsDomains,
        shouldProtect
    );
    const channelFuncCode = assignFunctionCode(
        resourceGroup,
        channelFunc.webApp,
        publicFunctionPrefix,
        '../Signalco.Channel.Slack/bin/Release/net6.0/publish/',
        shouldProtect);
    new checkly.Check(`func-apicheck-${publicFunctionPrefix}`, {
        name: 'Channel - Slack',
        activated: true,
        frequency: 15,
        type: 'API',
        locations: ['eu-west-1'],
        tags: [stack === 'production' ? 'public' : 'dev', 'channel'],
        request: {
            method: 'GET',
            url: interpolate`https://${channelFunc.dnsCname.hostname}/api/status`
        }
    });

    return {
        ...channelFunc,
        ...channelFuncCode
    };
}
