using System.Threading.Tasks;

namespace RemoteConfiguration
{
    public interface IRemoteConfigurationAccessor
    {
        public Task<string?> GetJsonString();
    }
}