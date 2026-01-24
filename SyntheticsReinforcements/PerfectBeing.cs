using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Area;
using System;
using System.Collections.Generic;
using System.Text;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    public class PerfectBeing : Reinforcement
    {
        public const float SpeedBonus = 5f;
        public const float AimBonus = .5f;
        public const float MeleeDamageBonus = 5f;
        public PerfectBeing()
        {
            company = Company.Synthetics;
            stackable = false;
            maxStacks = 1;
            rarity = Rarity.Expert;
            name = "Perfect Being";
            description = "Your units gain +5 speed, +50 aim, and +5 melee damage when at full health.";
            flavourText = "The synthetic body is Weyland-Yutani's answer to life itself. In pristine condition, it performs exactly as designed: flawless.";
        }

        protected static PerfectBeing instance;
        public static PerfectBeing Instance => instance ??= new();
    }


    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor)]
    [HarmonyPatch([typeof(UnitData), typeof(Enumerations.Team), typeof(GridManager)])]
    public static class PerfectBeing_UnitStatsChangeConstructor
    {
        public static void Postfix(BattleUnit __instance, Enumerations.Team team)
        {
            if (!PerfectBeing.Instance.IsActive || team != Enumerations.Team.Player)
                return;
            __instance.ChangeStat(Enumerations.UnitStats.Speed, PerfectBeing.SpeedBonus, "PerfectBeing_FullHealthBonusSpeed");
            __instance.ChangeStat(Enumerations.UnitStats.Accuracy, PerfectBeing.AimBonus, "PerfectBeing_FullHealthBonusAccuracy");
            __instance.ChangeStat(Enumerations.UnitStats.Power, PerfectBeing.MeleeDamageBonus, "PerfectBeing_FullHealthBonusPower");

            __instance.OnHealthChanged += (currentHealth) =>
            {
                if (currentHealth >= __instance.CurrentMaxHealth)
                {
                    __instance.ChangeStat(Enumerations.UnitStats.Speed, PerfectBeing.SpeedBonus, "PerfectBeing_FullHealthBonusSpeed");
                    __instance.ChangeStat(Enumerations.UnitStats.Accuracy, PerfectBeing.AimBonus, "PerfectBeing_FullHealthBonusAccuracy");
                    __instance.ChangeStat(Enumerations.UnitStats.Power, PerfectBeing.MeleeDamageBonus, "PerfectBeing_FullHealthBonusPower");
                }
                else
                {
                    __instance.ReverseChangeOfStat("PerfectBeing_FullHealthBonusSpeed");
                    __instance.ReverseChangeOfStat("PerfectBeing_FullHealthBonusAccuracy");
                    __instance.ReverseChangeOfStat("PerfectBeing_FullHealthBonusPower");
                }
            };
        }
    }
}
