namespace Signal.Core
{
    public static class SecretKeys
    {
        public const string StorageAccountConnectionString = "SignalStorageAccountConnectionString";

        public static class OidcApiAuthorizationSettings
        {
            public const string IssuerUrl = "OidcApiAuthorizationSettings--IssuerUrl";

            public const string Audience = "OidcApiAuthorizationSettings--Audience";
        }
    }
}
