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

		[Theory]
		[InlineData("ThePrefix", null, "The message", "ThePrefix: The message")]
		[InlineData("The Prefix", null, "Another message", "The Prefix: Another message")]
		[InlineData("ThePrefix", "-", "The message", "ThePrefix-The message")]
		[InlineData("ThePrefix", " # ", "The message", "ThePrefix # The message")]
		[InlineData("ThePrefix", null, "", "ThePrefix: ")]
		[InlineData("ThePrefix", null, null, "ThePrefix: (null)")]
		public void BasicStringTests(string prefix, string? separator, string? input, string expectedOutput)
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
			_ = state.Should().ContainKey("{OriginalFormat}").WhoseValue.Should().Be("{plPrefix}{plSeparator}{Message}");
			_ = state.Should().ContainKey("Message").WhoseValue.Should().Be(input);
			_ = state.Should().ContainKey("plPrefix").WhoseValue.Should().Be(prefix);
			_ = state.Should().ContainKey("plSeparator").WhoseValue.Should().Be(separator ?? ": ");
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
			_ = state.Should().ContainKey("{OriginalFormat}").WhoseValue.Should().Be("{plPrefix}{plSeparator}The first number is {firstNumber}, the second number is {secondNumber}.");
			_ = state.Should().ContainKey("plPrefix").WhoseValue.Should().Be("ThePrefix");
			_ = state.Should().ContainKey("plSeparator").WhoseValue.Should().Be(": ");
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
			_ = state.Should().ContainKey("{OriginalFormat}").WhoseValue.Should().Be("{plPrefix}{plSeparator}The first number is {firstNumber}.");
			_ = state.Should().ContainKey("plPrefix").WhoseValue.Should().Be("ThePrefix");
			_ = state.Should().ContainKey("plSeparator").WhoseValue.Should().Be(": ");
		}
	}
}