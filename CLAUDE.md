This is a game mod project.

I am on Windows, but some of your commands are run in MinGW, IDK why. It is a bit confusing.

You are working on localizing the mod. The data is stored in I18nData.cs. But the file can be very large, so please restrain yourself to only read (grep) the part you are working on.

Localization.cs (read it!) contains the code to format the string. If the original string needs some placeholders from the other parts of the code, feel free to refactor and use them.

You only need to localize the strings that the user can see. Logs can be left in English.

There some available icons that you can should use in the strings when appropriate.

<sprite name="CoinIcon">
<sprite name="AccuracyIcon">
<sprite name="Armoricon">
<sprite name="BiomassIcon">
<sprite name="Healthicon">
<sprite name="override icon">
<sprite name="PackageIcon">
<sprite name="Powericon">
<sprite name="Speedicon">
<sprite name="time icon">
<sprite name=AccessPointIcon>

For example: "5 coins" should be written as "5 <sprite name="CoinIcon">".

All the below languages should be added:
* English (en)
* Greek(el)
* French(fr)
* Chinese(Simplified) (zh)
* Chinese(Traditional) (zh-TW)
* German(de)
* Polish(pl)
* Portuguese(pt)
* Russian(ru)
* Spanish(es)
* Ukrainian(uk)
* Japanese(ja)
* Korean(ko)

About this mod:
This mod adds a rouge-like system to the game, allowing players to call for reinforcements after each mission. The reinforcements have unique attributes and abilities, making each playthrough different.

Some important files:
    Companies.cs: Contains all definitions of the companies (paths). And basic classes like Company/CompanyAffinity
    Reinforcement.cs: The base class of all Reinforcements.
    UnitStatsTools.cs: Some existing code to help mess with unit stats.
    ActionCardsUpgraderTools.cs: Some existing code to help mess with generating action cards in the action cards store.
    (Higely recommended) XenopurgeRougeLike.txt: If you are looking for some existing examples of certain effects, you can check this file. Please be noted that not all reinforcements/affinities are implemented yet. But it's a good place to start.

About the original game:
The original game is called "Xenopurge". It is a tactical RTS where players control a squad of soldiers fighting against aliens(xenoes). During battle, the game allows the player to use "ActionCards" to perform actions, not direct control.

Keywords:
    增援(reinforcement) is a term only used in the mod, not in the original game.
    合成人(synthetics) is a kind of unit in the original game. The player can get a squad of all synthetic units to fight against aliens.
    装备(equipment) is a general term for items that can be equipped by units, including ranged weapons, melee weapons, and gears.
    公司(company) is a mod-only flavor term for the player's faction(流派) or path(路线) of the reinforcements. For example, Wayland-Yutani is a company that specializes in synthetic units. Its path (internal class name) is Synthetics. Companies.cs contains all the companies in the mod.

