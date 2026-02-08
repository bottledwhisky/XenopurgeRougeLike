using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Commands;
using System;
using System.Collections.Generic;
using System.Reflection;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SupportReinforcements
{
    /// <summary>
    /// 战地急救：获得战地急救指令，恢复一名生命值低于50%的队友10点生命值。
    /// Field Medic: Gain a Field Medic action card that restores 10 HP to an ally with less than 50% HP.
    /// 实现细节：作为覆盖指令，显示计时器，无限使用
    /// Implementation: Works as override command, shows timer, unlimited uses
    /// </summary>
    public class FieldMedic : Reinforcement
    {
        // Reflection field to access BattleUnit's private _currentHealth field
        internal static readonly FieldInfo _currentHealthField = AccessTools.Field(typeof(BattleUnit), "_currentHealth");
        internal static readonly FieldInfo _currentMaxHealthField = AccessTools.Field(typeof(BattleUnit), "_currentMaxHealth");

        // Healing amount
        public const float HealAmount = 10f;

        // Health threshold percentage (0.5 = 50%)
        public const float HealthThresholdPercent = 0.5f;

        // Duration of the healing process (in seconds) - same as Apply First Aid Kit
        public const float HealDuration = 7.4f;

        public FieldMedic()
        {
            company = Company.Support;
            rarity = Rarity.Standard;
            name = L("support.field_medic.name");
            description = L("support.field_medic.description", (int)HealAmount, (int)(HealthThresholdPercent * 100));
            flavourText = L("support.field_medic.flavour");
        }

        private static FieldMedic _instance;
        public static FieldMedic Instance => _instance ??= new();

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
            var onHealthChanged = onHealthChangedField.GetValue(unit) as Action<float>;
            onHealthChanged?.Invoke(newHealth);

            MelonLogger.Msg($"FieldMedic: Healed unit {unit.UnitNameNoNumber} for {HealAmount} HP. Current health: {newHealth}/{maxHealth}");
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

            var fieldMedicCard = new FieldMedicOuterCard(actionCardInfo, new FieldMedicDelayCommandDataSO());

            __instance.InBattleActionCards.Add(fieldMedicCard);
        }
    }

    /// <summary>
    /// The outer action card that appears in the card list
    /// Uses OverrideCommands_UnitAsTarget_Card pattern to create DelayActionCardCommand
    /// </summary>
    public class FieldMedicOuterCard : ActionCard, IUnitTargetable
    {
        private static readonly FieldInfo _currentMaxHealthField = AccessTools.Field(typeof(BattleUnit), "_currentMaxHealth");
        private readonly DelayCardCommandDataSO _commandDataSO;

        public Team TeamToAffect => Team.Player;

        public FieldMedicOuterCard(ActionCardInfo actionCardInfo, DelayCardCommandDataSO commandDataSO)
        {
            Info = actionCardInfo;
            _commandDataSO = commandDataSO;
            // Set to 0 for unlimited uses
            _usesLeft = 0;
        }

        public override ActionCard GetCopy()
        {
            return new FieldMedicOuterCard(Info, _commandDataSO);
        }

        public void ApplyCommand(BattleUnit unit)
        {
            if (!FieldMedic.Instance.IsActive)
                return;

            if (unit == null || !unit.IsAlive || unit.Team != Team.Player)
                return;

            // Check if unit's health is below threshold (50% max HP)
            float maxHealth = (float)_currentMaxHealthField.GetValue(unit);
            float healthThreshold = maxHealth * FieldMedic.HealthThresholdPercent;
            if (unit.CurrentHealth >= healthThreshold)
                return;

            // Create DelayActionCardCommand using the game's built-in system
            DelayActionCardCommand delayCommand = new DelayActionCardCommand(unit);
            delayCommand.InitializeValues(_commandDataSO);

            // Override current command with heal command
            ActionCard.CostOfActionCard costOfActionCard = default;
            costOfActionCard.ActionCardId = Info.Id;
            unit.CommandsManager.OverrideCurrentCommandFromActionCard(delayCommand, costOfActionCard);
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
            // Can only target units with less than 50% HP
            else if (unit.CurrentHealth >= (float)FieldMedic._currentMaxHealthField.GetValue(unit) * FieldMedic.HealthThresholdPercent)
            {
                reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.InsufficientUnits);
            }
            // Can't target units in close combat
            else if (unit.CommandsManager.IsEngagedInCloseCombat)
            {
                reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.UnitIsEngagedInCloseCombat);
            }
            // Can't target units already being healed by this command
            else if (unit.CommandsManager.CurrentCommand is DelayActionCardCommand delayCmd)
            {
                var cmdData = delayCmd.CommandData as DelayCardCommandDataSO;
                if (cmdData is FieldMedicDelayCommandDataSO)
                {
                    reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.AlreadyHasEffect);
                }
            }

            return reasons;
        }

        public IEnumerable<CommandsAvailabilityChecker.CardUnavailableReason> IsCardValid()
        {
            var reasons = new List<CommandsAvailabilityChecker.CardUnavailableReason>();

            if (!FieldMedic.Instance.IsActive)
            {
                reasons.Add(CommandsAvailabilityChecker.CardUnavailableReason.ObjectiveNotFoundYet);
                return reasons;
            }

            // Check if there are any valid units that can be healed
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                var pm = gameManager.GetTeamManager(Team.Player);
                var playerUnits = pm.BattleUnits;

                // Check if any unit is valid for healing
                bool hasValidTarget = false;
                foreach (var unit in playerUnits)
                {
                    if (unit.IsAlive &&
                        unit.CurrentHealth < (float)_currentMaxHealthField.GetValue(unit) * FieldMedic.HealthThresholdPercent &&
                        !unit.CommandsManager.IsEngagedInCloseCombat)
                    {
                        // Check if not already being healed
                        if (unit.CommandsManager.CurrentCommand is DelayActionCardCommand delayCmd)
                        {
                            var cmdData = delayCmd.CommandData as DelayCardCommandDataSO;
                            if (cmdData is FieldMedicDelayCommandDataSO)
                            {
                                continue; // Skip this unit, already being healed
                            }
                        }
                        hasValidTarget = true;
                        break;
                    }
                }

                if (!hasValidTarget)
                {
                    reasons.Add(CommandsAvailabilityChecker.CardUnavailableReason.ObjectiveNotFoundYet);
                }
            }

            return reasons;
        }
    }

    /// <summary>
    /// Custom ActionCardInfo for FieldMedic outer card
    /// </summary>
    public class FieldMedicActionCardInfo : ActionCardInfo
    {
        public string CustomCardName => L("support.field_medic.card_name");

        public string CustomCardDescription => L("support.field_medic.card_description",
            (int)FieldMedic.HealAmount,
            (int)(FieldMedic.HealthThresholdPercent * 100));

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
    /// DelayCardCommandDataSO that references the inner heal action card
    /// This is used by DelayActionCardCommand to know what to execute after the delay
    /// </summary>
    public class FieldMedicDelayCommandDataSO : DelayCardCommandDataSO
    {
        private static FieldMedicInnerActionCardSO _actionCardSO;

        public string CustomCommandName => L("support.field_medic.command_name");
        public string CustomCommandDescription => L("support.field_medic.command_description", (int)FieldMedic.HealAmount);

        public FieldMedicDelayCommandDataSO()
        {
            if (_actionCardSO == null)
            {
                _actionCardSO = new FieldMedicInnerActionCardSO();
            }

            // Set the action card using reflection
            AccessTools.Field(typeof(DelayCardCommandDataSO), "_actionCard").SetValue(this, _actionCardSO);

            // Set command properties using reflection
            AccessTools.Field(typeof(CommandDataSO), "_id").SetValue(this, Guid.NewGuid().ToString());
            AccessTools.Field(typeof(CommandDataSO), "_commandDuration").SetValue(this, FieldMedic.HealDuration);
            AccessTools.Field(typeof(CommandDataSO), "_marineState").SetValue(this, MarineState.Neutral);
            AccessTools.Field(typeof(CommandDataSO), "_commandCategory").SetValue(this, CommandCategories.Move);
            AccessTools.Field(typeof(CommandDataSO), "_showTimer").SetValue(this, true);
            AccessTools.Field(typeof(CommandDataSO), "_isOverrideCommand").SetValue(this, true);
        }
    }

    /// <summary>
    /// Patch to intercept CommandName getter for FieldMedicDelayCommandDataSO
    /// </summary>
    [HarmonyPatch(typeof(CommandDataSO), "CommandName", MethodType.Getter)]
    public static class FieldMedicDelayCommandDataSO_CommandName_Patch
    {
        public static bool Prefix(CommandDataSO __instance, ref string __result)
        {
            if (__instance is FieldMedicDelayCommandDataSO customData)
            {
                __result = customData.CustomCommandName;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to intercept CommandDescription getter for FieldMedicDelayCommandDataSO
    /// </summary>
    [HarmonyPatch(typeof(CommandDataSO), "CommandDescription", MethodType.Getter)]
    public static class FieldMedicDelayCommandDataSO_CommandDescription_Patch
    {
        public static bool Prefix(CommandDataSO __instance, ref string __result)
        {
            if (__instance is FieldMedicDelayCommandDataSO customData)
            {
                __result = customData.CustomCommandDescription;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Inner ActionCardSO that represents what happens after the delay
    /// This is the "effect" that DelayActionCardCommand will trigger
    /// </summary>
    public class FieldMedicInnerActionCardSO : ActionCardSO
    {
        public FieldMedicInnerActionCardSO()
        {
            // Initialize the _actionCardInfo field using reflection
            var info = new FieldMedicInnerActionCardInfo();
            info.SetId("FieldMedicInner");
            AccessTools.Field(typeof(ActionCardSO), "_actionCardInfo").SetValue(this, info);
        }

        public override ActionCard CreateInstance()
        {
            return new FieldMedicInnerActionCard(Info);
        }
    }

    /// <summary>
    /// Inner action card info (not shown to user, just for internal use)
    /// </summary>
    public class FieldMedicInnerActionCardInfo : ActionCardInfo
    {
        public FieldMedicInnerActionCardInfo()
        {
            AccessTools.Field(typeof(ActionCardInfo), "_uses").SetValue(this, 0);
            AccessTools.Field(typeof(ActionCardInfo), "canNotBeReplenished").SetValue(this, false);
        }
    }

    /// <summary>
    /// Inner action card that applies the heal effect
    /// This is triggered by DelayActionCardCommand after the timer
    /// </summary>
    public class FieldMedicInnerActionCard : ActionCard, IUnitTargetable
    {
        public Team TeamToAffect => Team.Player;

        public FieldMedicInnerActionCard(ActionCardInfo info)
        {
            Info = info;
            _usesLeft = 0;
        }

        public override ActionCard GetCopy()
        {
            return new FieldMedicInnerActionCard(Info);
        }

        public void ApplyCommand(BattleUnit unit)
        {
            // This is called by DelayActionCardCommand when timer completes
            FieldMedic.ApplyHealing(unit);
        }

        IEnumerable<CommandsAvailabilityChecker.UnitAnavailableReasons> IUnitTargetable.IsUnitValid(BattleUnit unit)
        {
            // Always valid when called from DelayActionCardCommand
            return new List<CommandsAvailabilityChecker.UnitAnavailableReasons>();
        }
    }
}
