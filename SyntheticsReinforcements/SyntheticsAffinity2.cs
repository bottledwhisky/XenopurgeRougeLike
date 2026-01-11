using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Area;
using System;
using UnityEngine;
using static SpaceCommander.Enumerations;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    public class SyntheticsAffinity2 : CompanyAffinity
    {
        public SyntheticsAffinity2()
        {
            unlockLevel = 2;
            company = Company.Synthetics;
            description = "速度+1，瞄准+10，近战伤害+1";
        }

        public const float SpeedBonus = 1f;
        public const float AccuracyBonus = .1f;
        public const float PowerBonus = 1f;

        public static SyntheticsAffinity2 _instance;

        public static SyntheticsAffinity2 Instance => _instance ??= new();


        public override void OnActivate()
        {
            UnitStatsTools.InBattleUnitStatChanges["SyntheticsAffinity2_Speed"] = new UnitStatChange(UnitStats.Speed, SpeedBonus, ShouldApply);
            UnitStatsTools.InBattleUnitStatChanges["SyntheticsAffinity2_Accuracy"] = new UnitStatChange(UnitStats.Accuracy, AccuracyBonus, ShouldApply);
            UnitStatsTools.InBattleUnitStatChanges["SyntheticsAffinity2_Power"] = new UnitStatChange(UnitStats.Power, PowerBonus, ShouldApply);
        }

        public override void OnDeactivate()
        {
            UnitStatsTools.InBattleUnitStatChanges.Remove("SyntheticsAffinity2_Speed");
            UnitStatsTools.InBattleUnitStatChanges.Remove("SyntheticsAffinity2_Accuracy");
            UnitStatsTools.InBattleUnitStatChanges.Remove("SyntheticsAffinity2_Power");
        }

        public static bool ShouldApply(BattleUnit unit, Team team)
        {
            return Instance.IsActive && team == Team.Player;
        }
    }
}
