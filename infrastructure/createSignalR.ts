import { interpolate } from '@pulumi/pulumi';
import { ResourceGroup } from '@pulumi/azure-native/resources';
import { SignalR, listSignalRKeysOutput } from '@pulumi/azure-native/signalrservice';

export function createSignalR (resourceGroup: ResourceGroup, namePrefix: string, protect: boolean) {
    const signalr = new SignalR('signalr-' + namePrefix, {
        cors: {
            allowedOrigins: ['*']
        },
        kind: 'SignalR',
        networkACLs: {
            defaultAction: 'Deny',
            publicNetwork: {
                allow: [
                    'ServerConnection',
                    'ClientConnection',
                    'RESTAPI',
                    'Trace'
                ]
            }
        },
        resourceGroupName: resourceGroup.name,
        sku: {
            capacity: 1,
            name: 'Free_F1' // "Standard_S1"
        }
    }, {
        protect
        // parent: resourceGroup
    });

    const connectionString = interpolate`${listSignalRKeysOutput({
        resourceGroupName: resourceGroup.name,
        resourceName: signalr.name
    }).primaryConnectionString}`;

    return {
        signalr,
        connectionString
    };
}
