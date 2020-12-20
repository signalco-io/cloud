using System;
using Signal.Core;

namespace Signal.Api.Public
{
    public class UserAuth : IUserAuth
    {
        public UserAuth(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
            
            this.UserId = userId;
        }

        public string UserId { get; }
    }
}