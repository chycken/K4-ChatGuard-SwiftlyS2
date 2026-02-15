using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;

namespace K4ChatGuard.Services;

public class BlacklistService
{
	private readonly ISwiftlyCore _core;
	private readonly List<string> _blacklistedWords;

	public BlacklistService(ISwiftlyCore core, List<string> blacklistedWords)
	{
		_core = core;
		_blacklistedWords = [.. blacklistedWords.Select(w => w.ToLowerInvariant())];

		_core.Logger.LogInformation($"Loaded {_blacklistedWords.Count} blacklisted words");
	}

	public bool ContainsBlacklistedWord(string message, out string? matchedWord)
	{
		matchedWord = null;

		if (_blacklistedWords.Count == 0)
			return false;

		var lowerMessage = message.ToLowerInvariant();

		foreach (var word in _blacklistedWords)
		{
			if (lowerMessage.Contains(word))
			{
				matchedWord = word;
				return true;
			}
		}

		return false;
	}
}
