using System;

namespace RemoteConfiguration
{
    public class RemoteLoadExceptionContext
    {
        /// <summary>
        /// The <see cref="RemoteConfigurationProvider"/> that caused the exception.
        /// </summary>
        public RemoteConfigurationProvider Provider { get; set; } = null!;

        /// <summary>
        /// The exception that occurred in Load.
        /// </summary>
        public Exception Exception { get; set; } = null!;

        /// <summary>
        /// If true, the exception will not be rethrown.
        /// </summary>
        public bool Ignore { get; set; }
    }
}