using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace RemoteConfiguration
{
    public static class RemoteConfigurationExtensions
    {
        private const string RemoteLoadExceptionHandlerKey = "RemoteLoadExceptionHandlerKey";

        /// <summary>
        /// Sets a default action to be invoked for file-based providers when an error occurs.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="handler">The Action to be invoked on a file load exception.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder SetRemoteLoadExceptionHandler(this IConfigurationBuilder builder, Action<RemoteLoadExceptionContext> handler)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Properties[RemoteLoadExceptionHandlerKey] = handler;
            return builder;
        }

        /// <summary>
        /// Gets the default <see cref="RemoteConfigurationProvider"/> to be used for file-based providers.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static Action<RemoteLoadExceptionContext>? GetRemoteLoadExceptionHandler(this IConfigurationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (builder.Properties.TryGetValue(RemoteLoadExceptionHandlerKey, out var handler))
            {
                return handler as Action<RemoteLoadExceptionContext>;
            }

            return null;
        }

        public static IConfigurationBuilder AddRemote(this IConfigurationBuilder builder, Func<Task<string?>> getJsonStringFunc)
        {
            return builder.AddRemote(new LambdaRemoteConfigurationAccessor(getJsonStringFunc), false, 0, null);
        }

        public static IConfigurationBuilder AddRemote(this IConfigurationBuilder builder, IRemoteConfigurationAccessor accessor)
        {
            return builder.AddRemote(accessor, false, 0, null);
        }

        public static IConfigurationBuilder AddRemote(this IConfigurationBuilder builder, Func<Task<string?>> getJsonStringFunc, bool optional)
        {
            return builder.AddRemote(new LambdaRemoteConfigurationAccessor(getJsonStringFunc), optional, 0, null);
        }

        public static IConfigurationBuilder AddRemote(this IConfigurationBuilder builder, IRemoteConfigurationAccessor accessor, bool optional)
        {
            return builder.AddRemote(accessor, optional, 0, null);
        }

        public static IConfigurationBuilder AddRemote(this IConfigurationBuilder builder, Func<Task<string?>> getJsonStringFunc, bool optional, int reloadInterval)
        {
            return builder.AddRemote(new LambdaRemoteConfigurationAccessor(getJsonStringFunc), optional, reloadInterval);
        }

        public static IConfigurationBuilder AddRemote(this IConfigurationBuilder builder, IRemoteConfigurationAccessor accessor, bool optional, int reloadInterval)
        {
            return builder.AddRemote(accessor, optional, reloadInterval, null);
        }

        public static IConfigurationBuilder AddRemote(this IConfigurationBuilder builder, Func<Task<string?>> getJsonStringFunc, bool optional, ConfigurationReloadToken? reloadToken)
        {
            return builder.AddRemote(new LambdaRemoteConfigurationAccessor(getJsonStringFunc), optional, 0, reloadToken);
        }

        public static IConfigurationBuilder AddRemote(this IConfigurationBuilder builder, IRemoteConfigurationAccessor accessor, bool optional, ConfigurationReloadToken? reloadToken)
        {
            return builder.AddRemote(accessor, optional, 0, reloadToken);
        }

        public static IConfigurationBuilder AddRemote(
            this IConfigurationBuilder builder, Func<Task<string?>> getJsonStringFunc, bool optional, int reloadInterval, ConfigurationReloadToken? reloadToken)
        {
            return builder.AddRemote(new LambdaRemoteConfigurationAccessor(getJsonStringFunc), optional, reloadInterval, reloadToken);
        }

        public static IConfigurationBuilder AddRemote(this IConfigurationBuilder builder, Action<RemoteConfigurationSource> action)
        {
            return builder.Add(action);
        }

        public static IConfigurationBuilder AddRemote(
            this IConfigurationBuilder builder, IRemoteConfigurationAccessor accessor, bool optional, int reloadInterval, ConfigurationReloadToken? reloadToken)
        {
            if (accessor == null!)
            {
                throw new ArgumentNullException(nameof(accessor));
            }

            return builder.Add(
                new RemoteConfigurationSource
                {
                    Accessor = accessor,
                    Optional = optional,
                    ReloadToken = reloadToken,
                    ReloadInterval = reloadInterval
                });
        }
    }
}