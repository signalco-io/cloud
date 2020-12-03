using System.Threading;
using System.Threading.Tasks;

namespace Signal.Core
{
    public interface IVoiceService
    {
        Task<byte[]> TextToAudioAsync(string text, CancellationToken cancellationToken);
    }
}