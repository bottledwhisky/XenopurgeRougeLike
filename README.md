# XenopurgeRougelike

A rogue-like mod that adds a reinforcement system with unique company paths, making each playthrough different.

A MelonLoader mod for Xenopurge.

Source code is available on [GitHub](https://github.com/bottledwhisky/XenopurgeRougeLike).

## Features

- **7 Company Paths**: Choose reinforcements from specialized companies, each with distinct playstyles:
  - **Engineer**: Mines, grenades, turrets, and explosives specialization
  - **Support**: Medical supplies, buffs, healing, and scavenging
  - **Warrior**: Melee combat, shotguns, armor, and close-range dominance
  - **Gunslinger**: Ranged precision, critical hits, and suppression tactics
  - **Scavenger**: Exploration, item collection, economy, and pistol/knife expertise
  - **Rockstar**: Fans system, bonus income, shop enhancements, and NPC allies
  - **Synthetics**: High stats when healthy, hacking bonuses, and smart weapons
  - **Xeno**: Mind control, stun abilities, and anti-xeno damage bonuses
- **Progressive Affinity System**: Unlock stronger bonuses as you collect 2/4/6 reinforcements from the same company
- **Diverse Reinforcements**: 80+ unique reinforcements with varying rarities (Common/Elite/Expert) that modify gameplay mechanics
- **Universal Reinforcements**: Generic stat boosts and weapon specializations available to all paths

## Requirements

- [MelonLoader](https://melonwiki.xyz/)

## Installation

Skip to step 2 if you already have MelonLoader installed.

1. Install MelonLoader
    a. `<game_directory>` is the directory where the game executable is located. For example, `C:\Program Files (x86)\Steam\steamapps\common\Xenopurge`. If you still cannot find it, right-click the game in your Steam library, select "Manage", then "Browse local files".
2. Unzip XenopurgeRougeLike-x.x.zip in `<game_directory>`
3. Restart the game

## Uninstall

1. Delete the mod DLL XenopurgeRougeLike.dll from `<game_directory>/Mods/`
2. Delete `<game_directory>/UserData/XenopurgeRougeLike`

## How It Works

- After each mission, choose from 3 random reinforcements to enhance your squad
- Build synergies by collecting reinforcements from the same company path
- Company affinity bonuses activate at 2, 4, and 6 reinforcements from the same path
- Each run creates a unique build based on your reinforcement choices

## Notes

- Mac users: MelonLoader only supports Windows and Linux. Wait for Steam Workshop support.
- Xenopurge uses Mono, not IL2CPP.
