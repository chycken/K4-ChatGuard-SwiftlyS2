using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;

namespace K4ChatGuard.Services;

public class SpamProtectionService
{
	private readonly ISwiftlyCore _core;
	private readonly ConcurrentDictionary<ulong, Queue<DateTime>> _playerData = new();
	private readonly ConcurrentDictionary<ulong, DateTime> _lastSpamAttempt = new();
	private readonly int _maxMessages;
	private readonly int _timeWindowSeconds;

	public SpamProtectionService(
		ISwiftlyCore core,
		int maxMessages,
		int timeWindowSeconds)
	{
		_core = core;
		_maxMessages = maxMessages;
		_timeWindowSeconds = timeWindowSeconds;

		_core.Logger.LogInformation(
			$"Spam Protection: {maxMessages} messages per {timeWindowSeconds}s"
		);
	}

	public bool IsChatSpam(ulong steamId)
	{
		var data = _playerData.GetOrAdd(steamId, _ => new Queue<DateTime>());
		var now = DateTime.UtcNow;

		// Check if player is in cooldown from previous spam
		if (_lastSpamAttempt.TryGetValue(steamId, out var lastSpam))
		{
			var cooldownEnd = lastSpam.AddSeconds(_timeWindowSeconds);
			if (now < cooldownEnd)
			{
				// Still in cooldown, update last spam attempt to extend cooldown
				_lastSpamAttempt[steamId] = now;
				return true;
			}
			else
			{
				// Cooldown expired, clear spam data
				_lastSpamAttempt.TryRemove(steamId, out _);
				data.Clear();
			}
		}

		var windowStart = now.AddSeconds(-_timeWindowSeconds);

		// Remove old messages outside the time window
		while (data.Count > 0 && data.Peek() < windowStart)
		{
			data.Dequeue();
		}

		// Check if spam limit exceeded
		if (data.Count >= _maxMessages)
		{
			// Mark as spam and start cooldown
			_lastSpamAttempt[steamId] = now;
			return true;
		}

		// Add current message
		data.Enqueue(now);
		return false;
	}

	public void ClearPlayerData(ulong steamId)
	{
		_playerData.TryRemove(steamId, out _);
		_lastSpamAttempt.TryRemove(steamId, out _);
	}
}
