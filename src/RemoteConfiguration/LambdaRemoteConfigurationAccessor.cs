using System;
using System.Threading.Tasks;

namespace RemoteConfiguration
{
    internal class LambdaRemoteConfigurationAccessor : IRemoteConfigurationAccessor
    {
        private readonly Func<Task<string?>> _func;

        public LambdaRemoteConfigurationAccessor(Func<Task<string?>> func)
        {
            _func = func;
        }

        public Task<string?> GetJsonString() => _func();
    }
}