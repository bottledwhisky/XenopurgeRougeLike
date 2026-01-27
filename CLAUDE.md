This is a game mod project.
You are encouraged to read the existing Reinforcement sub-classes to quickly get an example of what we are working on. Existing code already contains common patterns like how to run code upon a mission start/end, or how to change stats, or test unit type conditions.
The decompiled source code of the original game is available here: D:\projects\xenopurge\old
In D:\projects\xenopurge\old, each folder contains a "README" text file that tells you which functionalities are implemented in that folder, and in which files. You should read those README files first to understand the code structure to avoid getting lost.
I might use Chinese to communicate with you, but the code is mostly in English.

About this mod:
This mod adds a rouge-like system to the game, allowing players to call for reinforcements after each mission. The reinforcements have unique attributes and abilities, making each playthrough different.

Some important files:
    Companies.cs: Contains all definitions of the companies (paths). And basic classes like Company/CompanyAffinity
    Reinforcement.cs: The base class of all Reinforcements.
    UnitStatsTools.cs: Some existing code to help mess with unit stats.
    ActionCardsUpgraderTools.cs: Some existing code to help mess with generating action cards in the action cards store.
    XenopurgeRougeLike.txt: If you are looking for some existing examples of certain effects, you can check this file. Please be noted that not all reinforcements/affinities are implemented yet. But it's a good place to start.

About the original game:
The original game is called "Xenopurge". It is a tactical RTS where players control a squad of soldiers fighting against aliens(xenoes). During battle, the game allows the player to use "ActionCards" to perform actions, not direct control.

Keywords:
    增援(reinforcement) is a term only used in the mod, not in the original game.
    合成人(synthetics) is a kind of unit in the original game. The player can get a squad of all synthetic units to fight against aliens.
    装备(equipment) is a general term for items that can be equipped by units, including ranged weapons, melee weapons, and gears.
    公司(company) is a mod-only flavor term for the player's faction(流派) or path(路线) of the reinforcements. For example, Wayland-Yutani is a company that specializes in synthetic units. Its path (internal class name) is Synthetics. Companies.cs contains all the companies in the mod.

Common confusions:
    BattleUnit is the unit object in battles. To persist the changes you made to a unit, you need to update the UpgradableUnit object associated with the BattleUnit. If you do not wish to persist the changes, you can just modify the BattleUnit object, or patch the attribute getters.

    This error can be ignored: 所生成项目的处理器架构“MSIL”与引用“MelonLoader”的处理器架构“AMD64”不匹配。

    All reinforcements should be registered in its corresponding "Path" class. For example, SyntheticsReinforcements\SmartWeaponModule.cs is registered in Synthetics.cs. Same for other paths like Xeno or Rockstar.
