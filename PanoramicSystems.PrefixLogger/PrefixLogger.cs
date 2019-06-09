using Microsoft.Extensions.Logging;
using System;

namespace PanoramicSystems
{
	public class PrefixLogger : ILogger
	{
		private readonly string _prefix;
		private readonly ILogger _logger;

		public PrefixLogger(string prefix, ILogger logger)
		{
			_prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);

		public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			var message = formatter != null && exception != null
				? formatter(state, exception)
				: state.ToString();

			if (exception == null)
			{
				_logger.Log(logLevel, eventId, _prefix + ": " + message);
			}
			else
			{
				_logger.Log(logLevel, eventId, _prefix + ": " + message, exception, formatter);
			}
		}
	}
}
