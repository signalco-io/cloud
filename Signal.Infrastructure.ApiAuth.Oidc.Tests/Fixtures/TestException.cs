using System;

namespace Signal.Infrastructure.ApiAuth.Oidc.Tests.TestFixtures
{
    public class TestException : Exception
    {
        public TestException()
        {
        }

        public TestException(string message)
            : base(message)
        {
        }
    }
}
