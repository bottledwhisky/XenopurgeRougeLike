This is a game mod project.
You are encouraged to read the existing Reinforcement sub-classes to quickly get an example of what we are working on. Existing code already contains common patterns like how to run code upon a mission start/end, or how to change stats, or test unit type conditions.
The decompiled source code of the original game is available here: D:\projects\xenopurge\old
In D:\projects\xenopurge\old, each folder contains a "README" text file that tells you which functionalities are implemented in that folder, and in which files. You should read those README files first to understand the code structure to avoid getting lost.
I might use Chinese to communicate with you, but the code is mostly in English.

About this mod:
This mod adds a rouge-like system to the game, allowing players to call for reinforcements after each mission. The reinforcements have unique attributes and abilities, making each playthrough different.

About the original game:
The original game is called "Xenopurge". It is a tactical RTS where players control a squad of soldiers fighting against aliens(xenoes). During battle, the game allows the player to use "ActionCards" to perform actions, not direct control.

D:\projects\xenopurge\old> dir
    Directory: D:\projects\xenopurge\old

Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
d----            2026/1/3    17:44                .claude
d----            2026/1/3    17:07                SpaceCommander
d----            2026/1/3    17:11                SpaceCommander.Abilities
d----          2025/12/25    21:30                SpaceCommander.Achievements
d----            2026/1/3    17:15                SpaceCommander.ActionCards
d----            2026/1/3    17:22                SpaceCommander.Area
d----          2025/12/25    21:30                SpaceCommander.Area.BSP
d----          2025/12/25    21:30                SpaceCommander.Area.Drawers
d----          2025/12/25    21:30                SpaceCommander.Audio
d----            2026/1/3    17:26                SpaceCommander.BattleManagement
d----            2026/1/3    17:27                SpaceCommander.BattleManagement.UI
d----          2025/12/25    21:30                SpaceCommander.Camera
d----          2025/12/25    21:30                SpaceCommander.CommanderStatus.UI
d----            2026/1/3    17:32                SpaceCommander.Commands
d----          2025/12/25    21:30                SpaceCommander.Commands.SpaceCommander.Commands
d----          2025/12/25    21:30                SpaceCommander.CustomInputControls
d----            2026/1/3    17:35                SpaceCommander.Database
d----            2026/1/3    17:38                SpaceCommander.Difficulties
d----            2026/1/3    17:39                SpaceCommander.EndGame
d----            2026/1/3    17:41                SpaceCommander.GameFlow
d----            2026/1/3    17:41                SpaceCommander.HomeScreen.UI
d----          2025/12/25    21:30                SpaceCommander.InteractionSystem
d----          2025/12/25    21:30                SpaceCommander.LogsScreen.UI
d----            2026/1/3    17:42                SpaceCommander.Objectives
d----            2026/1/3    17:44                SpaceCommander.PartyCustomization
d----          2025/12/25    21:30                SpaceCommander.PartyCustomization.UI
d----            2026/1/3    17:44                SpaceCommander.PersistentProgression
d----            2026/1/3    17:44                SpaceCommander.Progression
d----          2025/12/25    21:30                SpaceCommander.ScenesFlow
d----          2025/12/25    21:30                SpaceCommander.ScreensSystem
d----          2025/12/25    21:30                SpaceCommander.SquadSelection.UI
d----          2025/12/25    21:30                SpaceCommander.Tests
d----          2025/12/25    21:30                SpaceCommander.Tutorial
d----            2026/1/3    17:46                SpaceCommander.UI
d----          2025/12/25    21:30                SpaceCommander.Utilities
d----          2025/12/25    21:30                SpaceCommander.Variants
d----            2026/1/3    17:46                SpaceCommander.Weapons
-a---            2026/1/3    17:19            161 README

Keywords:
    增援(reinforcement) is a term only used in the mod, not in the original game.
    合成人(synthetics) is a kind of unit in the original game. The player can get a squad of all synthetic units to fight against aliens.
    装备(equipment) is a general term for items that can be equipped by units, including ranged weapons, melee weapons, and gears.
    公司(company) is a mod-only flavor term for the player's faction(流派) or path(路线) of the reinforcements. For example, Wayland-Yutani is a company that specializes in synthetic units. Its path (internal class name) is Synthetics. Companies.cs contains all the companies in the mod.

Common confusions:
    BattleUnit is the unit object in battles. To persist the changes you made to a unit, you need to update the UpgradableUnit object associated with the BattleUnit. If you do not wish to persist the changes, you can just modify the BattleUnit object, or patch the attribute getters.

    This error can be ignored: 所生成项目的处理器架构“MSIL”与引用“MelonLoader”的处理器架构“AMD64”不匹配。

    All reinforcements should be registered in its corresponding "Path" class. For example, SyntheticsReinforcements\SmartWeaponModule.cs is registered in Synthetics.cs. Same for other paths like Xeno or Rockstar.
