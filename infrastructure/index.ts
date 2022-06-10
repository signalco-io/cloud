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
import createWebAppAppInsights from './createWebAppAppInsights';
import createAppInsights from './createAppInsights';
import createSes from './createSes';
import createChannelFunction from './createChannelFunction';
import apiStatusCheck from './apiStatusCheck';

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
const channelNames = ['slack'];

const resourceGroup = new ResourceGroup(resourceGroupName);
const corsDomains = [`www.${domainName}`, domainName];

const signalr = createSignalR(resourceGroup, signalrPrefix, corsDomains, shouldProtect);
apiStatusCheck(signalrPrefix, 'SignalR', signalr.signalr.hostName, 15, '/api/v1/health');

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
apiStatusCheck(publicFunctionPrefix, 'API', pubFunc.dnsCname.hostname, 5);

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
apiStatusCheck(internalFunctionPrefix, 'Internal', intFunc.webApp.hostNames[0], 15);

// Generate channels functions
const channels = [];
for (let i = 0; i < channelNames.length; i++) {
    channels.push(createChannelFunction(channelNames[i], resourceGroup, shouldProtect));
}

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
    ...channels.map(c => webAppIdentity(c.webApp))
]);
const s1 = vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'Auth0--ApiIdentifier', config.requireSecret('secret-auth0ApiIdentifier'));
const s2 = vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'Auth0--ClientId--Station', config.requireSecret('secret-auth0ClientIdStation'), s1.secret);
const s3 = vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'Auth0--ClientSecret--Station', config.requireSecret('secret-auth0ClientSecretStation'), s2.secret);
const s4 = vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'Auth0--Domain', config.require('secret-auth0Domain'), s3.secret);
const s5 = vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'HCaptcha--Secret', config.requireSecret('secret-hcaptchaSecret'), s4.secret);
const s6 = vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'HCaptcha--SiteKey', config.requireSecret('secret-hcaptchaSiteKey'), s5.secret);
const s7 = vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'SignalStorageAccountConnectionString', storage.connectionString, s6.secret);
const s8 = vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'SignalcoKeyVaultUrl', interpolate`${vault.keyVault.properties.vaultUri}`, s7.secret);
const s9 = vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'SmtpNotificationServerUrl', ses.smtpServer, s8.secret);
const s10 = vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'SmtpNotificationFromDomain', ses.smtpFromDomain, s9.secret);
const s11 = vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'SmtpNotificationUsername', ses.smtpUsername, s10.secret);
const s12 = vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'SmtpNotificationPassword', ses.smtpPassword, s11.secret);
const s13 = vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'Slack--SigningSecret', config.requireSecret('secret-slackSigningSecret'), s12.secret);
const s14 = vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'Slack--ClientId', config.require('secret-slackClientId'), s13.secret);
vaultSecret(resourceGroup, vault.keyVault, keyvaultPrefix, 'Slack--ClientSecret', config.requireSecret('secret-slackClientSecret'), s14.secret);

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
channels.map(channel => {
    return assignFunctionSettings(
        resourceGroup,
        channel.webApp,
        'channelslack',
        channel.storageAccount.connectionString,
        channel.codeBlobUlr,
        {
            SignalcoKeyVaultUrl: interpolate`${vault.keyVault.properties.vaultUri}`,
            APPINSIGHTS_INSTRUMENTATIONKEY: interpolate`${pubFuncInsights.component.instrumentationKey}`,
            APPLICATIONINSIGHTS_CONNECTION_STRING: interpolate`${pubFuncInsights.component.connectionString}`
        },
        shouldProtect
    );
});

createAppInsights(resourceGroup, 'web', 'signalco');

export const signalRUrl = signalr.signalr.hostName;
export const internalApiUrl = intFunc.webApp.hostNames[0];
export const publicApiUrl = pubFunc.dnsCname.hostname;
export const channelsUrls = channels.map(c => c.dnsCname.hostname);
