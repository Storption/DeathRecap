# DeathRecap

An [EXILED](https://github.com/ExMod-Team/EXILED) plugin for SCP: Secret Laboratory that shows a player a recap of their death once they enter spectator.

[![Downloads](https://img.shields.io/github/downloads/Storption/DeathRecap/total?style=for-the-badge&logo=github&color=blue)](https://github.com/Storption/DeathRecap/releases/latest)
[![Latest](https://img.shields.io/github/v/release/Storption/DeathRecap?include_prereleases&style=for-the-badge&logo=github&label=Latest%20Release&color=green)](https://github.com/Storption/DeathRecap/releases/latest)
[![Discord](https://img.shields.io/discord/1114170053949128817?style=for-the-badge&color=5865F2&logo=discord&label=Discord&logoColor=white)](https://join.storption.com)

## How it works
 
When a player dies, they're shown who killed them, the weapon used, the distance of the killing blow, how much total damage the killer dealt to them, and how much damage they managed to deal back before dying. The killer's name uses their badge color when they have one, falling back to their role's color otherwise.
 
By default, the recap stays on screen for as long as the player remains spectating that life — it disappears the moment they respawn or the round ends. This can be changed to auto-hide after a fixed number of seconds instead, via config.
 
**Auto-update** - checks this plugin's own GitHub repo for a newer release, and if found, downloads and applies it automatically, restarting the server once the current round ends.

## Requirememts

- [EXILED](https://github.com/ExMod-Team/EXILED) 9.14.2 or later

## Installation

1. Download the latest `DeathRecap.dll` from the [Releases](https://github.com/Storption/DeathRecap/releases) page.
2. Place it in your server's EXILED plugins folder (`%AppData%\EXILED\Plugins` on Windows `.config\EXILED\Plugins` on Linux).
3. Restart your server. A default config will be generated on first load.

## Config

```yaml
# Whether the plugin is enabled.
is_enabled: true
# Whether debug messages are shown.
debug: false
# How long, in seconds, the recap stays visible. 0 means it stays until the player leaves spectator or the round ends.
recap_duration_seconds: 0
# How many blank lines to pad the recap hint with, controlling its vertical position on screen.
hint_line_padding: 15
# The recap text's size, as a percentage of the default hint size.
hint_text_size_percent: 80
# Whether to check for and automatically install updates.
auto_update_enabled: true
# Whether to keep a backup of the previous .dll before replacing it with an update.
auto_update_backup: true
# Whether to automatically restart the server once the current round ends, to apply a downloaded update. Never restarts mid-round.
auto_update_restart: true
```

The full recap text - including the exact layout and wording - is configurable via the generated translation file.

## Showcase
<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/872b4dbb-6bd1-431a-a7d2-7dd5ebaaf52c" />
