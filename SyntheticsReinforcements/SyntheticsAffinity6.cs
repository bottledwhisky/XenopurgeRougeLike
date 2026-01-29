using HarmonyLib;
using SpaceCommander;
using System;
using System.Collections.Generic;
using static SpaceCommander.Enumerations;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    public class SyntheticsAffinity6 : CompanyAffinity
    {
        public SyntheticsAffinity6()
        {
            unlockLevel = 6;
            company = Company.Synthetics;
            description = "速度+3，瞄准+30，近战伤害+3，开局获得8点接入点数，更高概率获得同流派增援";
        }

        public const float SpeedBonus = 3f;
        public const float AccuracyBonus = .3f;
        public const float PowerBonus = 3f;
        public const int StartingAccessPointsBonus = 8;
        public const float ReinforcementChanceBonus = 2f;

        public override void OnActivate()
        {
            AwardSystem.WeightModifiers.Add(ModifyWeights);
            UnitStatsTools.InBattleUnitStatChanges["SyntheticsAffinity6_Speed"] = new UnitStatChange(UnitStats.Speed, SpeedBonus, ShouldApply);
            UnitStatsTools.InBattleUnitStatChanges["SyntheticsAffinity6_Accuracy"] = new UnitStatChange(UnitStats.Accuracy, AccuracyBonus, ShouldApply);
            UnitStatsTools.InBattleUnitStatChanges["SyntheticsAffinity6_Power"] = new UnitStatChange(UnitStats.Power, PowerBonus, ShouldApply);
        }

        public override void OnDeactivate()
        {
            UnitStatsTools.InBattleUnitStatChanges.Remove("SyntheticsAffinity6_Speed");
            UnitStatsTools.InBattleUnitStatChanges.Remove("SyntheticsAffinity6_Accuracy");
            UnitStatsTools.InBattleUnitStatChanges.Remove("SyntheticsAffinity6_Power");

            AwardSystem.WeightModifiers.Remove(ModifyWeights);
        }

        public static SyntheticsAffinity6 _instance;

        public static SyntheticsAffinity6 Instance => _instance ??= new();

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
    public static class SyntheticsAffinity6CreateNewGamePatch
    {
        public static void Postfix(GameManager __instance)
        {
            CompanyAffinity companyAffinity = SyntheticsAffinity6.Instance;
            if (!companyAffinity.IsActive)
                return;
            var AccessPointsManager = __instance.AccessPointsManager;
            AccessPointsManager.AddAccessPoints(SyntheticsAffinity6.StartingAccessPointsBonus);
        }
    }
}
