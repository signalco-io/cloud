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

## Development for Cloud

[![Deploy Public API to Azure Functions](https://github.com/signalco-io/cloud/actions/workflows/deploy-azure-function-public.yml/badge.svg)](https://github.com/signalco-io/cloud/actions/workflows/deploy-azure-function-public.yml)
[![Deploy Internal API to Azure Functions](https://github.com/signalco-io/cloud/actions/workflows/deploy-azure-function-internal.yml/badge.svg)](https://github.com/signalco-io/cloud/actions/workflows/deploy-azure-function-internal.yml)
[![CodeQL](https://github.com/signalco-io/cloud/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/signalco-io/cloud/actions/workflows/codeql-analysis.yml)
[![Maintainability](https://api.codeclimate.com/v1/badges/47b77031e67ff69bb053/maintainability)](https://codeclimate.com/github/signalco-io/cloud/maintainability)

### Azure KeyVault configuration

Required secrets

| Name | Description | Example |
|------|-------------|---------|
| `Auth0--AppIdentifier` | Auth0 App Identifier | `https://api.signal.dfnoise.com` |
| `Auth0--Domain` | Auth0 Domain | `dfnoise.eu.auth0.com` |
| `SignalStorageAccountConnectionString` | Azure Storage Account connection string | `DefaultEndpointsProtocol=https;AccountName=signal;AccountKey=ACCOUNT_KEY;EndpointSuffix=core.windows.net` |
| `AzureSpeech--SubscriptionKey` | Azure Speech subscription key | `AZURE_SPEECH_SUBSCRIPTION_KEY` |
| `AzureSpeech--Region` | Azure Speech region | `westeurope` |
