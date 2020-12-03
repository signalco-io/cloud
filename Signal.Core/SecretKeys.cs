namespace Signal.Core
{
    public static class SecretKeys
    {
        public const string StorageAccountConnectionString = "SignalStorageAccountConnectionString";

        public static class OidcApiAuthorizationSettings
        {
            public const string IssuerUrl = "Oidc--IssuerUrl";

            public const string Audience = "Oidc--Audience";
        }

        public static class Auth0
        {
            public const string AppIdentifier = "Auth0--AppIdentifier";

            public const string Domain = "Auth0--Domain";
        }

        public static class AzureSpeech
        {
            public const string SubscriptionKey = "AzureSpeech--SubscriptionKey";

            public const string Region = "AzureSpeech--Region";
        }
    }
}
