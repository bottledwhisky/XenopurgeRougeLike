using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.Area;
using SpaceCommander.Area.Drawers;
using SpaceCommander.BattleManagement.UI;
using SpaceCommander.Commands;
using SpaceCommander.GameFlow;
using SpaceCommander.Objectives;
using System;
using System.Collections.Generic;
using System.Linq;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.RockstarReinforcements
{
    // 战斗开始时自动部署一个"热情的粉丝"，他会自己找乐子。解锁粉丝数，每场战斗后，获得1k-2k粉丝。
    public class RockstarAffinityBase: CompanyAffinity
    {
        public RockstarAffinityBase()
        {
            company = Company.Rockstar;
        }

        public override void OnDeactivate()
        {
            RockstarAffinityHelpers.ResetStates();
            UnitsPlacementPhasePatch.ResetStates();
        }

        public override Dictionary<string, object> SaveState()
        {
            return new Dictionary<string, object>
            {
                { "fanCount", RockstarAffinityHelpers.fanCount }
            };
        }

        public override void LoadState(Dictionary<string, object> state)
        {
            if (state.ContainsKey("fanCount"))
            {
                RockstarAffinityHelpers.fanCount = Convert.ToInt32(state["fanCount"]);
                MelonLogger.Msg($"[RockstarAffinity2] Loaded fan count: {RockstarAffinityHelpers.fanCount}");
            }
        }
    }

    public class RockstarAffinity2 : RockstarAffinityBase
    {
        public RockstarAffinity2()
        {
            unlockLevel = 2;
            description = L("rockstar.affinity2.description");
        }

        public static RockstarAffinity2 _instance;

        public static RockstarAffinity2 Instance => _instance ??= new();

        public override string ToFullDescription()
        {
            return base.ToFullDescription() + $"\n{L("rockstar.affinity2.fan_count", RockstarAffinityHelpers.fanCount)}";
        }

        public static bool IsAnyRockstarAffinityActive => Instance.IsActive || RockstarAffinity4.Instance.IsActive || RockstarAffinity6.Instance.IsActive;
    }

    [HarmonyPatch(typeof(BattleUnitGO), "BindCharacter")]
    public class BattleUnitGO_BindCharacterPatch
    {
        public static bool Prefix(BattleUnit battleUnit)
        {
            if (!UnitsPlacementPhasePatch.IsFan(battleUnit))
            {
                return true;
            }
            AccessTools.Field(typeof(BattleUnit), "_unitTag").SetValue(battleUnit, UnitTag.None);
            return true;
        }

        public static void Postfix(BattleUnit battleUnit)
        {
            if (!UnitsPlacementPhasePatch.IsFan(battleUnit))
            {
                return;
            }
            AccessTools.Field(typeof(BattleUnit), "_unitTag").SetValue(battleUnit, UnitTag.NPC);
        }
    }

    // Helper class for temporarily removing and restoring fans from a list
    public class FanListHelper
    {
        public List<BattleUnit> buList;
        public List<int> fanIndexes = [];

        public static FanListHelper RemoveFans(List<BattleUnit> list)
        {
            var helper = new FanListHelper { buList = list };

            // Remove fans in reverse order to maintain correct indexes
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (UnitsPlacementPhasePatch.IsFan(list[i]))
                {
                    helper.fanIndexes.Insert(0, i); // Insert at beginning to maintain order
                    list.RemoveAt(i);
                }
            }

            return helper;
        }

        public void RestoreFans()
        {
            for (int i = 0; i < fanIndexes.Count; i++)
            {
                buList.Insert(fanIndexes[i], UnitsPlacementPhasePatch.fans[i]);
            }
        }
    }

    [HarmonyPatch(typeof(UnitsPlacementPhase))]
    public class UnitsPlacementPhasePatch
    {
        public static event Action<BattleUnit> OnFanCreated;

        static HuntCommandDataSO huntCommand;

        public static HuntCommandDataSO HuntCommandDataSO => huntCommand ??= UnityEngine.Resources.FindObjectsOfTypeAll<HuntCommandDataSO>().FirstOrDefault();

        public static List<BattleUnit> fans = [];

        public static bool IsFan(BattleUnit unit) => fans.Contains(unit);

        public static void ResetStates()
        {
            fans.Clear();
        }

        [HarmonyPatch("AddNPCsPhase")]
        [HarmonyPrefix]
        public static void AddNPCsPhase(GridManager gridManager, IEnumerable<BattleUnit> battleUnits)
        {
            if (!RockstarAffinity2.IsAnyRockstarAffinityActive)
            {
                return;
            }
            MelonLogger.Msg("AddNPCsPhase: true");

            // Clear fans list from previous battle
            fans.Clear();

            var gameManager = GameManager.Instance;
            BattleUnitsManager teamManager = gameManager.GetTeamManager(Team.Player);

            // Determine how many fans to spawn
            int fanCount = Superfan.Instance.IsActive ? 2 : 1;

            for (int i = 0; i < fanCount; i++)
            {
                // Create fan unit
                var ud = RockstarAffinityHelpers.CreateFanUnitData();
                // Assign NPC tag to reuse the NPC deployment logic, will be changed back to None later
                // to avoid being a mission objective
                ud.UnitTag = UnitTag.NPC;

                var fan = new BattleUnit(ud, Team.Player, gridManager)
                {
                    DeploymentOrder = 5 + i
                };
                fan.AddCommands();

                fans.Add(fan);
                teamManager.AddBattleUnit(fan);

                OnFanCreated?.Invoke(fan);
            }

            if (fanCount > 1)
            {
                MelonLogger.Msg($"Superfan: Spawning {fanCount} Passionate Fans!");
            }
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
            battleUnits = battleUnits.Where(bu => !UnitsPlacementPhasePatch.IsFan(bu));
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
            int baseOrder = battleUnits.Count();
            for (int i = 0; i < UnitsPlacementPhasePatch.fans.Count; i++)
            {
                UnitsPlacementPhasePatch.fans[i].DeploymentOrder = baseOrder + i + 1;
            }
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
            battleUnits = battleUnits.Where(bu => !UnitsPlacementPhasePatch.IsFan(bu));
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
            var unitTagField = AccessTools.Field(typeof(BattleUnit), "_unitTag");
            foreach (var fan in UnitsPlacementPhasePatch.fans)
            {
                unitTagField.SetValue(fan, UnitTag.None);
            }
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
            foreach (var fan in UnitsPlacementPhasePatch.fans)
            {
                deadUnits.Remove(fan);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ChooseUnitForCard_BattleManagementDirectory), "Initialize")]
    public class ChooseUnitForCard_BattleManagementDirectoryPatch
    {
        public static bool Prefix(ChooseUnitForCard_BattleManagementDirectory __instance, out FanListHelper __state)
        {
            if (!RockstarAffinity2.IsAnyRockstarAffinityActive)
            {
                __state = null;
                return true;
            }
            var _battleUnitsManager = AccessTools.Field(typeof(ChooseUnitForCard_BattleManagementDirectory), "_battleUnitsManager").GetValue(__instance) as BattleUnitsManager;
            var buList = _battleUnitsManager.BattleUnits as List<BattleUnit>;
            __state = FanListHelper.RemoveFans(buList);
            return true;
        }

        public static void Postfix(FanListHelper __state)
        {
            __state?.RestoreFans();
        }
    }


    [HarmonyPatch(typeof(ExtractSoldierObjective), "AddProgress")]
    public class ExtractSoldierObjectivePatch
    {
        public static bool Prefix(ExtractSoldierObjective __instance, out FanListHelper __state)
        {
            if (!RockstarAffinity2.IsAnyRockstarAffinityActive)
            {
                __state = null;
                return true;
            }
            var _battleUnitsManager = AccessTools.Field(typeof(ExtractSoldierObjective), "_battleUnitsManager").GetValue(__instance) as BattleUnitsManager;
            var buList = _battleUnitsManager.BattleUnits as List<BattleUnit>;
            __state = FanListHelper.RemoveFans(buList);
            return true;
        }

        public static void Postfix(FanListHelper __state)
        {
            __state?.RestoreFans();
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
            return !UnitsPlacementPhasePatch.IsFan(battleUnit);
        }

        [HarmonyPatch("DrawLine")]
        [HarmonyPrefix]
        public static bool DrawLine(int lineIndex, List<Tile> lineTiles, BattleUnit battleUnit)
        {
            return !UnitsPlacementPhasePatch.IsFan(battleUnit);
        }
    }
}
