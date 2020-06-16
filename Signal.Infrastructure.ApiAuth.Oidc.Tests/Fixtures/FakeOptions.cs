using Microsoft.Extensions.Options;

namespace Signal.Infrastructure.ApiAuth.Oidc.Tests.TestFixtures
{
    /// <summary>
    /// A fake for providing fake options to test fixtures.
    /// </summary>
    /// <typeparam name="TOptions">
    /// The type of the options to provide.
    /// </typeparam>
    public class FakeOptions<TOptions> : IOptions<TOptions>
        where TOptions : class, new()
    {
        /// <summary>
        /// Get or Set the options to provide.
        /// </summary>
        public TOptions Value { get; set; }
    }
}
