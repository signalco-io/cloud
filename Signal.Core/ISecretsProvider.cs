using System.Threading.Tasks;

namespace Signal.Core
{
    public interface ISecretsProvider
    {
        Task<string> GetSecretAsync(string key);
    }
}
