import * as pulumi from "@pulumi/pulumi";
import * as web from "@pulumi/azure-native/web";
import * as resources from "@pulumi/azure-native/resources";
import * as cloudflare from "@pulumi/cloudflare";
import * as command from "@pulumi/command";

export function assignCustomDomain(resourceGroup: resources.ResourceGroup, webApp: web.WebApp, servicePlan: web.AppServicePlan, namePrefix: string, domainName: string, protect: boolean) {
    const txtVerifyRecord = new cloudflare.Record(`func-dns-txt-domainverify-${namePrefix}`, {
        name: "asuid." + domainName,
        zoneId: "1f5a35e22cb52dfb6f087934cf2141a5",
        type: "TXT",
        value: pulumi.interpolate`${webApp.customDomainVerificationId}`
    }, {
        protect: protect
    });
    new cloudflare.Record("dns-cname-cpub", {
        name: "next-api", // TODO: Use subdomain from domainName
        zoneId: "1f5a35e22cb52dfb6f087934cf2141a5", // TODO: Use from config
        type: "CNAME",
        value: webApp.hostNames[0],
        ttl: 1
    }, {
        protect: protect
    });

    const assignHostName = new command.local.Command("func-hostname-assign", {
        create: pulumi.interpolate`az webapp config hostname add --webapp-name ${webApp.name} --resource-group ${resourceGroup.name} --hostname ${domainName}`,
        // delete: `az webapp config hostname remove --webapp-name ${webApp.name} --resource-group ${resourceGroup.id} --hostname ${domainName}`,
    }, {
        dependsOn: [txtVerifyRecord]
    });

    // TODO: Assign hostname binding
    // const bindingDisabled = new web.WebAppHostNameBinding(`func-hostnamebind-${namePrefix}-disable`, {
    //     name: app.name,
    //     resourceGroupName: resourceGroup.name,
    //     hostName: domainName,
    //     hostNameType: web.HostNameType.Verified,
    //     sslState: web.SslState.Disabled,
    //     customHostNameDnsRecordType: web.CustomHostNameDnsRecordType.CName
    // }, {
    //     protect: protect,
    //     dependsOn: [record]
    // });

    const cert = new web.Certificate('func-cert-' + namePrefix, {
        resourceGroupName: resourceGroup.name,
        canonicalName: domainName,
        serverFarmId: servicePlan.id,
    }, {
        protect: protect,
        dependsOn: [assignHostName],
    });

    const resetHostName = new command.local.Command("func-hostname-remove", {
        create: pulumi.interpolate`az webapp config hostname remove --webapp-name ${webApp.name} --resource-group ${resourceGroup.id} --hostname ${domainName}`
    }, {
        dependsOn: [cert]
    })

    // TODO: Set SSL and thumbprint to hostname binding
    const bindingEnabled = new web.WebAppHostNameBinding(`func-hostnamebind-${namePrefix}`, {
        name: webApp.name,
        resourceGroupName: resourceGroup.name,
        hostName: domainName,
        hostNameType: web.HostNameType.Verified,
        sslState: web.SslState.SniEnabled,
        customHostNameDnsRecordType: web.CustomHostNameDnsRecordType.CName,
        thumbprint: cert.thumbprint
    }, {
        dependsOn: [resetHostName],
        protect: protect
    });

    return webApp;
}
