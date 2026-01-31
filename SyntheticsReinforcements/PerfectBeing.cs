using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Area;
using System;
using System.Collections.Generic;
using System.Text;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    // Perfect Being: While at full health, gain +5 speed, +50% accuracy, and +5 power. Lose these bonuses when damaged.
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
            name = L("synthetics.perfect_being.name");
            description = L("synthetics.perfect_being.description", (int)SpeedBonus, (int)(AimBonus * 100), (int)MeleeDamageBonus);
            flavourText = L("synthetics.perfect_being.flavour");
        }

        protected static PerfectBeing instance;
        public static PerfectBeing Instance => instance ??= new();
    }


    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor)]
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
