import { Config, getStack } from "@pulumi/pulumi";
import { ResourceGroup } from "@pulumi/azure-native/resources";
import { createFunction } from "./createFunction";
import { createSignalR } from "./createSignalR";
import { createStorageAccount } from "./createStorageAccount";
import { createKeyVault } from "./createKeyVault";
import createPublicFunction from "./createPublicFunction";
import { webAppIdentity } from "./webAppIdentity";
import vaultSecret from "./vaultSecret";

/*
 * NOTE: `parent` configuration is currently disabled for all resources because
 *       there is memory issued when enabled.
*/

// TODO: prepare for CI/CD
// TODO: Assign keyvault connection string to Functions

// pulumi config set azure-native:clientId <clientID> 
// pulumi config set azure-native:clientSecret <clientSecret> --secret 
// pulumi config set azure-native:tenantId <tenantID> 
// pulumi config set azure-native:subscriptionId <subscriptionId>
// pulumi stack select next

const config = new Config();
const stack = getStack();
const shouldProtect = stack === 'production';

const resourceGroupName = `signalco-cloud-${stack}`
const publicFunctionPrefix = `cpub`;
const publicFunctionSubDomain = stack === 'production' ? 'api' : `${stack}-api`;
const internalFunctionPrefix = `cint`;
const signalrPrefix = `sr`
const storagePrefix = `store`;
const keyvaultPrefix = `kv`;

const resourceGroup = new ResourceGroup(resourceGroupName);

const signalr = createSignalR(resourceGroup, signalrPrefix, shouldProtect);

// Create Public function
const pubFunc = createPublicFunction(
    resourceGroup,
    publicFunctionPrefix,
    "../Signal.Api.Public/bin/Release/net6.0/publish/",
    publicFunctionSubDomain,
    shouldProtect,
    {
        "AzureSignalRConnectionString": signalr.connectionString
    }
);

// Create Internal function
const intFunc = createFunction(
    resourceGroup, 
    internalFunctionPrefix, 
    "../Signal.Api.Internal/bin/Release/net6.0/publish/", 
    shouldProtect
);

// Create general storage
// TODO: Create with more redundancy than function storage account
const storage = createStorageAccount(resourceGroup, storagePrefix, shouldProtect);

// Create and populate vault
const vault = createKeyVault(resourceGroup, keyvaultPrefix, shouldProtect, [
    webAppIdentity(pubFunc.webApp),
    webAppIdentity(intFunc.webApp)
]);
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'Auth0--ApiIdentifier', config.requireSecret('secret-auth0ApiIdentifier'));
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'Auth0--ClientId--Station', config.requireSecret('secret-auth0ClientIdStation'));
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'Auth0--ClientSecret--Station', config.requireSecret('secret-auth0ClientSecretStation'));
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'Auth0--Domain', config.require('secret-auth0Domain'));
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'HCaptcha--Secret', config.requireSecret('secret-hcaptchaSecret'));
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'HCaptcha--SiteKey', config.requireSecret('secret-hcaptchaSiteKey'));
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'SignalStorageAccountConnectionString', storage.connectionString);

export const signalRUrl = signalr.signalr.hostName;
export const internalApiUrl = intFunc.webApp.hostNames[0];
export const publicApiUrl = pubFunc.dnsCname.hostname;