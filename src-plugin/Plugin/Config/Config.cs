namespace K4ChatGuard.Config;

public class PluginConfig
{
	public List<string> BlacklistedWords { get; set; } = ["fuck", "dick", "badword", "offensive", "spam"];
	public bool BlockIpAddresses { get; set; } = true;
	public List<string> WhitelistedIPs { get; set; } = ["127.0.0.1", "localhost"];
	public bool BlockBadNames { get; set; } = true;
	public string BlockedNameReplacement { get; set; } = "Blocked";
	public SpamProtectionConfig SpamProtection { get; set; } = new();
}

public class SpamProtectionConfig
{
	public bool Enabled { get; set; } = true;
	public int MaxMessages { get; set; } = 3;
	public int TimeWindowSeconds { get; set; } = 2;
}
