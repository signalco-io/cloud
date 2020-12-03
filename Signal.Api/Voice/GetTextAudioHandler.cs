using System;
using Signal.Api.Handlers;
using Signal.Core;

namespace Signal.Api.Voice
{
    public class GetTextAudioHandler : ServiceHandler<GetTextAudioRequest, GetTextAudioResponse, IVoiceService, byte[]>
    {
        public GetTextAudioHandler(IServiceProvider serviceProvider) : base(serviceProvider, (req, service, cancellation) => service.TextToAudioAsync(req.Text, cancellation))
        {
        }
    }
}