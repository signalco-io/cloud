using System;
using System.Threading;
using System.Threading.Tasks;
using Signal.Core;

namespace Signal.Infrastructure.AzureSpeech
{
    internal class AzureSpeechVoiceService : IVoiceService
    {
        private readonly IVoiceSynthesizer voiceSynthesizer;


        public AzureSpeechVoiceService(IVoiceSynthesizer voiceSynthesizer)
        {
            this.voiceSynthesizer = voiceSynthesizer ?? throw new ArgumentNullException(nameof(voiceSynthesizer));
        }


        public async Task<byte[]> TextToAudioAsync(string text, CancellationToken cancellationToken)
        {
            // TODO: Cache text audio to reduce Azure usage

            return await this.voiceSynthesizer.TextToAudioAsync(text, cancellationToken);
        }
    }
}