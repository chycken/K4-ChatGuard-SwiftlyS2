using K4ChatGuard.Config;
using K4ChatGuard.Events;
using K4ChatGuard.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.NetMessages;
using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared.ProtobufDefinitions;

namespace K4ChatGuard;

[PluginMetadata(Id = "k4.chatguard", Version = "1.0.0", Name = "K4 - ChatGuard", Author = "K4ryuu", Description = "Advanced chat protection with blacklist filtering, IP blocking, and spam prevention for CS2.")]
public sealed partial class Plugin(ISwiftlyCore core) : BasePlugin(core)
{
	public static new ISwiftlyCore Core { get; private set; } = null!;

	internal IOptionsMonitor<PluginConfig> Config { get; private set; } = null!;
	internal BlacklistService BlacklistService { get; private set; } = null!;
	internal IpFilterService IpFilterService { get; private set; } = null!;
	internal SpamProtectionService SpamService { get; private set; } = null!;
	internal NameFilterService? NameFilterService { get; private set; }

	private ChatEvents? _chatEvents;

	public override void Load(bool hotReload)
	{
		Core = base.Core;

		// Initialize config
		Core.Configuration.InitializeJsonWithModel<PluginConfig>("config.json", "K4ChatGuard");

		ServiceCollection services = new();
		services.AddSwiftly(Core)
			.AddOptions<PluginConfig>()
			.BindConfiguration("K4ChatGuard");

		var provider = services.BuildServiceProvider();
		Config = provider.GetRequiredService<IOptionsMonitor<PluginConfig>>();

		// Initialize services
		BlacklistService = new BlacklistService(Core, Config.CurrentValue.BlacklistedWords);
		IpFilterService = new IpFilterService(Core, Config.CurrentValue.WhitelistedIPs);
		SpamService = new SpamProtectionService(
			Core,
			Config.CurrentValue.SpamProtection.MaxMessages,
			Config.CurrentValue.SpamProtection.TimeWindowSeconds
		);

		if (Config.CurrentValue.BlockBadNames)
		{
			NameFilterService = new NameFilterService(
				Core,
				BlacklistService,
				Config.CurrentValue.BlockedNameReplacement
			);
		}

		// Register events
		_chatEvents = new ChatEvents(Core, Config, BlacklistService, IpFilterService, SpamService, NameFilterService);
	}

	public override void Unload()
	{
		// Cleanup if needed
	}

	[ServerNetMessageHandler]
	public HookResult OnChatMessage(CUserMessageSayText2 msg)
	{
		return _chatEvents!.HandleChatMessage(msg);
	}
}
