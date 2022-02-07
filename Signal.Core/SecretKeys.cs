namespace Signal.Core
{
    public static class SecretKeys
    {
        public const string StorageAccountConnectionString = "SignalStorageAccountConnectionString";

        public static class Auth0
        {
            public const string ApiIdentifier = "Auth0--ApiIdentifier";

            public const string Domain = "Auth0--Domain";

            public const string ClientSecretBeacon = "Auth0--ClientSecret--Station";

            public const string ClientIdBeacon = "Auth0--ClientId--Station";
        }

        public static class HCaptcha
        {
            public const string SiteKey = "HCaptcha--SiteKey";

            public const string Secret = "HCaptcha--Secret";
        }

        public static class AzureSpeech
        {
            public const string SubscriptionKey = "AzureSpeech--SubscriptionKey";

            public const string Region = "AzureSpeech--Region";
        }
    }
}
