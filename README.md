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

Visit <a aria-label="Signalco learn" href="<https://www.signalco.io/learn>">https://www.signalco.io/learn</a> to get started with Signalco.

## Development for Cloud

Deployments

[![Deploy Public API to Azure Functions](https://github.com/signalco-io/cloud/actions/workflows/deploy-azure-function-public.yml/badge.svg)](https://github.com/signalco-io/cloud/actions/workflows/deploy-azure-function-public.yml)

[![Deploy Internal API to Azure Functions](https://github.com/signalco-io/cloud/actions/workflows/deploy-azure-function-internal.yml/badge.svg)](https://github.com/signalco-io/cloud/actions/workflows/deploy-azure-function-internal.yml)

Code

[![CodeQL](https://github.com/signalco-io/cloud/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/signalco-io/cloud/actions/workflows/codeql-analysis.yml)
[![Maintainability](https://api.codeclimate.com/v1/badges/47b77031e67ff69bb053/maintainability)](https://codeclimate.com/github/signalco-io/cloud/maintainability)
[![CodeFactor](https://www.codefactor.io/repository/github/signalco-io/cloud/badge)](https://www.codefactor.io/repository/github/signalco-io/cloud)

### API reference

Production API

- OpenAPI v3 specs: `https://api.signalco.io/api/swagger.{extension}`
- Swagger UI: `https://api.signalco.io/api/swagger/ui`

### Azure Function application settings

Required settings

| Name | Description | Example |
|------|-------------|---------|
| `AzureSignalRConnectionString` | SignalR connection string | `Endpoint=https://signalhub.service.signalr.net;AccessKey=d8s5FF5f48aS8s6s5s22+SbWvdasdaswGhs4/s4s8s7s554=;Version=1.0;` |
| `OpenApi__Version` | OpenAPI version | `v3` |
| `OpenApi__DocTitle` | OpenAPI document title | `Signalco Public Cloud API documentation` |

### Azure KeyVault configuration

Required secrets

| Name | Description | Example |
|------|-------------|---------|
| `Auth0--AppIdentifier` | Auth0 App Identifier | `https://api.signal.dfnoise.com` |
| `Auth0--Domain` | Auth0 Domain | `dfnoise.eu.auth0.com` |
| `SignalStorageAccountConnectionString` | Azure Storage Account connection string | `DefaultEndpointsProtocol=https;AccountName=signal;AccountKey=ACCOUNT_KEY;EndpointSuffix=core.windows.net` |
| `AzureSpeech--SubscriptionKey` | Azure Speech subscription key | `dasdas4897dsa4dw7a4s8qd7a78a5s7a8s5a3ssdaghhy8r4` |
| `AzureSpeech--Region` | Azure Speech region | `westeurope` |
| `HCaptcha--SiteKey` | hCaptcha site key | `608453b3-d2db-4382-8694-15071d873d1f` |
| `HCaptcha--Secret` | hCaptcha secret | `0x0056f1e6187b8e78e427da0fb1fa88a9` |
