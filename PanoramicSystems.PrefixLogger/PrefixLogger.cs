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

        public PrefixLogger(ILogger logger, string prefix) : this(logger, prefix, ": ")
        {
        }

        public PrefixLogger(ILogger logger, string prefix, string separator)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentNullException(nameof(prefix));
            }
            Prefix = prefix;
            Separator = separator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState originalState, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (originalState is IEnumerable<KeyValuePair<string, object>> oldState)
            {
                var newState = new List<KeyValuePair<string, object>>();
                foreach (var item in oldState)
                {
                    if (item.Key == "{OriginalFormat}")
                    {
                        newState.Add(new KeyValuePair<string, object>(item.Key, "{plPrefix}{plSeparator}" + item.Value));
                    }
                    else
                    {
                        newState.Add(item);
                    }
                }
                newState.Add(new KeyValuePair<string, object>("plPrefix", Prefix));
                newState.Add(new KeyValuePair<string, object>("plSeparator", Separator));
                var message = Prefix +
                    Separator +
                    (formatter != null
                        ? formatter(originalState, exception)
                        : originalState?.ToString()
                            ?? string.Empty);
                _logger.Log(logLevel, eventId, newState, exception, (_, exception) =>
                    message);
                return;
            }

            // Fallback to just changing the output text
            _logger.Log(
                logLevel,
                eventId,
                originalState,
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

            //_logger.Log(
            //    logLevel,
            //    eventId,
            //    state,
            //    exception,
            //    (TState state, Exception ex) =>
            //    {
            //        var message = formatter != null
            //        ? formatter(state, exception)
            //        : state?.ToString()
            //            ?? string.Empty;
            //        return Prefix + Separator + message;
            //    }
            //);
        }
    }
}
