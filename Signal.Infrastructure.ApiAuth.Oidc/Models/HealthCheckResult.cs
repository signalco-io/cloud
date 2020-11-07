﻿namespace Signal.Infrastructure.ApiAuth.Oidc.Models
{
    public class HealthCheckResult
    {
        /// <summary>
        /// Construt a HealthCheckResult that indicates good health.
        /// </summary>
        public HealthCheckResult()
        {
        }

        /// <summary>
        /// Construt a HealthCheckResult that indicates bad health.
        /// </summary>
        /// <param name="badHealthMessage">
        /// The message describing the bad health.
        /// </param>
        public HealthCheckResult(string? badHealthMessage)
        {
            this.BadHealthMessage = badHealthMessage;
        }

        public bool IsHealthy => this.BadHealthMessage == null;

        public string? BadHealthMessage { get; set; }
    }
}
