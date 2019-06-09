using Microsoft.Extensions.Logging;

namespace PanoramicSystems.PrefixLogger
{
	public class PrefixLoggerFactory : ILoggerFactory
	{
		private readonly string _prefix;
		private readonly ILoggerFactory _loggerFactory;

		private bool disposed;

		public PrefixLoggerFactory(string prefix, ILoggerFactory loggerFactory)
		{
			_prefix = prefix;
			_loggerFactory = loggerFactory;
		}

		public void AddProvider(ILoggerProvider provider)
			=> _loggerFactory.AddProvider(provider);

		public ILogger CreateLogger(string categoryName)
			=> new PrefixLogger(_prefix, _loggerFactory.CreateLogger(categoryName));

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					_loggerFactory.Dispose();
				}
				disposed = true;
			}
		}

		public void Dispose() => Dispose(true);
	}
}
