using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;

namespace K4ChatGuard.Services;

public partial class IpFilterService
{
	private readonly ISwiftlyCore _core;
	private readonly List<string> _whitelistedIPs;

	// IPv4 pattern: matches 0.0.0.0 to 255.255.255.255 with optional port (e.g., 192.168.1.1:27015)
	[GeneratedRegex(@"\b(?:\d{1,3}\.){3}\d{1,3}(?::\d{1,5})?\b")]
	private static partial Regex IPv4Pattern();

	public IpFilterService(ISwiftlyCore core, List<string> whitelistedIPs)
	{
		_core = core;
		_whitelistedIPs = whitelistedIPs;
		_core.Logger.LogInformation($"IP Filter Service initialized (IPv4 only, {whitelistedIPs.Count} whitelisted)");
	}

	public bool ContainsIpAddress(string message, out string? detectedIp)
	{
		detectedIp = null;

		// Check for IPv4
		if (IPv4Pattern().IsMatch(message))
		{
			var match = IPv4Pattern().Match(message);
			var ipWithPort = match.Value;

			// Extract IP without port
			var ip = ipWithPort.Split(':')[0];

			if (!IsValidIPv4(ip))
				return false;

			// Check if IP is whitelisted
			if (_whitelistedIPs.Contains(ip) || _whitelistedIPs.Contains("localhost") && ip == "127.0.0.1")
			{
				return false;
			}

			detectedIp = ipWithPort;
			return true;
		}

		return false;
	}

	private static bool IsValidIPv4(string ip)
	{
		var parts = ip.Split('.');
		if (parts.Length != 4)
			return false;

		foreach (var part in parts)
		{
			if (!int.TryParse(part, out int num) || num < 0 || num > 255)
				return false;
		}

		return true;
	}
}
