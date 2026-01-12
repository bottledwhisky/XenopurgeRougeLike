using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.Area;
using SpaceCommander.Area.Drawers;
using SpaceCommander.Audio;
using SpaceCommander.BattleManagement.UI;
using SpaceCommander.Commands;
using SpaceCommander.Database;
using SpaceCommander.GameFlow;
using SpaceCommander.Objectives;
using System;
using System.Collections.Generic;
using System.Linq;
using static SpaceCommander.Enumerations;

namespace XenopurgeRougeLike.RockstarReinforcements
{
    // 战斗开始时自动部署一个“热情的粉丝”，他会自己找乐子。解锁粉丝数，每场战斗后，获得1k-2k粉丝。

    public class RockstarAffinity2 : CompanyAffinity
    {
        public RockstarAffinity2()
        {
            unlockLevel = 2;
            company = Company.Rockstar;
            description = "A \"Passionate Fan\" is automatically deployed at the start of battle and will find their own fun. Unlock Fan Count; gain 1k-2k fans after each battle.";
        }

        public static RockstarAffinity2 _instance;

        public static RockstarAffinity2 Instance => _instance ??= new();

        public override string ToFullDescription()
        {
            return base.ToFullDescription() + $"\nCurrent Fan Count: {RockstarAffinityHelpers.fanCount}";
        }

        public static bool IsAnyRockstarAffinityActive => Instance.IsActive || RockstarAffinity4.Instance.IsActive || RockstarAffinity6.Instance.IsActive;
    }

    [HarmonyPatch(typeof(BattleUnitGO), "BindCharacter")]
    public class BattleUnitGO_BindCharacterPatch
    {
        public static bool Prefix(BattleUnit battleUnit)
        {
            if (battleUnit != UnitsPlacementPhasePatch.fan)
            {
                return true;
            }
            AccessTools.Field(typeof(BattleUnit), "_unitTag").SetValue(UnitsPlacementPhasePatch.fan, UnitTag.None);
            return true;
        }

        public static void Postfix(BattleUnit battleUnit)
        {
            if (battleUnit != UnitsPlacementPhasePatch.fan)
            {
                return;
            }
            AccessTools.Field(typeof(BattleUnit), "_unitTag").SetValue(UnitsPlacementPhasePatch.fan, UnitTag.NPC);
        }
    }

    [HarmonyPatch(typeof(UnitsPlacementPhase))]
    public class UnitsPlacementPhasePatch
    {
        static HuntCommandDataSO huntCommand;

        public static HuntCommandDataSO HuntCommandDataSO => (huntCommand ??= UnityEngine.Resources.FindObjectsOfTypeAll<HuntCommandDataSO>().FirstOrDefault());

        public static BattleUnit fan;

        [HarmonyPatch("AddNPCsPhase")]
        [HarmonyPrefix]
        public static void AddNPCsPhase(GridManager gridManager, IEnumerable<BattleUnit> battleUnits)
        {
            if (!RockstarAffinity2.IsAnyRockstarAffinityActive)
            {
                return;
            }
            MelonLogger.Msg("AddNPCsPhase: true");
            var gameManager = GameManager.Instance;
            BattleUnitsManager teamManager = gameManager.GetTeamManager(Team.Player);

            // Create fan unit data using shared helper method
            var ud = RockstarAffinityHelpers.CreateFanUnitData();
            // Assign NPC tag to reuse the NPC deployment logic, will be changed back to None later
            // to avoid being a mission objective
            ud.UnitTag = UnitTag.NPC;

            fan = new(ud, Team.Player, gridManager)
            {
                DeploymentOrder = 5
            };
            fan.AddCommands();

            teamManager.AddBattleUnit(fan);
        }
    }

    [HarmonyPatch(typeof(HireUnitGeneratorSettingsSO))]
    public class HireUnitGeneratorSettingsSO_Patch
    {
        static int FanIndex = -5;

        [HarmonyPatch("GetUnitNameByIndex")]
        [HarmonyPrefix]
        public static bool GetUnitNameByIndex(int index, out string __result)
        {
            if (index == FanIndex)
            {
                __result = RockstarAffinityHelpers.FAN_NAME;
                return false;
            }
            __result = "";
            return true;
        }

        [HarmonyPatch("GetUnitIndexByName")]
        [HarmonyPrefix]
        public static bool GetUnitIndexByName(string unitName, out int __result)
        {
            if (unitName == RockstarAffinityHelpers.FAN_NAME)
            {
                __result = FanIndex;
                return false;
            }
            __result = 0;
            return true;
        }
    }

    [HarmonyPatch(typeof(DeploymentUnitListView_BattleManagement))]
    public static class DeploymentUnitListView_BattleManagementPatch
    {
        [HarmonyPatch("CreateList")]
        [HarmonyPrefix]
        public static bool CreateListPrefix(ref IEnumerable<BattleUnit> battleUnits)
        {
            if (!RockstarAffinity2.IsAnyRockstarAffinityActive)
            {
                return true;
            }
            battleUnits = battleUnits.Where(bu => bu != UnitsPlacementPhasePatch.fan);
            return true;
        }

        [HarmonyPatch("CreateList")]
        [HarmonyPostfix]
        public static void CreateListPostfix(IEnumerable<BattleUnit> battleUnits)
        {
            if (!RockstarAffinity2.IsAnyRockstarAffinityActive)
            {
                return;
            }
            UnitsPlacementPhasePatch.fan.DeploymentOrder = battleUnits.Count() + 1;
        }
    }

    [HarmonyPatch(typeof(UnitsListView_BattleManagement))]
    public static class UnitsListView_BattleManagementPatch
    {
        [HarmonyPatch("CreateList")]
        [HarmonyPrefix]
        public static bool CreateList(ref IEnumerable<BattleUnit> battleUnits)
        {
            if (!RockstarAffinity2.IsAnyRockstarAffinityActive)
            {
                return true;
            }
            battleUnits = battleUnits.Where(bu => bu != UnitsPlacementPhasePatch.fan);
            return true;
        }
    }

    [HarmonyPatch(typeof(GameManager))]
    public class GameManager_Patch
    {
        [HarmonyPatch("StartCommandsExecution")]
        [HarmonyPostfix]
        public static void StartCommandsExecution()
        {
            if (!RockstarAffinity2.IsAnyRockstarAffinityActive)
            {
                return;
            }
            AccessTools.Field(typeof(BattleUnit), "_unitTag").SetValue(UnitsPlacementPhasePatch.fan, UnitTag.None);
        }

        [HarmonyPatch("GiveEndGameRewards")]
        [HarmonyPrefix]
        public static bool GiveEndGameRewards(GameManager __instance, bool victory)
        {
            if (!RockstarAffinity2.IsAnyRockstarAffinityActive)
            {
                return true;
            }
            var _teams = AccessTools.Field(typeof(GameManager), "_teams").GetValue(__instance) as Dictionary<Team, BattleUnitsManager>;
            var bum = _teams[Team.Player];
            var deadUnits = bum.DeadUnits as List<BattleUnit>;
            deadUnits.Remove(UnitsPlacementPhasePatch.fan);
            return true;
        }
    }

    [HarmonyPatch(typeof(ChooseUnitForCard_BattleManagementDirectory), "Initialize")]
    public class ChooseUnitForCard_BattleManagementDirectoryPatch
    {
        public class State
        {
            public List<BattleUnit> buList;
            public int fanOrder;
        }

        public static bool Prefix(ChooseUnitForCard_BattleManagementDirectory __instance, out State __state)
        {
            if (!RockstarAffinity2.IsAnyRockstarAffinityActive)
            {
                __state = null;
                return true;
            }
            var _battleUnitsManager = AccessTools.Field(typeof(ChooseUnitForCard_BattleManagementDirectory), "_battleUnitsManager").GetValue(__instance) as BattleUnitsManager;
            var buList = _battleUnitsManager.BattleUnits as List<BattleUnit>;
            var fanOrder = buList.IndexOf(UnitsPlacementPhasePatch.fan);
            if (fanOrder == -1)
            {
                __state = null;
                return true;
            }
            buList.RemoveAt(fanOrder);
            __state = new State()
            {
                buList = buList,
                fanOrder = fanOrder,
            };
            return true;
        }

        public static void Postfix(State __state)
        {
            if (__state == null)
            {
                return;
            }
            __state.buList.Insert(__state.fanOrder, UnitsPlacementPhasePatch.fan);
        }
    }


    [HarmonyPatch(typeof(ExtractSoldierObjective), "AddProgress")]
    public class ExtractSoldierObjectivePatch
    {
        public class State
        {
            public List<BattleUnit> buList;
            public int fanOrder;
        }

        public static bool Prefix(ExtractSoldierObjective __instance, out State __state)
        {
            if (!RockstarAffinity2.IsAnyRockstarAffinityActive)
            {
                __state = null;
                return true;
            }
            var _battleUnitsManager = AccessTools.Field(typeof(ExtractSoldierObjective), "_battleUnitsManager").GetValue(__instance) as BattleUnitsManager;
            var buList = _battleUnitsManager.BattleUnits as List<BattleUnit>;
            var fanOrder = buList.IndexOf(UnitsPlacementPhasePatch.fan);
            if (fanOrder == -1)
            {
                __state = null;
                return true;
            }
            buList.RemoveAt(fanOrder);
            __state = new State()
            {
                buList = buList,
                fanOrder = fanOrder,
            };
            return true;
        }

        public static void Postfix(State __state)
        {
            if (__state == null)
            {
                return;
            }
            __state.buList.Insert(__state.fanOrder, UnitsPlacementPhasePatch.fan);
        }
    }

    // Hide the path because otherwise it will reveal the fan's movement
    // Because it's fun to be unpredictable
    [HarmonyPatch(typeof(PathDrawer))]
    public class PathDrawerPatch
    {
        [HarmonyPatch("ClearPath")]
        [HarmonyPrefix]
        public static bool ClearPath(BattleUnit battleUnit)
        {
            if (battleUnit == UnitsPlacementPhasePatch.fan)
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch("DrawLine")]
        [HarmonyPrefix]
        public static bool DrawLine(int lineIndex, List<Tile> lineTiles, BattleUnit battleUnit)
        {
            if (battleUnit == UnitsPlacementPhasePatch.fan)
            {
                return false;
            }
            return true;
        }
    }
}
