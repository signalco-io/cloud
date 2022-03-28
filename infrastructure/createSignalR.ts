import * as pulumi from "@pulumi/pulumi";
import * as signalrservice from "@pulumi/azure-native/signalrservice";

export function createSignalR(resourceGroupName: pulumi.Output<string>, namePrefix: string, protect: boolean) {
    const signalhub = new signalrservice.SignalR("signalhub", {
        cors: {
            allowedOrigins: ["*"],
        },
        kind: "SignalR",
        networkACLs: {
            defaultAction: "Deny",
            publicNetwork: {
                allow: [
                    "ServerConnection",
                    "ClientConnection",
                    "RESTAPI",
                    "Trace",
                ],
            },
        },
        resourceGroupName: resourceGroupName,
        resourceName: "signalhub",
        sku: {
            capacity: 1,
            name: "Standard_S1",
            tier: "Standard",
        },
    }, {
        protect: protect
    });
}
