using Microsoft.Extensions.Logging;

namespace PanoramicSystems
{
	public static class PrefixLoggerExtensions
	{
		public static PrefixLogger PrefixedWith(this ILogger iLogger, string prefix)
			=> new(iLogger, prefix);

		public static PrefixLogger PrefixedWith(this ILogger iLogger, string prefix, string separator)
			=> new(iLogger, prefix, separator);
	}
}
