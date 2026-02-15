using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;

namespace K4ChatGuard.Services;

public class NameFilterService
{
	private readonly ISwiftlyCore _core;
	private readonly BlacklistService _blacklistService;
	private readonly string _replacementName;

	public NameFilterService(
		ISwiftlyCore core,
		BlacklistService blacklistService,
		string replacementName)
	{
		_core = core;
		_blacklistService = blacklistService;
		_replacementName = replacementName;

		_core.Logger.LogInformation(
			$"Name Filter: Bad names will be replaced with '{replacementName}'"
		);
	}

	public void CheckAndRenameBadName(IPlayer player)
	{
		if (player.IsFakeClient)
			return;

		var playerName = player.Controller.PlayerName;

		if (_blacklistService.ContainsBlacklistedWord(playerName, out var matchedWord))
		{
			var newName = $"{_replacementName}#{player.UserID}";

			_core.Logger.LogInformation(
				$"Renaming player '{newName}' (SteamID: {player.SteamID}) - contains blacklisted word '{matchedWord}'"
			);

			var localizer = _core.Translation.GetPlayerLocalizer(player);
			var prefix = localizer["prefix"];
			var message = localizer["name.blocked", prefix, newName];
			player.SendChat(message);

			_core.Scheduler.NextWorldUpdate(() =>
			{
				player.Controller.PlayerName = newName;
				player.Controller.PlayerNameUpdated();
			});
		}
	}
}
