import { ResourceGroup } from '@pulumi/azure-native/resources';
import createPublicFunction from './createPublicFunction';
import { assignFunctionCode } from './assignFunctionCode';
import apiStatusCheck from './apiStatusCheck';

export default function createChannelFunction (channelName: string, resourceGroup: ResourceGroup, shouldProtect: boolean) {
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
    apiStatusCheck(publicFunctionPrefix, `Channel - ${channelName}`, channelFunc.dnsCname.hostname, 15);

    return {
        name: channelName,
        ...channelFunc,
        ...channelFuncCode
    };
}
