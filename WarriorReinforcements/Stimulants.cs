using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Area;
using System;
using System.Collections.Generic;
using System.Linq;
using TimeSystem;
using UnityEngine;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.WarriorReinforcements
{
    /// <summary>
    /// 兴奋剂：（血量大于10点时可用）受到10点伤害，+5速度，+5近战伤害，-20瞄准，持续30秒
    /// Stimulants: (When health > 10) Take 10 damage, +5 Speed, +5 Power, -20 Accuracy, lasts 30 seconds
    /// </summary>
    public class Stimulants : Reinforcement
    {
        // Damage cost to use stimulants
        public const float HealthCost = 10f;

        // Minimum health required to use
        public const float MinHealthRequired = 10f;

        // Stat bonuses/penalties
        public const float SpeedBonus = 5f;
        public const float PowerBonus = 5f;
        public const float AccuracyPenalty = -0.2f; // -20 accuracy

        // Duration in seconds
        public const float Duration = 30f;

        // Dictionary to track active stimulant effects per unit
        public static Dictionary<BattleUnit, StimulantsEffect> activeEffects = new Dictionary<BattleUnit, StimulantsEffect>();

        public Stimulants()
        {
            company = Company.Warrior;
            rarity = Rarity.Elite;
            name = L("warrior.stimulants.name");
            description = L("warrior.stimulants.description",
                (int)MinHealthRequired,
                (int)HealthCost,
                (int)SpeedBonus,
                (int)PowerBonus,
                (int)(AccuracyPenalty * 100),
                (int)Duration);
            flavourText = L("warrior.stimulants.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("warrior.stimulants.description",
                (int)MinHealthRequired,
                (int)HealthCost,
                (int)SpeedBonus,
                (int)PowerBonus,
                (int)(AccuracyPenalty * 100),
                (int)Duration);
        }

        private static Stimulants _instance;
        public static Stimulants Instance => _instance ??= new();

        /// <summary>
        /// Struct to track active stimulants effect
        /// </summary>
        public struct StimulantsEffect
        {
            public float remainingTime;
        }
    }

    // Patch to clear state when mission starts
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public class Stimulants_TestGame_StartGame_Patch
    {
        public static void Postfix()
        {
            Stimulants.activeEffects.Clear();
        }
    }

    /// <summary>
    /// Patch to inject StimulantsActionCard into InBattleActionCardsManager after initialization
    /// </summary>
    [HarmonyPatch(typeof(InBattleActionCardsManager), "Initialize")]
    public static class Stimulants_InjectActionCard_Patch
    {
        public static void Postfix(InBattleActionCardsManager __instance)
        {
            if (!Stimulants.Instance.IsActive)
                return;

            var actionCardInfo = new StimulantsActionCardInfo();
            actionCardInfo.SetId("Stimulants");

            var stimulantsCard = new StimulantsActionCard(actionCardInfo);

            __instance.InBattleActionCards.Add(stimulantsCard);
        }
    }

    /// <summary>
    /// Stimulants action card - damages unit to gain temporary stat buffs
    /// Implements IUnitTargetable to target player units
    /// </summary>
    public class StimulantsActionCard : ActionCard, IUnitTargetable
    {
        public Team TeamToAffect => Team.Player;

        public StimulantsActionCard(ActionCardInfo actionCardInfo)
        {
            Info = actionCardInfo;
        }

        public override ActionCard GetCopy()
        {
            return new StimulantsActionCard(Info);
        }

        public void ApplyCommand(BattleUnit unit)
        {
            if (!Stimulants.Instance.IsActive)
                return;

            if (unit == null || !unit.IsAlive || unit.Team != Team.Player)
                return;

            // Check if unit has enough health
            if (unit.CurrentHealth <= Stimulants.MinHealthRequired)
                return;

            ApplyStimulantsEffect(unit);
        }

        private void ApplyStimulantsEffect(BattleUnit unit)
        {
            // Check if effect is already active (non-stackable)
            if (Stimulants.activeEffects.ContainsKey(unit))
            {
                // Refresh duration instead of stacking
                var effect = Stimulants.activeEffects[unit];
                effect.remainingTime = Stimulants.Duration;
                Stimulants.activeEffects[unit] = effect;
                MelonLogger.Msg($"Stimulants: Refreshed effect duration for unit {unit.UnitId} to {Stimulants.Duration}s");
                return;
            }

            // Deal damage cost
            unit.Damage(Stimulants.HealthCost);
            MelonLogger.Msg($"Stimulants: Unit {unit.UnitId} took {Stimulants.HealthCost} damage. Current health: {unit.CurrentHealth}");

            // Apply stat changes
            unit.ChangeStat(UnitStats.Speed, Stimulants.SpeedBonus, "Stimulants_Speed");
            unit.ChangeStat(UnitStats.Power, Stimulants.PowerBonus, "Stimulants_Power");
            unit.ChangeStat(UnitStats.Accuracy, Stimulants.AccuracyPenalty, "Stimulants_Accuracy");

            // Track the effect
            Stimulants.activeEffects[unit] = new Stimulants.StimulantsEffect
            {
                remainingTime = Stimulants.Duration
            };

            MelonLogger.Msg($"Stimulants: Applied effect to unit {unit.UnitId} for {Stimulants.Duration}s (+{Stimulants.SpeedBonus} Speed, +{Stimulants.PowerBonus} Power, {(int)(Stimulants.AccuracyPenalty * 100)} Accuracy)");
        }

        IEnumerable<CommandsAvailabilityChecker.UnitAnavailableReasons> IUnitTargetable.IsUnitValid(BattleUnit unit)
        {
            var reasons = new List<CommandsAvailabilityChecker.UnitAnavailableReasons>();

            if (!Stimulants.Instance.IsActive)
            {
                return reasons;
            }

            // Can only target alive units
            if (!unit.IsAlive)
            {
                reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.UnitIsDead);
            }
            else if (unit.CurrentHealth <= Stimulants.MinHealthRequired)
            {
                // Not enough health
                reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.InsufficientUnits);
            }

            return reasons;
        }
    }

    /// <summary>
    /// Custom ActionCardInfo for Stimulants
    /// </summary>
    public class StimulantsActionCardInfo : ActionCardInfo
    {
        public string CustomCardName => L("warrior.stimulants.card_name");

        public string CustomCardDescription => L("warrior.stimulants.card_description",
            (int)Stimulants.MinHealthRequired,
            (int)Stimulants.HealthCost,
            (int)Stimulants.SpeedBonus,
            (int)Stimulants.PowerBonus,
            (int)(Stimulants.AccuracyPenalty * 100),
            (int)Stimulants.Duration);

        public StimulantsActionCardInfo()
        {
            AccessTools.Field(typeof(ActionCardInfo), "_uses").SetValue(this, 0); // 0 = unlimited uses
            AccessTools.Field(typeof(ActionCardInfo), "canNotBeReplenished").SetValue(this, false);
        }
    }

    /// <summary>
    /// Patch to intercept CardName getter for StimulantsActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardName", MethodType.Getter)]
    public static class StimulantsActionCardInfo_CardName_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is StimulantsActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardName;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to intercept CardDescription getter for StimulantsActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardDescription", MethodType.Getter)]
    public static class StimulantsActionCardInfo_CardDescription_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is StimulantsActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardDescription;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to subscribe to OnDeath event for player units
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor, [typeof(UnitData), typeof(Enumerations.Team), typeof(GridManager)])]
    public class Stimulants_BattleUnit_Constructor_Patch
    {
        public static void OnUpdate(BattleUnit __instance, float deltaTime)
        {
            if (!Stimulants.Instance.IsActive)
                return;

            if (!Stimulants.activeEffects.ContainsKey(__instance))
                return;

            // Decrease timer
            var effect = Stimulants.activeEffects[__instance];
            effect.remainingTime -= deltaTime;

            // Check if stimulants expired or unit died
            if (effect.remainingTime <= 0 || !__instance.IsAlive)
            {
                RemoveStimulantsEffect(__instance);
            }
            else
            {
                Stimulants.activeEffects[__instance] = effect;
            }
        }

        private static void RemoveStimulantsEffect(BattleUnit unit)
        {
            Stimulants.activeEffects.Remove(unit);

            if (unit.IsAlive)
            {
                // Remove stat buffs
                unit.ReverseChangeOfStat("Stimulants_Speed");
                unit.ReverseChangeOfStat("Stimulants_Power");
                unit.ReverseChangeOfStat("Stimulants_Accuracy");

                MelonLogger.Msg($"Stimulants: Effect ended for {unit.UnitNameNoNumber}.");
            }
        }

        public static void Postfix(BattleUnit __instance, Team team)
        {
            if (!Stimulants.Instance.IsActive)
                return;

            if (team == Team.Player)
            {
                void onUpdateAction(float deltaTime)
                {
                    OnUpdate(__instance, deltaTime);
                }
                TempSingleton<TimeManager>.Instance.OnTimeUpdated += onUpdateAction;

                void action()
                {
                    if (Stimulants.activeEffects.ContainsKey(__instance))
                    {
                        Stimulants.activeEffects.Remove(__instance);
                    }
                    __instance.OnDeath -= action;
                    TempSingleton<TimeManager>.Instance.OnTimeUpdated -= onUpdateAction;
                }

                __instance.OnDeath += action;
            }
        }
    }
}
