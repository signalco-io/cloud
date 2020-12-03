using Voyager.Api;

namespace Signal.Api.Voice
{
    [VoyagerRoute(HttpMethod.Get, "voice/text-audio")]
    public class GetTextAudioRequest : EndpointRequest<GetTextAudioResponse>
    {
        public string Text { get; set; }
    }
}