using System;

namespace Signal.Api.Public
{
    public class BeaconRefreshTokenResponseDto
    {
        public BeaconRefreshTokenResponseDto(string accessToken, DateTime expire)
        {
            this.AccessToken = accessToken;
            this.Expire = expire;
        }

        public string AccessToken { get; }

        public DateTime Expire { get; }
    }
}