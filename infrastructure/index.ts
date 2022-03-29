import * as pulumi from "@pulumi/pulumi";
import * as resources from "@pulumi/azure-native/resources";
import * as cloudflare from "@pulumi/cloudflare";
import { createFunction } from "./createFunction";
import { createSignalR } from "./createSignalR";
import { createStorageAccount } from "./createStorageAccount";
import { createKeyVault } from "./createKeyVault";

// pulumi config set azure-native:clientId <clientID> 
// pulumi config set azure-native:clientSecret <clientSecret> --secret 
// pulumi config set azure-native:tenantId <tenantID> 
// pulumi config set azure-native:subscriptionId <subscriptionId>
// pulumi stack select staging

let stack = pulumi.getStack();

const shouldProtect = stack === 'production';
const resourceGroup = new resources.ResourceGroup("signalco-public-" + stack);

createSignalR(resourceGroup, 'sr', shouldProtect);

// TODO: Pass SignalR connection string
const isInitial = false;
const pubFunc = createFunction(resourceGroup, 'cpub', "next-api.signalco.io", "../Signal.Api.Public/bin/Release/net6.0/publish/", isInitial, shouldProtect);
createFunction(resourceGroup, 'cint', undefined, "../Signal.Api.Internal/bin/Release/net6.0/publish/", isInitial, shouldProtect);

// TODO: Create with more redundancy than function storage account
createStorageAccount(resourceGroup, 'store', shouldProtect);

// TODO: Assign Functions to KeyVault, enable access to secrets
createKeyVault(resourceGroup, "", shouldProtect);

// TODO: Link cloudflare DNS to created APIs
new cloudflare.Record("dns-cname-cpub", {
    name: "next-api",
    zoneId: "1f5a35e22cb52dfb6f087934cf2141a5",
    type: "CNAME",
    value: pubFunc.hostNames[0],
    ttl: 1
});