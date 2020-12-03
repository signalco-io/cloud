using Microsoft.AspNetCore.Mvc;

namespace Signal.Api.Voice
{
    public class GetTextAudioResponse : FileContentResult
    {
        public GetTextAudioResponse(byte[] fileContents) : base(fileContents, "application/octet-stream")
        {
        }
    }
}
