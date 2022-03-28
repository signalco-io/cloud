import * as pulumi from "@pulumi/pulumi";
import * as resources from "@pulumi/azure-native/resources";
import { createFunction } from "./createFunction";
import { createSignalR } from "./createSignalR";

// pulumi config set azure-native:clientId <clientID> 
// pulumi config set azure-native:clientSecret <clientSecret> --secret 
// pulumi config set azure-native:tenantId <tenantID> 
// pulumi config set azure-native:subscriptionId <subscriptionId>
// pulumi stack select staging

let stack = pulumi.getStack();

const shouldProtect = stack === 'production';
const resourceGroup = new resources.ResourceGroup("signalco-public-" + stack);
createSignalR(resourceGroup.name, 'sr', shouldProtect);
createFunction(resourceGroup.name, 'cpub', shouldProtect);
createFunction(resourceGroup.name, 'cint', shouldProtect);
