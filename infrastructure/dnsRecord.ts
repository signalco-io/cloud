import { Config, Output, Resource } from "@pulumi/pulumi";
import { Record } from "@pulumi/cloudflare";

export function dnsRecord(name: string, dnsName: string | Output<string>, value: string | Output<string>, type: "CNAME" | "TXT", protect: boolean, parent?: Resource) {
    const config = new Config();
    const zoneId = config.requireSecret('zoneid');
    return new Record(name, {
        name: dnsName,
        zoneId: zoneId,
        type: type,
        value: value
    }, {
        protect: protect,
        // parent: parent
    });
}
