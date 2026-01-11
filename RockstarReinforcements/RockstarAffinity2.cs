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
    public class RockstarAffinity2 : CompanyAffinity
    {
        public static int fanCount = 0;
        public RockstarAffinity2()
        {
            unlockLevel = 2;
            description = "A \"Passionate Fan\" is automatically deployed at the start of battle and will find their own fun. Unlock Fan Count; gain 1k-2k fans after each battle.";
        }

        public static RockstarAffinity2 Instance => (RockstarAffinity2)Company.GetAffinity(CompanyType.Rockstar, 2);
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

        public static HuntCommandDataSO FindHuntCommandDataSO()
        {
            if (huntCommand != null)
            {
                return huntCommand;
            }

            try
            {
                HuntCommandDataSO[] huntCommands = UnityEngine.Resources.FindObjectsOfTypeAll<HuntCommandDataSO>();
                if (huntCommands != null && huntCommands.Length > 0)
                {
                    MelonLogger.Msg($"Found {huntCommands.Length} HuntCommandDataSO instances");
                    return huntCommands[0];
                }

                MelonLogger.Warning("Could not find or create HuntCommandDataSO");
                return null;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error finding HuntCommandDataSO: {ex}");
                return null;
            }
        }

        public static BattleUnit fan;

        [HarmonyPatch("AddNPCsPhase")]
        [HarmonyPrefix]
        public static void AddNPCsPhase(GridManager gridManager, IEnumerable<BattleUnit> battleUnits)
        {
            MelonLogger.Msg("AddNPCsPhase: true");
            var gameManager = GameManager.Instance;
            BattleUnitsManager teamManager = gameManager.GetTeamManager(Team.Player);
            var _playerData = Singleton<Player>.Instance.PlayerData;
            MelonLogger.Msg($"AddNPCsPhase: 2");
            var ud = _playerData.Squad.SquadUnits.First().GetCopyOfUnitData();
            ud.UnitId = Guid.NewGuid().ToString();
            ud.UnitName = "Fan";
            ud.UnitTag = UnitTag.NPC;
            ud.UnitNameLocalizedStringIndex = -5;

            var hug = Singleton<AssetsDatabase>.Instance.HireUnitGeneratorSettingsSO;


            var _voiceActingListSO = AccessTools.Field(typeof(HireUnitGeneratorSettingsSO), "_voiceActingListSO").GetValue(hug) as VoiceActingListSO;
            MelonLogger.Msg($"AddNPCsPhase: _voiceActingListSO={_voiceActingListSO}");

            bool gender = UnityEngine.Random.Range(0, 2) == 0;

            ud.VoiceActorGUID = _voiceActingListSO.GetRandomVoiceActor(gender ? Gender.female : Gender.male).AssetGUID;
            HuntCommandDataSO huntCommand = FindHuntCommandDataSO();
            MelonLogger.Msg($"AddNPCsPhase: 3");
            var cmdList = ud.CommandsDataSOList.ToList();
            cmdList.Insert(2, huntCommand);
            foreach (var cmd in cmdList)
            {
                MelonLogger.Msg($"AddNPCsPhase: cmd {cmd.CommandName} {cmd.GetType()}");
            }
            ud.CommandsDataSOList = cmdList.ToArray();
            fan = new BattleUnit(ud, Team.Player, gridManager);
            fan.DeploymentOrder = 5;
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
                __result = "Fan";
                return false;
            }
            __result = "";
            return true;
        }

        [HarmonyPatch("GetUnitIndexByName")]
        [HarmonyPrefix]
        public static bool GetUnitIndexByName(string unitName, out int __result)
        {
            if (unitName == "Fan")
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
            battleUnits = battleUnits.Where(bu => bu != UnitsPlacementPhasePatch.fan);
            return true;
        }

        [HarmonyPatch("CreateList")]
        [HarmonyPostfix]
        public static void CreateListPostfix(IEnumerable<BattleUnit> battleUnits)
        {
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
            battleUnits = battleUnits.Where(bu => bu != UnitsPlacementPhasePatch.fan);
            return true;
        }
    }

    [HarmonyPatch(typeof(GameManager), "StartCommandsExecution")]
    public class GameManager_StartCommandsExecution_Patch
    {
        public static void Postfix()
        {
            AccessTools.Field(typeof(BattleUnit), "_unitTag").SetValue(UnitsPlacementPhasePatch.fan, UnitTag.None);
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
