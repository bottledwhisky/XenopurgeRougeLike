using HarmonyLib;
using SpaceCommander;
using System;
using System.Collections.Generic;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    // Synthetics Affinity Level 4: All units gain +2 speed, +20% accuracy, and +2 power. Start missions with +4 access points. Synthetics reinforcements appear 2x more often.
    public class SyntheticsAffinity4 : CompanyAffinity
    {
        public const float SpeedBonus = 2f;
        public const float AccuracyBonus = .2f;
        public const float PowerBonus = 2f;
        public const int StartingAccessPointsBonus = 4;
        public const float ReinforcementChanceBonus = 2f;

        public SyntheticsAffinity4()
        {
            unlockLevel = 4;
            company = Company.Synthetics;
            description = L("synthetics.affinity4.description", (int)SpeedBonus, (int)(AccuracyBonus * 100), (int)PowerBonus, StartingAccessPointsBonus, (int)ReinforcementChanceBonus);
        }

        public override void OnActivate()
        {
            AwardSystem.WeightModifiers.Add(ModifyWeights);
            UnitStatsTools.InBattleUnitStatChanges["SyntheticsAffinity4_Speed"] = new UnitStatChange(UnitStats.Speed, SpeedBonus, ShouldApply);
            UnitStatsTools.InBattleUnitStatChanges["SyntheticsAffinity4_Accuracy"] = new UnitStatChange(UnitStats.Accuracy, AccuracyBonus, ShouldApply);
            UnitStatsTools.InBattleUnitStatChanges["SyntheticsAffinity4_Power"] = new UnitStatChange(UnitStats.Power, PowerBonus, ShouldApply);
        }

        public override void OnDeactivate()
        {
            UnitStatsTools.InBattleUnitStatChanges.Remove("SyntheticsAffinity4_Speed");
            UnitStatsTools.InBattleUnitStatChanges.Remove("SyntheticsAffinity4_Accuracy");
            UnitStatsTools.InBattleUnitStatChanges.Remove("SyntheticsAffinity4_Power");

            AwardSystem.WeightModifiers.Remove(ModifyWeights);
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
