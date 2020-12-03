using Microsoft.Extensions.DependencyInjection;
using Signal.Core;

namespace Signal.Infrastructure.AzureSpeech
{
    public static class AzureSpeechExtensions
    {
        public static void AddAzureSpeech(this IServiceCollection services)
        {
            services.AddTransient<IVoiceService, AzureSpeechVoiceService>();
            services.AddTransient<IVoiceSynthesizer, VoiceSynthesizer>();
        }
    }
}
