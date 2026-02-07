using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Area;
using SpaceCommander.Commands;
using System.Collections.Generic;
using System.Reflection;
using TimeSystem;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SupportReinforcements
{
    /// <summary>
    /// 战地急救：获得战地急救指令，恢复一名生命值小于20的队友10点生命值。
    /// Field Medic: Gain a Field Medic action card that restores 10 HP to an ally with less than 20 HP.
    /// 实现细节：需要花费时间，可无限使用
    /// Implementation: Takes time, unlimited uses
    /// </summary>
    public class FieldMedic : Reinforcement
    {
        // Reflection field to access BattleUnit's private _currentHealth field
        internal static readonly FieldInfo _currentHealthField = AccessTools.Field(typeof(BattleUnit), "_currentHealth");
        internal static readonly FieldInfo _currentMaxHealthField = AccessTools.Field(typeof(BattleUnit), "_currentMaxHealth");

        // Healing amount
        public const float HealAmount = 10f;

        // Maximum health threshold to use this card
        public const float MaxHealthThreshold = 20f;

        // Duration of the healing process (in seconds) - same as Apply First Aid Kit
        public const float HealDuration = 7.4f;

        // Dictionary to track units currently being healed
        public static Dictionary<BattleUnit, FieldMedicHealingState> activeHealing = new Dictionary<BattleUnit, FieldMedicHealingState>();

        public FieldMedic()
        {
            company = Company.Support;
            rarity = Rarity.Standard;
            name = L("support.field_medic.name");
            description = L("support.field_medic.description", (int)HealAmount, (int)MaxHealthThreshold);
            flavourText = L("support.field_medic.flavour");
        }

        private static FieldMedic _instance;
        public static FieldMedic Instance => _instance ??= new();

        /// <summary>
        /// Struct to track active healing state
        /// </summary>
        public struct FieldMedicHealingState
        {
            public float remainingTime;
            public float totalDuration;
        }

        /// <summary>
        /// Apply healing to a unit
        /// </summary>
        public static void ApplyHealing(BattleUnit unit)
        {
            if (unit == null || !unit.IsAlive)
                return;

            // Get current health and max health
            float currentHealth = (float)_currentHealthField.GetValue(unit);
            float maxHealth = (float)_currentMaxHealthField.GetValue(unit);

            // Calculate new health (capped at max health)
            float newHealth = currentHealth + HealAmount;
            if (newHealth > maxHealth)
                newHealth = maxHealth;

            // Set the new health value
            _currentHealthField.SetValue(unit, newHealth);

            // Trigger health changed event manually using reflection
            var onHealthChangedField = AccessTools.Field(typeof(BattleUnit), "OnHealthChanged");
            var onHealthChanged = onHealthChangedField.GetValue(unit) as System.Action<float>;
            onHealthChanged?.Invoke(newHealth);

            MelonLogger.Msg($"FieldMedic: Healed unit {unit.UnitNameNoNumber} for {HealAmount} HP. Current health: {newHealth}/{maxHealth}");
        }
    }

    // Patch to clear state when mission starts
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public class FieldMedic_TestGame_StartGame_Patch
    {
        public static void Postfix()
        {
            FieldMedic.activeHealing.Clear();
        }
    }

    /// <summary>
    /// Patch to inject FieldMedicActionCard into InBattleActionCardsManager after initialization
    /// </summary>
    [HarmonyPatch(typeof(InBattleActionCardsManager), "Initialize")]
    public static class FieldMedic_InjectActionCard_Patch
    {
        public static void Postfix(InBattleActionCardsManager __instance)
        {
            if (!FieldMedic.Instance.IsActive)
                return;

            var actionCardInfo = new FieldMedicActionCardInfo();
            actionCardInfo.SetId("FieldMedic");

            var fieldMedicCard = new FieldMedicActionCard(actionCardInfo);

            __instance.InBattleActionCards.Add(fieldMedicCard);
        }
    }

    /// <summary>
    /// Field Medic action card - heals a unit with low health over time
    /// Implements IUnitTargetable to target player units
    /// </summary>
    public class FieldMedicActionCard : ActionCard, IUnitTargetable
    {
        public Team TeamToAffect => Team.Player;

        public FieldMedicActionCard(ActionCardInfo actionCardInfo)
        {
            Info = actionCardInfo;
            // Set to 0 for unlimited uses
            _usesLeft = 0;
        }

        public override ActionCard GetCopy()
        {
            return new FieldMedicActionCard(Info);
        }

        public void ApplyCommand(BattleUnit unit)
        {
            if (!FieldMedic.Instance.IsActive)
                return;

            if (unit == null || !unit.IsAlive || unit.Team != Team.Player)
                return;

            // Check if unit's health is below threshold
            if (unit.CurrentHealth >= FieldMedic.MaxHealthThreshold)
                return;

            // Check if unit is already being healed
            if (FieldMedic.activeHealing.ContainsKey(unit))
            {
                MelonLogger.Warning($"FieldMedic: Unit {unit.UnitNameNoNumber} is already being healed");
                return;
            }

            StartHealingProcess(unit);
        }

        private void StartHealingProcess(BattleUnit unit)
        {
            // Calculate actual duration based on unit's speed (same formula as DelayActionCardCommand)
            float actualDuration = BattleUnitStatsExpressions.GetCommandDurationBasedOnSpeed(FieldMedic.HealDuration, unit.Speed);

            // Track the healing state
            FieldMedic.activeHealing[unit] = new FieldMedic.FieldMedicHealingState
            {
                remainingTime = actualDuration,
                totalDuration = actualDuration
            };

            MelonLogger.Msg($"FieldMedic: Started healing unit {unit.UnitNameNoNumber} for {actualDuration}s (base: {FieldMedic.HealDuration}s, speed: {unit.Speed})");
        }

        IEnumerable<CommandsAvailabilityChecker.UnitAnavailableReasons> IUnitTargetable.IsUnitValid(BattleUnit unit)
        {
            var reasons = new List<CommandsAvailabilityChecker.UnitAnavailableReasons>();

            if (!FieldMedic.Instance.IsActive)
            {
                return reasons;
            }

            // Can only target alive units
            if (!unit.IsAlive)
            {
                reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.UnitIsDead);
            }
            // Can only target units with less than MaxHealthThreshold HP
            else if (unit.CurrentHealth >= FieldMedic.MaxHealthThreshold)
            {
                reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.InsufficientUnits);
            }
            // Can't target units already being healed
            else if (FieldMedic.activeHealing.ContainsKey(unit))
            {
                reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.AlreadyHasEffect);
            }

            return reasons;
        }
    }

    /// <summary>
    /// Custom ActionCardInfo for FieldMedic
    /// </summary>
    public class FieldMedicActionCardInfo : ActionCardInfo
    {
        public string CustomCardName => L("support.field_medic.card_name");

        public string CustomCardDescription => L("support.field_medic.card_description",
            (int)FieldMedic.HealAmount,
            (int)FieldMedic.MaxHealthThreshold);

        public FieldMedicActionCardInfo()
        {
            AccessTools.Field(typeof(ActionCardInfo), "_uses").SetValue(this, 0); // 0 = unlimited uses
            AccessTools.Field(typeof(ActionCardInfo), "canNotBeReplenished").SetValue(this, false);
        }
    }

    /// <summary>
    /// Patch to intercept CardName getter for FieldMedicActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardName", MethodType.Getter)]
    public static class FieldMedicActionCardInfo_CardName_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is FieldMedicActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardName;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to intercept CardDescription getter for FieldMedicActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardDescription", MethodType.Getter)]
    public static class FieldMedicActionCardInfo_CardDescription_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is FieldMedicActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardDescription;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to subscribe to time updates and OnDeath event for player units
    /// This handles the delayed healing process
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor, [typeof(UnitData), typeof(Enumerations.Team), typeof(GridManager)])]
    public class FieldMedic_BattleUnit_Constructor_Patch
    {
        public static void OnUpdate(BattleUnit __instance, float deltaTime)
        {
            if (!FieldMedic.Instance.IsActive)
                return;

            if (!FieldMedic.activeHealing.ContainsKey(__instance))
                return;

            // Decrease timer
            var healingState = FieldMedic.activeHealing[__instance];
            healingState.remainingTime -= deltaTime;

            // Check if healing completed or unit died
            if (healingState.remainingTime <= 0f && __instance.IsAlive)
            {
                // Apply the healing effect
                FieldMedic.ApplyHealing(__instance);
                FieldMedic.activeHealing.Remove(__instance);
            }
            else if (!__instance.IsAlive)
            {
                // Unit died during healing - cancel it
                FieldMedic.activeHealing.Remove(__instance);
                MelonLogger.Msg($"FieldMedic: Healing cancelled for {__instance.UnitNameNoNumber} (unit died)");
            }
            else
            {
                // Update the healing state
                FieldMedic.activeHealing[__instance] = healingState;
            }
        }

        public static void Postfix(BattleUnit __instance, Team team)
        {
            if (!FieldMedic.Instance.IsActive)
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
                    if (FieldMedic.activeHealing.ContainsKey(__instance))
                    {
                        FieldMedic.activeHealing.Remove(__instance);
                    }
                    __instance.OnDeath -= action;
                    TempSingleton<TimeManager>.Instance.OnTimeUpdated -= onUpdateAction;
                }

                __instance.OnDeath += action;
            }
        }
    }
}
