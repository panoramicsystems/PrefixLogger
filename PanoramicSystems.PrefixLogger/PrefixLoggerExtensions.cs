using Microsoft.Extensions.Logging;

namespace PanoramicSystems
{
    public static class PrefixLoggerExtensions
    {
        public static PrefixLogger PrefixedWith(this ILogger iLogger, string prefix, string separator = ": ", TokenBehaviour tokenBehaviour = TokenBehaviour.UseTokens)
            => new(iLogger, prefix, separator, tokenBehaviour);
    }
}
