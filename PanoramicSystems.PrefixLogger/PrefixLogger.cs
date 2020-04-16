using Microsoft.Extensions.Logging;
using System;

namespace PanoramicSystems
{
	public class PrefixLogger : ILogger
	{
		public string Prefix { get; set; }
		public string Separator { get; set; }

		private readonly ILogger _logger;

		public PrefixLogger(ILogger logger, string prefix, string separator = ": ")
		{
			if (string.IsNullOrWhiteSpace(prefix))
			{
				throw new ArgumentNullException(nameof(prefix));
			}
			Prefix = prefix;

			if (string.IsNullOrWhiteSpace(separator))
			{
				throw new ArgumentNullException(nameof(separator));
			}
			Separator = separator;

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);

		public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			var message = formatter != null && exception != null
				? formatter(state, exception)
				: state?.ToString() ?? string.Empty;

			if (exception == null)
			{
				_logger.Log(logLevel, eventId, Prefix + Separator + message);
			}
			else
			{
				_logger.Log(logLevel, eventId, Prefix + Separator + message, exception, formatter);
			}
		}
	}
}
