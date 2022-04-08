import { Config, getStack, interpolate } from '@pulumi/pulumi';
import { ResourceGroup } from '@pulumi/azure-native/resources';
import { createFunction } from './createFunction';
import { createSignalR } from './createSignalR';
import { createStorageAccount } from './createStorageAccount';
import { createKeyVault } from './createKeyVault';
import createPublicFunction from './createPublicFunction';
import { webAppIdentity } from './webAppIdentity';
import vaultSecret from './vaultSecret';
import { Table } from '@pulumi/azure-native/storage';
import { assignFunctionCode } from './assignFunctionCode';
import { assignFunctionSettings } from './assignFunctionSettings';

/*
 * NOTE: `parent` configuration is currently disabled for all resources because
 *       there is memory issued when enabled.
*/

const config = new Config();
const stack = getStack();
const shouldProtect = stack === 'production';
const domainName = `${config.require('domain')}`;

const resourceGroupName = `signalco-cloud-${stack}`;
const publicFunctionPrefix = 'cpub';
const publicFunctionSubDomain = stack === 'production' ? 'api' : `${stack}-api`;
const internalFunctionPrefix = 'cint';
const signalrPrefix = 'sr';
const storagePrefix = 'store';
const keyvaultPrefix = 'kv';

const resourceGroup = new ResourceGroup(resourceGroupName);

const signalr = createSignalR(resourceGroup, signalrPrefix, shouldProtect);

// Create Public function
const corsDomains = [`www.${domainName}`, domainName];
const pubFunc = createPublicFunction(
    resourceGroup,
    publicFunctionPrefix,
    publicFunctionSubDomain,
    corsDomains,
    shouldProtect
);
const pubFuncCode = assignFunctionCode(
    resourceGroup,
    pubFunc.webApp,
    publicFunctionPrefix,
    '../Signal.Api.Public/bin/Release/net6.0/publish/',
    shouldProtect);

// Create Internal function
const intFunc = createFunction(
    resourceGroup,
    internalFunctionPrefix,
    shouldProtect
);
const intFuncCode = assignFunctionCode(
    resourceGroup,
    intFunc.webApp,
    internalFunctionPrefix,
    '../Signal.Api.Internal/bin/Release/net6.0/publish/',
    shouldProtect);

// Create general storage and prepare tables
const storage = createStorageAccount(resourceGroup, storagePrefix, shouldProtect);
const tableNames = [
    'userassigneddevices', 'userassignedprocesses', 'userassigneddashboards', 'userassignedbeacons',
    'beacons', 'devices', 'devicestates', 'devicesstateshistory', 'processes', 'dashboards', 'users',
    'webnewsletter'
];
tableNames.forEach(tableName => {
    new Table(`sa${storagePrefix}-table-${tableName}`, {
        resourceGroupName: resourceGroup.name,
        accountName: storage.storageAccount.name,
        tableName: tableName
    }, {
        protect: shouldProtect
    });
});

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
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'SignalcoKeyVaultUrl', interpolate`${vault.keyVault.properties.vaultUri}`);

// Populate function settings
assignFunctionSettings(
    resourceGroup,
    pubFunc.webApp,
    publicFunctionPrefix,
    pubFuncCode.storageAccount.connectionString,
    pubFuncCode.codeBlobUlr,
    {
        AzureSignalRConnectionString: signalr.connectionString,
        SignalcoKeyVaultUrl: interpolate`${vault.keyVault.properties.vaultUri}`
    },
    shouldProtect
);
assignFunctionSettings(
    resourceGroup,
    intFunc.webApp,
    internalFunctionPrefix,
    intFuncCode.storageAccount.connectionString,
    intFuncCode.codeBlobUlr,
    {
        SignalcoKeyVaultUrl: interpolate`${vault.keyVault.properties.vaultUri}`
    },
    shouldProtect
);

export const signalRUrl = signalr.signalr.hostName;
export const internalApiUrl = intFunc.webApp.hostNames[0];
export const publicApiUrl = pubFunc.dnsCname.hostname;

// TODO: Add Checkly checks for deployed functions
