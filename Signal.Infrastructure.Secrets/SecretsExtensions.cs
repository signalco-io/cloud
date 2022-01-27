using Microsoft.Extensions.DependencyInjection;
using Signal.Core;

namespace Signal.Infrastructure.Secrets;

public static class SecretsExtensions
{
    public static IServiceCollection AddSecrets(this IServiceCollection services) => 
        services.AddTransient<ISecretsProvider, SecretsProvider>();
}