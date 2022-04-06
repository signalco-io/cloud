import { Input } from '@pulumi/pulumi';
import { WebApp, WebAppApplicationSettings } from '@pulumi/azure-native/web';
import { ResourceGroup } from '@pulumi/azure-native/resources';

export function assignFunctionSettings (resourceGroup: ResourceGroup, app: WebApp, namePrefix: string, storageConnectionString: Input<string>, codeUrl: Input<string>, appSettings: Input<{ [key: string]: Input<string>; }>, protect: boolean) {
    const settings = new WebAppApplicationSettings(`func-appsettings-${namePrefix}`, {
        name: app.name,
        resourceGroupName: resourceGroup.name,
        properties: {
            AzureWebJobsStorage: storageConnectionString,
            WEBSITE_RUN_FROM_PACKAGE: codeUrl,
            FUNCTIONS_EXTENSION_VERSION: '~4',
            FUNCTIONS_WORKER_RUNTIME: 'dotnet',
            OpenApi__DocTitle: 'Signalco Cloud API',
            OpenApi__Version: 'v3',
            ...appSettings
        }
    }, {
        protect: protect
        // parent: app
    });

    return {
        appSettings: settings
    };
}
