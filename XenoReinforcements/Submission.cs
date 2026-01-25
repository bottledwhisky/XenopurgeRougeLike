using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.Abilities;
using SpaceCommander.Commands;
using System;
using UnityEngine;
using static SpaceCommander.Enumerations;

namespace XenopurgeRougeLike.XenoReinforcements
{
    /// <summary>
    /// 屈服：对生命值只剩一半或以下异形造成近战伤害后，有50%概率将其变为友军
    /// Submission: After dealing melee damage to a xeno below half health, there is a 50% chance to convert it to your team
    /// </summary>
    public class Submission : Reinforcement
    {
        public const float ConversionChance = .5f;

        public Submission()
        {
            company = Company.Xeno;
            rarity = Rarity.Expert;
            stackable = false;
            name = "Submission";
            description = "After dealing melee damage to a xeno, there is a {0}% chance to convert it to your team.";
            flavourText = "Your presence bends the hive mind to your will.";
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return string.Format(description, (int)(ConversionChance * 100));
        }

        protected static Submission instance;
        public static Submission Instance => instance ??= new();
    }

    /// <summary>
    /// Patch Melee command to trigger conversion on attack
    /// </summary>
    [HarmonyPatch(typeof(Melee), "Attack")]
    public static class Submission_Melee_Attack_Patch
    {
        public static void Postfix(Melee __instance, BattleUnit ____battleUnit, IDamagable ____target)
        {
            if (!Submission.Instance.IsActive)
                return;

            // Only process for player units attacking
            if (____battleUnit == null || ____battleUnit.Team != Team.Player || MindControl.MindControlledUnits.Contains(____battleUnit))
                return;

            // Check if target is a valid enemy unit
            if (____target == null || !____target.IsAlive)
                return;

            var bu = ____target as BattleUnit;
            if (bu == null) return;

            if (bu.CurrentHealth / bu.CurrentMaxHealth > 0.5f)
            {
                return;
            }

            // Target must be a BattleUnit (xeno)
            var targetUnit = ____target as BattleUnit;
            if (targetUnit == null || targetUnit.Team != Team.EnemyAI || targetUnit.UnitTag == UnitTag.Hive)
                return;

            // Roll for conversion chance
            float roll = UnityEngine.Random.value;
            if (roll > Submission.ConversionChance)
            {
                MelonLogger.Msg($"Submission: Roll failed ({roll:F2} > {Submission.ConversionChance})");
                return;
            }

            MelonLogger.Msg($"Submission: Roll succeeded ({roll:F2} <= {Submission.ConversionChance}), converting {targetUnit.UnitNameNoNumber}");

            // Convert the xeno to player team
            MindControlSystem.ConvertUnitToPlayer(targetUnit, ____battleUnit);
        }
    }
}
