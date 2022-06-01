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
import * as checkly from '@checkly/pulumi';
import createWebAppAppInsights from './createWebAppAppInsights';
import createAppInsights from './createAppInsights';
import createSes from './createSes';
import createChannelFunction from './createChannelFunction';

/*
 * NOTE: `parent` configuration is currently disabled for all resources because
 *       there is memory issued when enabled. (2022/04)
 * NOTE: checkly provider needs manual installation of plugin currently (2022/04)
 *       which is added to deployment action
 * NOTE: waiting for Circular dependency solution: https://github.com/pulumi/pulumi/issues/3021
 *       current workaround is to run `az` commands for domain verification (2022/04)
 *       - this doesn't work in CI because az is not installed and authenticated
 */

const config = new Config();
const stack = getStack();
const shouldProtect = stack === 'production';
const domainName = `${config.require('domain')}`;

const resourceGroupName = `signalco-cloud-${stack}`;
const publicFunctionPrefix = 'cpub';
const publicFunctionSubDomain = 'api';
const internalFunctionPrefix = 'cint';
const signalrPrefix = 'sr';
const storagePrefix = 'store';
const keyvaultPrefix = 'kv';

const resourceGroup = new ResourceGroup(resourceGroupName);
const corsDomains = [`www.${domainName}`, domainName];

const signalr = createSignalR(resourceGroup, signalrPrefix, corsDomains, shouldProtect);
new checkly.Check(`signalr-check-${signalrPrefix}`, {
    name: `SignalR (${stack})`,
    activated: true,
    frequency: 10,
    type: 'API',
    locations: ['eu-west-1'],
    tags: [stack === 'production' ? 'public' : 'dev'],
    request: {
        method: 'GET',
        url: interpolate`https://${signalr.signalr.hostName}/api/v1/health`
    }
});

// Create Public function
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
new checkly.Check(`func-apicheck-${publicFunctionPrefix}`, {
    name: `API (${stack})`,
    activated: true,
    frequency: 5,
    type: 'API',
    locations: ['eu-west-1'],
    tags: [stack === 'production' ? 'public' : 'dev', 'api'],
    request: {
        method: 'GET',
        url: interpolate`https://${pubFunc.dnsCname.hostname}/api/status`
    }
});

const pubFuncInsights = createWebAppAppInsights(resourceGroup, publicFunctionPrefix, pubFunc.webApp);

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

new checkly.Check(`func-apicheck-${internalFunctionPrefix}`, {
    name: `Internal (${stack})`,
    activated: true,
    frequency: 15,
    type: 'API',
    locations: ['eu-west-1'],
    tags: [stack === 'production' ? 'public' : 'dev'],
    request: {
        method: 'GET',
        url: interpolate`https://${intFunc.webApp.hostNames[0]}/api/status`
    }
});

// Generate channels
const slackFunc = createChannelFunction('slack', resourceGroup, shouldProtect);

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
        tableName
    }, {
        protect: shouldProtect
    });
});

// Create AWS SES service
const ses = createSes(`ses-${stack}`, 'notification');

// Create and populate vault
const vault = createKeyVault(resourceGroup, keyvaultPrefix, shouldProtect, [
    webAppIdentity(pubFunc.webApp),
    webAppIdentity(intFunc.webApp),
    webAppIdentity(slackFunc.webApp)
]);
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'Auth0--ApiIdentifier', config.requireSecret('secret-auth0ApiIdentifier'));
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'Auth0--ClientId--Station', config.requireSecret('secret-auth0ClientIdStation'));
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'Auth0--ClientSecret--Station', config.requireSecret('secret-auth0ClientSecretStation'));
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'Auth0--Domain', config.require('secret-auth0Domain'));
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'HCaptcha--Secret', config.requireSecret('secret-hcaptchaSecret'));
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'HCaptcha--SiteKey', config.requireSecret('secret-hcaptchaSiteKey'));
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'SignalStorageAccountConnectionString', storage.connectionString);
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'SignalcoKeyVaultUrl', interpolate`${vault.keyVault.properties.vaultUri}`);
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'SmtpNotificationServerUrl', ses.smtpServer);
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'SmtpNotificationFromDomain', ses.smtpFromDomain);
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'SmtpNotificationUsername', ses.smtpUsername);
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'SmtpNotificationPassword', ses.smtpPassword);
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'Slack--SigningSecret', config.requireSecret('secret-slackSigningSecret'));
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'Slack--ClientId', config.require('secret-slackClientId'));
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'Slack--ClientSecret', config.requireSecret('secret-slackClientSecret'));

// Populate function settings
assignFunctionSettings(
    resourceGroup,
    pubFunc.webApp,
    publicFunctionPrefix,
    pubFuncCode.storageAccount.connectionString,
    pubFuncCode.codeBlobUlr,
    {
        AzureSignalRConnectionString: signalr.connectionString,
        SignalcoKeyVaultUrl: interpolate`${vault.keyVault.properties.vaultUri}`,
        APPINSIGHTS_INSTRUMENTATIONKEY: interpolate`${pubFuncInsights.component.instrumentationKey}`,
        APPLICATIONINSIGHTS_CONNECTION_STRING: interpolate`${pubFuncInsights.component.connectionString}`
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

// Populate channel function settings
assignFunctionSettings(
    resourceGroup,
    slackFunc.webApp,
    'channelslack',
    slackFunc.storageAccount.connectionString,
    slackFunc.codeBlobUlr,
    {
        SignalcoKeyVaultUrl: interpolate`${vault.keyVault.properties.vaultUri}`,
        APPINSIGHTS_INSTRUMENTATIONKEY: interpolate`${pubFuncInsights.component.instrumentationKey}`,
        APPLICATIONINSIGHTS_CONNECTION_STRING: interpolate`${pubFuncInsights.component.connectionString}`
    },
    shouldProtect
);

createAppInsights(resourceGroup, 'web', 'signalco');

export const signalRUrl = signalr.signalr.hostName;
export const internalApiUrl = intFunc.webApp.hostNames[0];
export const publicApiUrl = pubFunc.dnsCname.hostname;
export const channelsUrls = [
    slackFunc.dnsCname.hostname
];
