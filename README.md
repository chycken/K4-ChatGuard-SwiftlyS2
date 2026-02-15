<a name="readme-top"></a>

![GitHub tag (with filter)](https://img.shields.io/github/v/tag/K4ryuu/K4-ChatGuard?style=for-the-badge&label=Version)
![GitHub Repo stars](https://img.shields.io/github/stars/K4ryuu/K4-ChatGuard?style=for-the-badge)
![GitHub issues](https://img.shields.io/github/issues/K4ryuu/K4-ChatGuard?style=for-the-badge)
![GitHub](https://img.shields.io/github/license/K4ryuu/K4-ChatGuard?style=for-the-badge)
![GitHub all releases](https://img.shields.io/github/downloads/K4ryuu/K4-ChatGuard/total?style=for-the-badge)
[![Discord](https://img.shields.io/badge/Discord-Join%20Server-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://dsc.gg/k4-fanbase)

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <h1 align="center">KitsuneLab©</h1>
  <h3 align="center">K4-ChatGuard</h3>
  <a align="center">Advanced chat protection system for Counter-Strike 2 servers. Features intelligent word filtering, IP address blocking with whitelist support, and robust spam prevention to maintain a clean and safe chat environment.</a>

  <p align="center">
    <br />
    <a href="https://github.com/K4ryuu/K4-ChatGuard/releases/latest">Download</a>
    ·
    <a href="https://github.com/K4ryuu/K4-ChatGuard/issues/new?assignees=K4ryuu&labels=bug&projects=&template=bug_report.md&title=%5BBUG%5D">Report Bug</a>
    ·
    <a href="https://github.com/K4ryuu/K4-ChatGuard/issues/new?assignees=K4ryuu&labels=enhancement&projects=&template=feature_request.md&title=%5BREQ%5D">Request Feature</a>
  </p>
</div>

### Support My Work

I create free, open-source Counter-Strike 2 plugins for the community. If you'd like to support my work, consider becoming a sponsor!

#### 💖 GitHub Sponsors

Support this project through [GitHub Sponsors](https://github.com/sponsors/K4ryuu) with flexible options:

- **One-time** or **monthly** contributions
- **Custom amount** - choose what works for you
- **Multiple tiers available** - from basic benefits to priority support or private project access

Every contribution helps me dedicate more time to development, support, and creating new features. Thank you! 🙏

<p align="center">
  <a href="https://github.com/sponsors/K4ryuu">
    <img src="https://img.shields.io/badge/sponsor-30363D?style=for-the-badge&logo=GitHub-Sponsors&logoColor=#EA4AAA" alt="GitHub Sponsors" />
  </a>
</p>

⭐ **Or support me for free by starring this repository!**

---

## Features

### 🛡️ Word Filtering (Protobuf-Based)

- Configurable blacklist with case-insensitive matching
- Admin bypass via permission system

### 🌐 IP Address Protection (Protobuf-Based)

- IPv4 detection with port support (e.g., `192.168.1.1:27015`)
- Whitelist support for legitimate IPs

### ⚡ Spam Prevention (Event-Based)

- Chat rate limiting (configurable: 3 messages per 2 seconds)
- Sliding window algorithm for accurate detection
- Spam cooldown - blocks spam attempts until player stops trying
- Automatic cleanup on player disconnect

### 👤 Name Filtering

- Automatically renames players with blocked words in their name
- Monitors name changes in real-time
- Supports placeholders: `#userid`, `#steamid`

## Technical Architecture

### Why Two Different Approaches?

**Spam Protection (Event-Based)** uses `ClientChatHookHandler`:

- Blocks messages **before** they enter the chat system
- Prevents commands from being forwarded to other plugins for processing
- Ideal for rate limiting where you want to completely stop message flow

**Word & IP Filtering (Protobuf-Based)** uses `CUserMessageSayText2`:

- Filters messages **during** processing
- Allows other plugins to still process the chat event
- Better for content filtering where plugin integration matters

### 🔐 Permission System

| Permission                   | Description                  |
| ---------------------------- | ---------------------------- |
| `chatguard.bypass.all`       | Bypass all chat filters      |
| `chatguard.bypass.blacklist` | Bypass word blacklist filter |
| `chatguard.bypass.ipfilter`  | Bypass IP address filter     |
| `chatguard.bypass.spam`      | Bypass spam protection       |

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## Dependencies

- [**SwiftlyS2**](https://github.com/swiftly-solution/swiftlys2): Server plugin framework for Counter-Strike 2

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## Installation

1. Install [SwiftlyS2](https://github.com/swiftly-solution/swiftlys2) on your server
2. [Download the latest release](https://github.com/K4ryuu/K4-ChatGuard/releases/latest)
3. Extract to your server's `swiftlys2/plugins/` directory
4. Configure `config.json` and translations in the plugin folder
5. Restart your server or use `css_plugins load K4-ChatGuard`

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## Configuration

### Main Configuration (`config.json`)

```json
{
  "K4ChatGuard": {
    "BlacklistedWords": ["fuck", "dick", "badword", "offensive", "spam"],
    "BlockIpAddresses": true,
    "WhitelistedIPs": ["127.0.0.1", "localhost"],
    "BlockBadNames": true,
    "BlockedNameReplacement": "Player#userid",
    "SpamProtection": {
      "Enabled": true,
      "MaxMessages": 3,
      "TimeWindowSeconds": 2
    }
  }
}
```

### Configuration Options

#### Word Filtering

- **BlacklistedWords** - Array of words to block (case-insensitive)

#### IP Filtering

- **BlockIpAddresses** - Enable/disable IP address filtering
- **WhitelistedIPs** - Array of allowed IP addresses

#### Name Filtering

- **BlockBadNames** - Enable/disable automatic name filtering
- **BlockedNameReplacement** - Replacement name (supports `#userid`, `#steamid`)

#### Spam Protection

- **Enabled** - Enable/disable spam protection
- **MaxMessages** - Maximum messages allowed in the time window
- **TimeWindowSeconds** - Time window for spam detection (seconds)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## Translations

Messages are customizable in `resources/translations/en.jsonc`. Supports color codes like `[red]`, `[silver]`, `[gold]`, etc.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## License

Distributed under the GPL-3.0 License. See `LICENSE.md` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## Contact

- **Discord**: Join our [K4 Fanbase Discord](https://dsc.gg/k4-fanbase)
- **Issues**: [GitHub Issues](https://github.com/K4ryuu/K4-ChatGuard/issues)

<p align="right">(<a href="#readme-top">back to top</a>)</p>
