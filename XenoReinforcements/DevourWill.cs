using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.Area;
using System;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.XenoReinforcements
{
    /// <summary>
    /// 吞噬意志：当眩晕中或被你控制的异形死亡时，你的所有队员回复10点生命值
    /// Devour Will: When a stunned or mind-controlled xeno dies, all your units restore 10 HP
    /// </summary>
    public class DevourWill : Reinforcement
    {
        public const float HealAmount = 10f;

        public DevourWill()
        {
            company = Company.Xeno;
            rarity = Rarity.Elite;
            stackable = false;
            name = L("xeno.devour_will.name");
            description = L("xeno.devour_will.description");
            flavourText = L("xeno.devour_will.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("xeno.devour_will.description", HealAmount);
        }

        protected static DevourWill instance;
public static DevourWill Instance => instance ??= new();
    }

    /// <summary>
    /// Patch BattleUnit constructor to add OnDeath listener for DevourWill
    /// Handles both enemy xenos (stunned) and player units (mind-controlled)
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor)]
    public static class DevourWill_BattleUnit_Constructor_Patch
    {
        public static void Postfix(BattleUnit __instance, Team team)
        {
            if (!DevourWill.Instance.IsActive)
                return;

            // For enemy units: check if they are stunned when they die
            if (team == Team.EnemyAI)
            {
                Action action = null;
                action = () =>
                {
                    // Check if the unit was stunned when it died
                    if (XenoStunTracker.IsStunned(__instance) || XenoStunTracker.IsMindControlled(__instance))
                    {
                        MelonLogger.Msg($"DevourWill: Stunned xeno {__instance.UnitNameNoNumber} died, healing all player units");
                        UnitStatsTools.HealAllPlayerUnits(DevourWill.HealAmount);
                    }
                    __instance.OnDeath -= action;
                };
                __instance.OnDeath += action;
            }
        }
    }
}
