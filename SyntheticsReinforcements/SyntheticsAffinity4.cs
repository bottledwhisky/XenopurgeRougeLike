using HarmonyLib;
using SpaceCommander;
using System;
using System.Collections.Generic;
using static SpaceCommander.Enumerations;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    public class SyntheticsAffinity4 : CompanyAffinity
    {
        public SyntheticsAffinity4()
        {
            unlockLevel = 4;
            company = Company.Synthetics;
            description = "速度+2，瞄准+20，近战伤害+2，开局获得4点接入点数，更高概率获得同流派增援";
        }

        public const float SpeedBonus = 2f;
        public const float AccuracyBonus = .2f;
        public const float PowerBonus = 2f;
        public const int StartingAccessPointsBonus = 4;
        public const float ReinforcementChanceBonus = 2f;

        public override void OnActivate()
        {
            XenopurgeRougeLike.WeightModifiers.Add(ModifyWeights);
            UnitStatsTools.InBattleUnitStatChanges["SyntheticsAffinity4_Speed"] = new UnitStatChange(UnitStats.Speed, SpeedBonus, ShouldApply);
            UnitStatsTools.InBattleUnitStatChanges["SyntheticsAffinity4_Accuracy"] = new UnitStatChange(UnitStats.Accuracy, AccuracyBonus, ShouldApply);
            UnitStatsTools.InBattleUnitStatChanges["SyntheticsAffinity4_Power"] = new UnitStatChange(UnitStats.Power, PowerBonus, ShouldApply);
        }

        public override void OnDeactivate()
        {
            UnitStatsTools.InBattleUnitStatChanges.Remove("SyntheticsAffinity4_Speed");
            UnitStatsTools.InBattleUnitStatChanges.Remove("SyntheticsAffinity4_Accuracy");
            UnitStatsTools.InBattleUnitStatChanges.Remove("SyntheticsAffinity4_Power");

            XenopurgeRougeLike.WeightModifiers.Remove(ModifyWeights);
        }


        public static SyntheticsAffinity4 _instance;

        public static SyntheticsAffinity4 Instance => _instance ??= new();


        public static bool ShouldApply(BattleUnit unit, Team team)
        {
            return Instance.IsActive && team == Team.Player;
        }

        private void ModifyWeights(List<Tuple<int, Reinforcement>> choices)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                // Check if this reinforcement belongs to Synthetics company
                if (choices[i].Item2.company.Type == CompanyType.Synthetics)
                {
                    int newWeight = (int)(choices[i].Item1 * ReinforcementChanceBonus);
                    choices[i] = new Tuple<int, Reinforcement>(newWeight, choices[i].Item2);
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameManager), "CreateNewGame")]
    public static class SyntheticsAffinity4CreateNewGamePatch
    {
        public static void Postfix(GameManager __instance)
        {
            CompanyAffinity companyAffinity = SyntheticsAffinity4.Instance;
            if (!companyAffinity.IsActive)
                return;
            var AccessPointsManager = __instance.AccessPointsManager;
            AccessPointsManager.AddAccessPoints(SyntheticsAffinity4.StartingAccessPointsBonus);
        }
    }
}
