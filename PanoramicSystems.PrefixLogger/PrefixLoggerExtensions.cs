using Microsoft.Extensions.Logging;

namespace PanoramicSystems
{
    public static class PrefixLoggerExtensions
    {
        public static ILogger PrefixedWith(this ILogger iLogger, string prefix)
            => new PrefixLogger(iLogger, prefix);

        public static ILogger PrefixedWith(this ILogger iLogger, string prefix, string separator)
            => new PrefixLogger(iLogger, prefix, separator);
    }
}
