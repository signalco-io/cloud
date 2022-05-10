<p align="center">
  <a href="#">
    <img height="128" width="455" alt="signalco" src="https://raw.githubusercontent.com/signalco-io/cloud/main/docs/images/logo-ghtheme-128x455.png">
  </a>
</p>
<h4 align="center">Automate your life.</h4>

<p align="center">
  <a href="#getting-started">Getting Started</a> •
  <a href="#development-for-cloud">Development for Cloud</a>
</p>

## Getting Started

Visit <a aria-label="Signalco learn" href="https://www.signalco.io/learn">https://www.signalco.io/learn</a> to get started with Signalco.

### Status

| Production | Next |
|------------|------|
| [![Deploy Production](https://github.com/signalco-io/cloud/actions/workflows/deploy-cloud.yml/badge.svg?branch=main)](https://github.com/signalco-io/cloud/actions/workflows/deploy-cloud.yml)<br/>[![CodeQL](https://github.com/signalco-io/cloud/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/signalco-io/cloud/actions/workflows/codeql-analysis.yml)<br/>[![Maintainability](https://api.codeclimate.com/v1/badges/47b77031e67ff69bb053/maintainability)](https://codeclimate.com/github/signalco-io/cloud/maintainability)<br/>[![CodeFactor](https://www.codefactor.io/repository/github/signalco-io/cloud/badge)](https://www.codefactor.io/repository/github/signalco-io/cloud) | [![Deploy Development](https://github.com/signalco-io/cloud/actions/workflows/deploy-cloud.yml/badge.svg?branch=next)](https://github.com/signalco-io/cloud/actions/workflows/deploy-cloud.yml) |

## Development for Cloud

### API reference

Production API

- OpenAPI v3 specs: `https://api.signalco.io/api/swagger.{extension}`
- Swagger UI: `https://api.signalco.io/api/swagger/ui`

### Deploying infrastructure

#### Locally via CLI

##### **Build projects**

- `dotnet publish ./Signal.Api.Public --configuration Release`
- `dotnet publish ./Signal.Api.Internal --configuration Release`
- `dotnet publish ./Signalco.Cloud.Channel.GitHubApp --configuration Release`

##### **Pulumi (required for Deploy step)**

- [Install Pulumi](https://www.pulumi.com/docs/get-started/install)
  - Windows: `winget install pulumi`
  - MacOS: `brew install pulumi`
- Navigate to `./infrastructure`
- `yarn install`
- `pulumi login`
- `pulumi stack select` or `pulumi stack new` to create your new stack

##### **Azure (required for Deploy step)**

- (optional) [Install Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
  - Windows: `winget install Microsoft.AzureCLI`
  - MacOS: `brew install azure-cli`
- Configure
  - on new stack
    - `az login`
    - or
      - `pulumi config set azure-native:clientId <clientID>`
      - `pulumi config set azure-native:clientSecret <clientSecret> --secret`
      - `pulumi config set azure-native:tenantId <tenantID>`
      - `pulumi config set azure-native:subscriptionId <subscriptionId>`
  - stacks `next` and `production` already configured

##### **AWS (required for Deploy step)**

- [Get credentials](https://www.pulumi.com/registry/packages/aws/installation-configuration/#get-your-credentials)
  - [Create IAM user with **Programmatic access**](https://docs.aws.amazon.com/IAM/latest/UserGuide/id_users_create.html#id_users_create_console)
  - Retrieve aAccess Key ID and Access Key Secret after you created the user
- Configure shared
  - on new stack
    - `pulumi config set ses-region eu-west-1`
  - stacks `next` and `production` are already configured
- Configure (pick one method)
  - _(recommended)_ create file `.aws/credentials` in your home directory
  - set environment variables (for CI/CD)
    - Windows:
      - `$env:AWS_ACCESS_KEY_ID = "<YOUR_ACCESS_KEY_ID>"`
      - `$env:AWS_SECRET_ACCESS_KEY = "<YOUR_SECRET_ACCESS_KEY>"`
    - MacOS/Linux:
      - `export AWS_ACCESS_KEY_ID=<YOUR_ACCESS_KEY_ID>`
      - `export AWS_SECRET_ACCESS_KEY=<YOUR_SECRET_ACCESS_KEY>`

`credentials` example:

```txt
[default]
aws_access_key_id = <YOUR_ACCESS_KEY_ID>
aws_secret_access_key = <YOUR_SECRET_ACCESS_KEY>
```

##### **CloudFlare (required for Deploy step)**

- on new stack
  - `pulumi config set --secret cloudflare:apiToken TOKEN`
- stacks `next` and `production` already configured

Checkly (prereqesite for Pulumi)

- on new stack
  - `pulumi config set checkly:apiKey cu_xxx --secret`
  - `pulumi config set checkly:accountId xxx`
- stacks `next` and `production` already configured

##### **Deploy**

- `pulumi up --stack <STACK>`
- `pulumi destroy` when done testing

#### Via GitHub Actions

Required secrets for GitHub actions are:

- `PULUMI_ACCESS_TOKEN` [Create a new Pulumi Access Token](https://app.pulumi.com/account/tokens) for Pulumi
- `AWS_ACCESS_KEY_ID` and `AWS_SECRET_ACCESS_KEY`
- Azure access is configured as Pulumi secret via [Service Principal](https://www.pulumi.com/registry/packages/azure-native/installation-configuration/#option-2-use-a-service-principal)
- CloudFlare token is configured as Pulumi secret via [Provider](https://www.pulumi.com/registry/packages/cloudflare/installation-configuration/#configuring-the-provider)
- Checkly token is configured as Pulumi secret via [API Key](https://www.pulumi.com/registry/packages/checkly/installation-configuration/#configuring-credentials)

#### Troubleshooting

##### Azure CLI warning about Microsoft Graph migration

```txt
error: Error: invocation of azure-native:authorization:getClientConfig returned an error: getting authenticated object ID: Error parsing json result from the Azure CLI: Error retrieving running Azure CLI: WARNING: The underlying Active Directory Graph API will be replaced by Microsoft Graph API in a future version of Azure CLI. Please carefully review all breaking changes introduced during this migration: https://docs.microsoft.com/cli/azure/microsoft-graph-migration
```

Followed by discussion here: <https://github.com/pulumi/pulumi-azure-native/discussions/1565>

The current (2022-03-31) workaround is to either:

1. Pin the az CLI to `2.33.1`
2. Set the following global config for az CLI: `az config set core.only_show_errors=true`
