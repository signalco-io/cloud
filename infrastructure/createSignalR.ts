import * as resources from "@pulumi/azure-native/resources";
import * as signalrservice from "@pulumi/azure-native/signalrservice";

export function createSignalR(resourceGroup: resources.ResourceGroup, namePrefix: string, protect: boolean) {
    return new signalrservice.SignalR("signalhub", {
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
        resourceGroupName: resourceGroup.name,
        resourceName: "signalhub",
        sku: {
            capacity: 1,
            name: "Free_F1", //"Standard_S1"
        },
    }, {
        protect: protect
    });
}
