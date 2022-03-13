using Microsoft.Extensions.Logging;
using System;

namespace PanoramicSystems
{
	public sealed class PrefixLoggerFactory : ILoggerFactory
	{
		public string Prefix { get; set; }
		public string Separator { get; set; }
		private readonly ILoggerFactory _loggerFactory;
		private bool isDisposed;

		public PrefixLoggerFactory(ILoggerFactory loggerFactory, string prefix) : this(loggerFactory, prefix, ": ")
		{ }

		public PrefixLoggerFactory(ILoggerFactory loggerFactory, string prefix, string separator)
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

			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
		}

		public void AddProvider(ILoggerProvider provider)
			=> _loggerFactory.AddProvider(provider);

		public ILogger CreateLogger(string categoryName)
			=> new PrefixLogger(_loggerFactory.CreateLogger(categoryName), Prefix, Separator);

		private void Dispose(bool disposing)
		{
			if (!isDisposed)
			{
				if (disposing)
				{
					_loggerFactory.Dispose();
				}

				isDisposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
