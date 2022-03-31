import * as pulumi from "@pulumi/pulumi";
import * as web from "@pulumi/azure-native/web";
import * as resources from "@pulumi/azure-native/resources";
import * as cloudflare from "@pulumi/cloudflare";
import * as command from "@pulumi/command";

function dnsRecord(name: string, dnsName: string | pulumi.Output<string>, value: string | pulumi.Output<string>, type: "CNAME" | "TXT", protect: boolean) {
    return new cloudflare.Record(name, {
        name: dnsName,
        zoneId: "1f5a35e22cb52dfb6f087934cf2141a5", // TODO: Use from config
        type: type,
        value: value
    }, {
        protect: protect
    });
}

export function assignCustomDomain(resourceGroup: resources.ResourceGroup, webApp: web.WebApp, servicePlan: web.AppServicePlan, namePrefix: string, domainName: string, protect: boolean) {
    const txtVerifyRecord = dnsRecord(
        `func-dns-txt-domainverify-${namePrefix}`,
        "asuid." + domainName,
        pulumi.interpolate`${webApp.customDomainVerificationId}`,
        "TXT",
        protect);
    const cname = dnsRecord(
        `dns-cname-${namePrefix}`,
        "next-api", // TODO: Use subdomain from domainName
        webApp.hostNames[0],
        "CNAME",
        protect
    );

    // Until Pulumi comes up with circular dependency solution
    // we are creating hostname binding via Azure CLI because 
    // it's required to create certificate, only then the
    // hostname binding with SSL cert can be configured
    const assignHostName = new command.local.Command(`func-hostname-assign-${namePrefix}`, {
        create: pulumi.interpolate`az webapp config hostname add --webapp-name \"${webApp.name}\" --resource-group \"${resourceGroup.name}\" --hostname \"${domainName}\"`,
    }, {
        dependsOn: [txtVerifyRecord],
        ignoreChanges: ["create"]
    });

    const cert = new web.Certificate(`func-cert-${namePrefix}`, {
        resourceGroupName: resourceGroup.name,
        canonicalName: domainName,
        serverFarmId: servicePlan.id,
    }, {
        protect: protect,
        dependsOn: [assignHostName],
    });

    const resetHostName = new command.local.Command(`func-hostname-remove-${namePrefix}`, {
        create: pulumi.interpolate`az webapp config hostname delete --webapp-name \"${webApp.name}\" --resource-group \"${resourceGroup.name}\" --hostname \"${domainName}\"`
    }, {
        dependsOn: [cert], 
        ignoreChanges: ["create"]
    })

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
