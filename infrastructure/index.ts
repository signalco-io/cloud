import * as pulumi from "@pulumi/pulumi";
import * as resources from "@pulumi/azure-native/resources";
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
createFunction(resourceGroup, 'cpub', "../Signal.Api.Public/bin/Debug/net6.0/publish/", shouldProtect);
createFunction(resourceGroup, 'cint', "../Signal.Api.Internal/bin/Debug/net6.0/publish/", shouldProtect);

// TODO: Create with more redundancy than function storage account
createStorageAccount(resourceGroup, 'store', shouldProtect);

// TODO: Assign Functions to KeyVault, enable access to secrets
createKeyVault(resourceGroup, "", shouldProtect);

// TODO: Link cloudflare DNS to created APIs
