# signalapi
Signal API

![Deploy Public API to Azure Functions](https://github.com/AleksandarDev/signalapi/workflows/Deploy%20Public%20API%20to%20Azure%20Functions/badge.svg)
![CodeQL](https://github.com/dfnoise/signalapi/workflows/CodeQL/badge.svg)

### Azure KeyVault configuration

Required secrets

| Name | Description | Example |
|------|-------------|---------|
| `Auth0--AppIdentifier` | Auth0 App Identifier | `https://api.signal.dfnoise.com` |
| `Auth0--Domain` | Auth0 Domain | `dfnoise.eu.auth0.com` |
| `SignalStorageAccountConnectionString` | Azure Storage Account connection string | `DefaultEndpointsProtocol=https;AccountName=signal;AccountKey=ACCOUNT_KEY;EndpointSuffix=core.windows.net` |
| `AzureSpeech--SubscriptionKey` | Azure Speech subscription key | `AZURE_SPEECH_SUBSCRIPTION_KEY` |
| `AzureSpeech--Region` | Azure Speech region | `westeurope` |
