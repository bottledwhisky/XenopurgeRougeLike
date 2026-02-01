This is a game mod project.

I am on Windows, but some of your commands are run in MinGW, IDK why. It is a bit confusing.

You are encouraged to read the existing Reinforcement sub-classes to quickly get an example of what we are working on. Existing code already contains common patterns like how to run code upon a mission start/end, or how to change stats, or test unit type conditions.
The decompiled source code of the original game is available here: D:\projects\xenopurge\old
In D:\projects\xenopurge\old, each folder contains a "README" text file that tells you which functionalities are implemented in that folder, and in which files. You should read those README files first to understand the code structure to avoid getting lost.
I might use Chinese to communicate with you, but the code is mostly in English.

Some important files:
    Companies.cs: Contains all definitions of the companies (paths). And basic classes like Company/CompanyAffinity
    Reinforcement.cs: The base class of all Reinforcements.
    UnitStatsTools.cs: Some existing code to help mess with unit stats.
    ActionCardsUpgraderTools.cs: Some existing code to help mess with generating action cards in the action cards store.
    (Higely recommended) XenopurgeRougeLike.txt: If you are looking for some existing examples of certain effects, you can check this file. Please be noted that not all reinforcements/affinities are implemented yet. But it's a good place to start.


If you are working on localizing the mod. The data is stored in .\I18nData\ folder. But the files in it can be very large, so please restrain yourself to only read (grep) the part you are working on. I18nData\affinity.cs is a small file that you can use as an example.

At the time of writing, the files in I18nData are:
PS D:\projects\xenopurge\XenopurgeRougeLike> dir I18nData   

    Directory: D:\projects\xenopurge\XenopurgeRougeLike\I18nData

Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
-a---           2026/1/30    19:12           4660 affinity.cs
-a---           2026/1/30    19:12          40692 company.cs
-a---           2026/1/30    19:12          50704 engineer.cs
-a---           2026/1/30    19:11          69071 rockstar.cs
-a---           2026/1/30    19:11          13498 ui.cs

And they are merged in I18nData.cs

Localization.cs (read it!) contains the code to format the string. If the original string needs some placeholders from the other parts of the code, feel free to refactor and use them.

You only need to localize the strings that the user can see. Logs can be left in English.
Use `using static XenopurgeRougeLike.ModLocalization;` then `L("my_module.my_string")` should be used because it is shorter.

L supports placeholders, for example ` L("rockstar.star_power.description_stack1", (int)(FanBonusMultiplier*100), FanPerStatBuff, HPBonus, (int)(AccuracyBonus*100), SpeedBonus, PowerBonus)`.

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

For example: "5 coins" should be written as "5 <sprite name="CoinIcon">". Coin, Accuracy, Armor, etc. should be used a unit. For example in English, +20 Accuracy/Aim should be written as "+20 <sprite name="AccuracyIcon">", instead of "<sprite name="AccuracyIcon"> +20".

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

Each run consists of many missions (a.k.a battles). If you use a mission-wide state, you should use this pattern to clear the state when the mission ends:

```c#
    // Example from reinforcement "ModularDesign"
    [HarmonyPatch(typeof(TestGame), "EndGame")]
    public static class ModularDesign_ClearTracking_Patch
    {
        public static void Postfix()
        {
            TurretRedeploymentTracker.ClearAll();
        }
    }
```

Similarly, TestGame has "StartGame" that you can use to initialize your state.

For BattleUnit, you should use this pattern to hook the OnDeath event:

```c#
    // Example from affinity "XenoAffinity6"
    // Patch BattleUnit constructor to add OnDeath listener
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor, [typeof(UnitData), typeof(Enumerations.Team), typeof(GridManager)])]
    public static class XenoAffinity6_BattleUnit_Constructor_Patch
    {
        public static void Postfix(BattleUnit __instance, Team team)
        {
            if (!XenoAffinity6.Instance.IsActive)
                return;

            if (team == Team.EnemyAI)
            {
                void action()
                {
                    // When this xeno dies, stun nearby xenos
                    XenoAffinity6_StunSystem.StunNearbyXenos(__instance);
                    __instance.OnDeath -= action;
                }

                __instance.OnDeath += action;
            }
        }
    }
```

瞄准 Accuracy is displayed in x100 scale, but is actually a float. E.g. +20 Aim is implemented as .2f in code.

About the original game:
The original game is called "Xenopurge". It is a tactical RTS where players control a squad of soldiers fighting against aliens(xenoes). During battle, the game allows the player to use "ActionCards" to perform actions, not direct control.

Keywords:
    增援(reinforcement) is a term only used in the mod, not in the original game.
    合成人(synthetics) is a kind of unit in the original game. The player can get a squad of all synthetic units to fight against aliens.
    装备(equipment) is a general term for items that can be equipped by units, including ranged weapons, melee weapons, and gears.
    公司(company) is a mod-only flavor term for the player's faction(流派) or path(路线) of the reinforcements. For example, Wayland-Yutani is a company that specializes in synthetic units. Its path (internal class name) is Synthetics. Companies.cs contains all the companies in the mod.

