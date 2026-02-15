using K4ChatGuard.Config;
using K4ChatGuard.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.ProtobufDefinitions;

namespace K4ChatGuard.Events;

public class ChatEvents
{
	private readonly ISwiftlyCore _core;
	private readonly IOptionsMonitor<PluginConfig> _config;
	private readonly BlacklistService _blacklistService;
	private readonly IpFilterService _ipFilterService;
	private readonly SpamProtectionService _spamService;
	private readonly NameFilterService? _nameFilterService;

	public ChatEvents(
		ISwiftlyCore core,
		IOptionsMonitor<PluginConfig> config,
		BlacklistService blacklistService,
		IpFilterService ipFilterService,
		SpamProtectionService spamService,
		NameFilterService? nameFilterService = null)
	{
		_core = core;
		_config = config;
		_blacklistService = blacklistService;
		_ipFilterService = ipFilterService;
		_spamService = spamService;
		_nameFilterService = nameFilterService;

		core.Registrator.Register(this);

		if (_config.CurrentValue.BlockBadNames && _nameFilterService != null)
		{
			core.GameEvent.HookPost<EventPlayerChangename>(OnPlayerChangeName);
			core.GameEvent.HookPost<EventPlayerActivate>(OnClientActivated);
		}
	}

	[ClientChatHookHandler]
	public HookResult OnClientChat(int playerId, string text, bool teamonly)
	{
		var player = _core.PlayerManager.GetPlayer(playerId);
		if (player == null || player.IsFakeClient)
			return HookResult.Continue;

		// Permission bypass check
		bool bypassAll = _core.Permission.PlayerHasPermission(player.SteamID, "chatguard.bypass.all");
		bool bypassSpam = bypassAll || _core.Permission.PlayerHasPermission(player.SteamID, "chatguard.bypass.spam");

		// Check spam protection (event-based)
		if (!bypassSpam && _config.CurrentValue.SpamProtection.Enabled && _spamService.IsChatSpam(player.SteamID))
		{
			var localizer = _core.Translation.GetPlayerLocalizer(player);
			var prefix = localizer["prefix"];
			var message = localizer["spam.chat.warning", prefix];
			player.SendChat(message);

			_core.Logger.LogInformation($"Chat spam detected from {player.Controller.PlayerName} ({player.SteamID})");
			return HookResult.Stop;
		}

		return HookResult.Continue;
	}

	public HookResult OnClientActivated(EventPlayerActivate @event)
	{
		if (!_config.CurrentValue.BlockBadNames || _nameFilterService == null)
			return HookResult.Continue;

		var player = _core.PlayerManager.GetPlayer(@event.UserId);
		if (player == null)
			return HookResult.Continue;

		_nameFilterService.CheckAndRenameBadName(player);
		return HookResult.Continue;
	}

	[EventListener<EventDelegates.OnClientDisconnected>]
	public void OnClientDisconnected(IOnClientDisconnectedEvent e)
	{
		var player = _core.PlayerManager.GetPlayer(e.PlayerId);
		if (player == null)
			return;

		_spamService.ClearPlayerData(player.SteamID);
	}

	public HookResult OnPlayerChangeName(EventPlayerChangename e)
	{
		var player = e.UserIdPlayer;
		if (player == null)
			return HookResult.Continue;

		_nameFilterService!.CheckAndRenameBadName(player);
		return HookResult.Continue;
	}

	public HookResult HandleChatMessage(CUserMessageSayText2 msg)
	{
		if (msg.Entityindex <= 0)
			return HookResult.Continue;

		var player = _core.PlayerManager.GetPlayer(msg.Entityindex - 1);
		if (player == null || player.IsFakeClient)
			return HookResult.Continue;

		var text = msg.Param2;

		// Permission bypass checks
		bool bypassAll = _core.Permission.PlayerHasPermission(player.SteamID, "chatguard.bypass.all");
		bool bypassBlacklist = bypassAll || _core.Permission.PlayerHasPermission(player.SteamID, "chatguard.bypass.blacklist");
		bool bypassIpFilter = bypassAll || _core.Permission.PlayerHasPermission(player.SteamID, "chatguard.bypass.ipfilter");

		var localizer = _core.Translation.GetPlayerLocalizer(player);
		var prefix = localizer["prefix"];

		// Check blacklisted words (protobuf-based filter)
		if (!bypassBlacklist && _blacklistService.ContainsBlacklistedWord(text, out var matchedWord))
		{
			var message = localizer["blacklist.blocked", prefix];
			player.SendChat(message);

			// Censor the word in logs
			_core.Logger.LogInformation($"Blocked blacklisted word '{matchedWord}' from {player.Controller.PlayerName} ({player.SteamID})");
			return HookResult.Stop;
		}

		// Check IP addresses (protobuf-based filter)
		if (!bypassIpFilter && _config.CurrentValue.BlockIpAddresses && _ipFilterService.ContainsIpAddress(text, out var detectedIp))
		{
			var message = localizer["ipfilter.blocked", prefix];
			player.SendChat(message);

			// Censor the IP in logs
			_core.Logger.LogInformation($"Blocked IP address '{detectedIp}' from {player.Controller.PlayerName} ({player.SteamID})");
			return HookResult.Stop;
		}

		return HookResult.Continue;
	}
}
