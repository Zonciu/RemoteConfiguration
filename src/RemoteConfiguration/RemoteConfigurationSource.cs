using System;
using Microsoft.Extensions.Configuration;

namespace RemoteConfiguration
{
    public class RemoteConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// Remote data source accessor.
        /// </summary>
        public IRemoteConfigurationAccessor Accessor { get; set; } = null!;

        /// <summary>
        /// Determines if loading the remote data is optional.
        /// </summary>
        public bool Optional { get; set; }

        /// <summary>
        /// Number of seconds of timed reload.
        /// </summary>
        public int ReloadInterval { get; set; } = 60;

        /// <summary>
        /// Will be called if an uncaught exception occurs in RemoteConfigurationProvider.Load.
        /// </summary>
        public Action<RemoteLoadExceptionContext>? OnLoadException { get; set; }

        /// <summary>
        /// Token to force reload configuration.
        /// </summary>
        public ConfigurationReloadToken? ReloadToken { get; set; }

        /// <summary>
        /// Builds the <see cref="IConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>A <see cref="IConfigurationProvider"/></returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new RemoteConfigurationProvider(this);
        }

        /// <summary>
        /// Called to use any default settings on the builder like the RemoteProvider or RemoteLoadExceptionHandler.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        public void EnsureDefaults(IConfigurationBuilder builder)
        {
            OnLoadException ??= builder.GetRemoteLoadExceptionHandler();
        }
    }
}