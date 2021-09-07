# Signalco Cloud

[![Deploy Public API to Azure Functions](https://github.com/signalco-io/cloud/actions/workflows/deploy-azure-function-public.yml/badge.svg)](https://github.com/signalco-io/cloud/actions/workflows/deploy-azure-function-public.yml)
[![Deploy Internal API to Azure Functions](https://github.com/signalco-io/cloud/actions/workflows/deploy-azure-function-internal.yml/badge.svg)](https://github.com/signalco-io/cloud/actions/workflows/deploy-azure-function-internal.yml)
[![CodeQL](https://github.com/signalco-io/cloud/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/signalco-io/cloud/actions/workflows/codeql-analysis.yml)

### Azure KeyVault configuration

Required secrets

| Name | Description | Example |
|------|-------------|---------|
| `Auth0--AppIdentifier` | Auth0 App Identifier | `https://api.signal.dfnoise.com` |
| `Auth0--Domain` | Auth0 Domain | `dfnoise.eu.auth0.com` |
| `SignalStorageAccountConnectionString` | Azure Storage Account connection string | `DefaultEndpointsProtocol=https;AccountName=signal;AccountKey=ACCOUNT_KEY;EndpointSuffix=core.windows.net` |
| `AzureSpeech--SubscriptionKey` | Azure Speech subscription key | `AZURE_SPEECH_SUBSCRIPTION_KEY` |
| `AzureSpeech--Region` | Azure Speech region | `westeurope` |
