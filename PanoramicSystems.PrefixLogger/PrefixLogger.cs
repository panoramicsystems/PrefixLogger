using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace PanoramicSystems
{
	public class PrefixLogger : ILogger
	{
		public string Prefix { get; set; }
		public string Separator { get; set; }

		private readonly ILogger _logger;
		private readonly TokenBehaviour _tokenBehaviour;
		internal readonly string _prefixId;
		internal readonly string _plPrefixWithId;
		internal readonly string _plPrefixSeparatorWithId;
		internal readonly string _plPrefixAndSeparatorWithId;

		public PrefixLogger(ILogger logger, string prefix, string separator = ": ", TokenBehaviour tokenBehaviour = TokenBehaviour.UseTokens)
		{
			if (string.IsNullOrWhiteSpace(prefix))
			{
				throw new ArgumentNullException(nameof(prefix));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			Prefix = prefix;
			Separator = separator;
			_tokenBehaviour = tokenBehaviour;
			_prefixId = Guid.NewGuid().ToString("N");
			_plPrefixWithId = "plPrefix" + _prefixId;
			_plPrefixSeparatorWithId = "plSeparator" + _prefixId;
			_plPrefixAndSeparatorWithId = "{" + _plPrefixWithId + "}{" + _plPrefixSeparatorWithId + "}";
		}

		public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);

		public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (_tokenBehaviour == TokenBehaviour.UseTokens && state is IEnumerable<KeyValuePair<string, object>> oldState)
			{
				var newState = new List<KeyValuePair<string, object>>
				{
					new KeyValuePair<string, object>(_plPrefixWithId, Prefix),
					new KeyValuePair<string, object>(_plPrefixSeparatorWithId, Separator)
				};
				foreach (var item in oldState)
				{
					if (item.Key == "{OriginalFormat}")
					{
						newState.Add(new KeyValuePair<string, object>(item.Key, _plPrefixAndSeparatorWithId + item.Value));
					}
					else
					{
						newState.Add(item);
					}
				}

				var message = Prefix +
					Separator +
					(formatter != null
						? formatter(state, exception)
						: state.ToString());
				_logger.Log(logLevel, eventId, newState, exception, (_, __) => message);
				return;
			}

			// Fallback to just changing the output text
			_logger.Log(
				logLevel,
				eventId,
				state,
				exception,
				(TState state, Exception ex) =>
				{
					var message = formatter != null
					? formatter(state, ex)
					: state?.ToString()
						?? string.Empty;
					return Prefix + Separator + message;
				}
			);
		}
	}
}