using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Timer = System.Timers.Timer;

namespace RemoteConfiguration
{
    public class RemoteConfigurationProvider : ConfigurationProvider, IDisposable
    {
        private readonly RemoteConfigurationSource _source;

        private readonly Timer? _reloadTimer;

        private readonly IDisposable? _reloadRegistration;

        public RemoteConfigurationProvider(RemoteConfigurationSource source)
        {
            _source = source;
            if (_source.ReloadToken != null)
            {
                _reloadRegistration = _source.ReloadToken.RegisterChangeCallback(_ => { Load(true).GetAwaiter().GetResult(); }, null);
            }

            if (_source.ReloadInterval > 0)
            {
                _reloadTimer = new Timer();
                _reloadTimer.Elapsed += (_, _) => { Load(true).GetAwaiter().GetResult(); };
                _reloadTimer.Interval = TimeSpan.FromSeconds(_source.ReloadInterval).TotalMilliseconds;
                _reloadTimer.Start();
            }
        }

        private async Task Load(bool reload)
        {
            try
            {
                var jsonString = await _source.Accessor.GetJsonString();
                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    if (_source.Optional)
                    {
                        Data = new Dictionary<string, string>();
                        if (reload)
                        {
                            OnReload();
                        }
                    }
                    else
                    {
                        HandleException(ExceptionDispatchInfo.Capture(new Exception("The configuration is empty and is not optional.")));
                    }
                }
                else
                {
                    Data = JsonConfigurationStringParser.Parse(jsonString);
                    if (reload)
                    {
                        OnReload();
                    }
                }
            }
            catch (Exception e)
            {
                HandleException(ExceptionDispatchInfo.Capture(e));
            }
        }

        public override void Load()
        {
            Load(false).GetAwaiter().GetResult();
        }

        private void HandleException(ExceptionDispatchInfo info)
        {
            var ignoreException = false;
            if (_source.OnLoadException != null)
            {
                var exceptionContext = new RemoteLoadExceptionContext
                {
                    Provider = this,
                    Exception = info.SourceException
                };
                _source.OnLoadException.Invoke(exceptionContext);
                ignoreException = exceptionContext.Ignore;
            }

            if (!ignoreException)
            {
                info.Throw();
            }
        }

        public void Dispose()
        {
            _reloadTimer?.Stop();
            _reloadTimer?.Dispose();
            _reloadRegistration?.Dispose();
        }
    }
}