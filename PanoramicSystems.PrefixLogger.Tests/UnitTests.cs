using Divergic.Logging.Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace PanoramicSystems.PrefixLoggerTests
{
	public class UnitTests
	{
		private readonly ICacheLogger<UnitTests> _logger;

		public UnitTests(ITestOutputHelper output)
		{
			_logger = output.BuildLoggerFor<UnitTests>();
		}

		[Fact]
		public void TestLoggerWithState()
		{
			var prefixLogger = new PrefixLogger(_logger, "LOGX", tokenBehaviour: TokenBehaviour.UsePlainText);
			using (var scope = prefixLogger!.BeginScope("TestScope {ScopeName}", new[] { "scope1" }))
			{
				prefixLogger.LogInformation("Hi there {Name}", "bob");
			}

			using (var scope = prefixLogger!.BeginScope(new Dictionary<string, object> { { "OrganizationId", 123 } }))
			{
				prefixLogger.LogInformation("Hi there again {Name}", "bob");
			}

			prefixLogger.LogInformation("Hi there {Name}", "kate");
			//_ = _logger.Last!
		}

		[Theory]
		[InlineData("ThePrefix", null, "The message", "ThePrefix: The message")]
		[InlineData("The Prefix", null, "Another message", "The Prefix: Another message")]
		[InlineData("ThePrefix", "-", "The message", "ThePrefix-The message")]
		[InlineData("ThePrefix", " # ", "The message", "ThePrefix # The message")]
		[InlineData("ThePrefix", null, "", "ThePrefix: ")]
		[InlineData("ThePrefix", null, null, "ThePrefix: (null)")]
		public void BasicStringTestsDefaultBehaviourWithTokens(string prefix, string? separator, string? input, string expectedOutput)
		{
			var prefixLogger = separator == null
				? new PrefixLogger(_logger, prefix)
				: new PrefixLogger(_logger, prefix, separator);
			prefixLogger.LogInformation("{Message}", input);
			_ = _logger.Entries.Should().HaveCount(1);
			_ = _logger.Last.Should().NotBeNull();
			_ = _logger.Last!.Message.Should().Be(expectedOutput);
			var state = _logger.Last!.State as IEnumerable<KeyValuePair<string, object>>;
			_ = state.Should().HaveCount(4);
			_ = state.Should().ContainKey("{OriginalFormat}").WhoseValue.Should().Be($"{{{prefixLogger._plPrefixWithId}}}{{{prefixLogger._plPrefixSeparatorWithId}}}{{Message}}");
			_ = state.Should().ContainKey("Message").WhoseValue.Should().Be(input);
			_ = state.Should().ContainKey(prefixLogger._plPrefixWithId).WhoseValue.Should().Be(prefix);
			_ = state.Should().ContainKey(prefixLogger._plPrefixSeparatorWithId).WhoseValue.Should().Be(separator ?? ": ");
		}

		[Theory]
		[InlineData("ThePrefix", null, "The message", "ThePrefix: The message")]
		[InlineData("The Prefix", null, "Another message", "The Prefix: Another message")]
		[InlineData("ThePrefix", "-", "The message", "ThePrefix-The message")]
		[InlineData("ThePrefix", " # ", "The message", "ThePrefix # The message")]
		[InlineData("ThePrefix", null, "", "ThePrefix: ")]
		[InlineData("ThePrefix", null, null, "ThePrefix: (null)")]
		public void BasicStringTestsDefaultPlainText(string prefix, string? separator, string? input, string expectedOutput)
		{
			var prefixLogger = separator == null
				? new PrefixLogger(_logger, prefix, tokenBehaviour: TokenBehaviour.UsePlainText)
				: new PrefixLogger(_logger, prefix, separator, tokenBehaviour: TokenBehaviour.UsePlainText);
			prefixLogger.LogInformation("{Message}", input);
			_ = _logger.Entries.Should().HaveCount(1);
			_ = _logger.Last.Should().NotBeNull();
			_ = _logger.Last!.Message.Should().Be(expectedOutput);
		}

		[Fact]
		public void FormattedTest_1Parameter_StateIsCorrect()
		{
			var prefixLogger = new PrefixLogger(_logger, "ThePrefix");
			prefixLogger.LogInformation("The first number is {firstNumber}.", 42);
			_ = _logger.Entries.Should().HaveCount(1);
			_ = _logger.Last.Should().NotBeNull();
			_ = _logger.Last!.Message.Should().Be("ThePrefix: The first number is 42.");
			var state = _logger.Last!.State as IEnumerable<KeyValuePair<string, object>>;
			_ = state.Should().HaveCount(4);
			_ = state.Should().ContainKey("firstNumber").WhoseValue.Should().Be(42);
			_ = state.Should().ContainKey("{OriginalFormat}").WhoseValue.Should().Be($"{{{prefixLogger._plPrefixWithId}}}{{{prefixLogger._plPrefixSeparatorWithId}}}The first number is {{firstNumber}}.");
			_ = state.Should().ContainKey(prefixLogger._plPrefixWithId).WhoseValue.Should().Be("ThePrefix");
			_ = state.Should().ContainKey(prefixLogger._plPrefixSeparatorWithId).WhoseValue.Should().Be(": ");
		}

		[Fact]
		public void FormattedTest_2Parameters_StateIsCorrect()
		{
			var prefixLogger = new PrefixLogger(_logger, "ThePrefix");
			prefixLogger.LogInformation("The first number is {firstNumber}, the second number is {secondNumber}.", 42, 69);
			_ = _logger.Entries.Should().HaveCount(1);
			_ = _logger.Last.Should().NotBeNull();
			_ = _logger.Last!.Message.Should().Be("ThePrefix: The first number is 42, the second number is 69.");
			var state = _logger.Last!.State as IEnumerable<KeyValuePair<string, object>>;
			_ = state.Should().HaveCount(5);
			_ = state.Should().ContainKey("firstNumber").WhoseValue.Should().Be(42);
			_ = state.Should().ContainKey("secondNumber").WhoseValue.Should().Be(69);
			_ = state.Should().ContainKey("{OriginalFormat}").WhoseValue.Should().Be($"{{{prefixLogger._plPrefixWithId}}}{{{prefixLogger._plPrefixSeparatorWithId}}}The first number is {{firstNumber}}, the second number is {{secondNumber}}.");
			_ = state.Should().ContainKey(prefixLogger._plPrefixWithId).WhoseValue.Should().Be("ThePrefix");
			_ = state.Should().ContainKey(prefixLogger._plPrefixSeparatorWithId).WhoseValue.Should().Be(": ");
		}

		[Fact]
		public void MultiplePrefixLoggers_StateIsCorrect()
		{
			var firstPrefixLogger = _logger.PrefixedWith("FirstPrefix");
			var plPrefix1WithId = firstPrefixLogger._plPrefixWithId;
			var plSeparator1WithId = firstPrefixLogger._plPrefixSeparatorWithId;
			var secondPrefixLogger = firstPrefixLogger.PrefixedWith("SecondPrefix", "# ");
			var plPrefix2WithId = secondPrefixLogger._plPrefixWithId;
			var plSeparator2WithId = secondPrefixLogger._plPrefixSeparatorWithId;
			secondPrefixLogger.LogInformation("The answer is, of course, {firstNumber}.", 42);
			_ = _logger.Entries.Should().HaveCount(1);
			_ = _logger.Last.Should().NotBeNull();
			_ = _logger.Last!.Message.Should().Be("FirstPrefix: SecondPrefix# The answer is, of course, 42.");
			var state = _logger.Last!.State as IEnumerable<KeyValuePair<string, object>>;
			_ = state.Should().HaveCount(6);
			_ = state.Should().ContainKey("firstNumber").WhoseValue.Should().Be(42);
			_ = state.Should().ContainKey("{OriginalFormat}").WhoseValue.Should().Be($"{{{plPrefix1WithId}}}{{{plSeparator1WithId}}}{{{plPrefix2WithId}}}{{{plSeparator2WithId}}}The answer is, of course, {{firstNumber}}.");
			_ = state.Should().ContainKey(plPrefix1WithId).WhoseValue.Should().Be("FirstPrefix");
			_ = state.Should().ContainKey(plSeparator1WithId).WhoseValue.Should().Be(": ");
			_ = state.Should().ContainKey(plPrefix2WithId).WhoseValue.Should().Be("SecondPrefix");
			_ = state.Should().ContainKey(plSeparator2WithId).WhoseValue.Should().Be("# ");
		}
	}
}