import * as pulumi from '@pulumi/pulumi';
import * as aws from '@pulumi/aws';
import { interpolate } from '@pulumi/pulumi';
import { dnsRecord } from './dnsRecord';

export default function createSes (prefix: string, subdomain: string) {
    const stack = pulumi.getStack();
    const config = new pulumi.Config();
    const baseDomain = config.require('domain');
    const sesRegion = config.require('ses-region');

    const emailUser = new aws.iam.User(
        `${prefix}-usr`,
        {
            name: `${prefix}-email`,
            path: '/system/'
        }
    );

    // Policy
    const allowedFromAddress = stack === 'production' ? `*@${baseDomain}` : `*@${stack}.${baseDomain}`;
    new aws.iam.UserPolicy(
        `${prefix}-ses-policy`,
        {
            user: emailUser.name,
            policy: JSON.stringify({
                Version: '2012-10-17',
                Statement: [
                    {
                        Action: [
                            'ses:SendEmail',
                            'ses:SendTemplatedEmail',
                            'ses:SendRawEmail',
                            'ses:SendBulkTemplatedEmail'
                        ],
                        Effect: 'Allow',
                        Resource: '*',
                        Condition: {
                            StringLike: {
                                'ses:FromAddress': allowedFromAddress
                            }
                        }
                    }
                ]
            }, null, '  ')
        }
    );

    // Email Access key
    const emailAccessKey = new aws.iam.AccessKey(
        `${prefix}-ses-access-key`,
        { user: emailUser.name }
    );

    const sesSmtpUsername = interpolate`${emailAccessKey.id}`;
    const sesSmtpPassword = interpolate`${emailAccessKey.sesSmtpPasswordV4}`;

    const sesDomainIdentity = new aws.ses.DomainIdentity(`${prefix}-domainIdentity`, {
        domain: `${subdomain}.${baseDomain}`
    });
    dnsRecord(`${prefix}-verify`, interpolate`_amazonses.${sesDomainIdentity.domain}`, sesDomainIdentity.verificationToken, 'TXT', false);

    // MailFrom
    const mailFrom = new aws.ses.MailFrom(
        `${prefix}-ses-mail-from`,
        {
            domain: sesDomainIdentity.domain,
            mailFromDomain: pulumi.interpolate`bounce.${sesDomainIdentity.domain}`
        });

    dnsRecord(`${prefix}-ses-mail-from-mx-record`, `bounce.${subdomain}`, `feedback-smtp.${sesRegion}.amazonses.com`, 'MX', false);
    dnsRecord(`${prefix}-spf`, mailFrom.mailFromDomain, 'v=spf1 include:amazonses.com -all', 'TXT', false);
    dnsRecord(`${prefix}-ses-dmarc`, `_dmarc.${subdomain}`, 'v=DMARC1; p=none; rua=mailto:contact@signalco.io; fo=1;', 'TXT', false);

    const sesDomainDkim = new aws.ses.DomainDkim(`${prefix}-sesDomainDkim`, {
        domain: sesDomainIdentity.domain
    });
    for (let i = 0; i < 3; i++) {
        const dkimValue = interpolate`${sesDomainDkim.dkimTokens[i]}.dkim.amazonses.com`;
        const dkimName = interpolate`${sesDomainDkim.dkimTokens[i]}._domainkey.${subdomain}`;
        dnsRecord(`${prefix}-dkim${i}`, dkimName, dkimValue, 'CNAME', false);
    }

    return {
        smtpUsername: sesSmtpUsername,
        smtpPassword: sesSmtpPassword,
        smtpServer: `email-smtp.${sesRegion}.amazonaws.com`
    };
}
