using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using System.Collections.Generic;
using UnityEngine;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.GunslingerReinforcements
{
    /// <summary>
    /// 死神之眼：获得"死神之眼"指令，限一次，所有友方单位后在15秒内所有远程攻击必定命中
    /// Death's Eye: Gain "Death's Eye" command, usable once, all friendly units' ranged attacks guaranteed to hit for 15 seconds
    /// </summary>
    public class DeathsEye : Reinforcement
    {
        public const float Duration = 15f;

        public DeathsEye()
        {
            company = Company.Gunslinger;
            rarity = Rarity.Elite;
            name = L("gunslinger.deaths_eye.name");
            flavourText = L("gunslinger.deaths_eye.flavour");
            description = L("gunslinger.deaths_eye.description", (int)Duration);
        }

        protected static DeathsEye instance;
        public static DeathsEye Instance => instance ??= new();

        // Track if Death's Eye buff is active
        public static bool IsBuffActive { get; private set; }
        public static float RemainingTime { get; private set; }

        public static void ActivateBuff()
        {
            IsBuffActive = true;
            RemainingTime = Duration;
            MelonLogger.Msg($"DeathsEye: Activated! All ranged attacks will hit for {Duration} seconds.");
        }

        public static void UpdateBuff(float deltaTime)
        {
            if (!IsBuffActive)
                return;

            RemainingTime -= deltaTime;
            if (RemainingTime <= 0)
            {
                DeactivateBuff();
            }
        }

        public static void DeactivateBuff()
        {
            IsBuffActive = false;
            RemainingTime = 0;
            MelonLogger.Msg("DeathsEye: Effect ended.");
        }
    }

    // Patch to clear state when mission starts
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public class DeathsEye_TestGame_StartGame_Patch
    {
        public static void Postfix()
        {
            DeathsEye.DeactivateBuff();
        }
    }

    /// <summary>
    /// Patch to inject DeathsEyeActionCard into InBattleActionCardsManager after initialization
    /// </summary>
    [HarmonyPatch(typeof(InBattleActionCardsManager), "Initialize")]
    public static class DeathsEye_InjectActionCard_Patch
    {
        public static void Postfix(InBattleActionCardsManager __instance)
        {
            if (!DeathsEye.Instance.IsActive)
                return;

            // Create custom ActionCardInfo
            var actionCardInfo = new DeathsEyeActionCardInfo();
            actionCardInfo.SetId("DeathsEye");

            // Create and add the DeathsEyeActionCard instance
            var deathsEyeCard = new DeathsEyeActionCard(actionCardInfo);

            // Add to the InBattleActionCards list
            __instance.InBattleActionCards.Add(deathsEyeCard);
        }
    }

    /// <summary>
    /// Death's Eye action card - makes all friendly ranged attacks guaranteed hits for 15 seconds
    /// Implements INoTargetable as it affects all units without targeting
    /// </summary>
    public class DeathsEyeActionCard : ActionCard, INoTargetable
    {
        public DeathsEyeActionCard(ActionCardInfo actionCardInfo)
        {
            Info = actionCardInfo;
        }

        public override ActionCard GetCopy()
        {
            return new DeathsEyeActionCard(Info);
        }

        public void ApplyCommand()
        {
            if (!DeathsEye.Instance.IsActive)
                return;

            DeathsEye.ActivateBuff();
        }

        IEnumerable<CommandsAvailabilityChecker.CardUnavailableReason> INoTargetable.IsCardValid()
        {
            var reasons = new List<CommandsAvailabilityChecker.CardUnavailableReason>();

            // Only available if DeathsEye reinforcement is active
            if (!DeathsEye.Instance.IsActive)
            {
                reasons.Add(CommandsAvailabilityChecker.CardUnavailableReason.ObjectiveNotFoundYet);
            }

            // Can't use if already active
            if (DeathsEye.IsBuffActive)
            {
                reasons.Add(CommandsAvailabilityChecker.CardUnavailableReason.ObjectiveNotFoundYet);
            }

            return reasons;
        }
    }

    /// <summary>
    /// Custom ActionCardInfo for DeathsEye
    /// </summary>
    public class DeathsEyeActionCardInfo : ActionCardInfo
    {
        public string CustomCardName => L("gunslinger.deaths_eye.card_name");

        public string CustomCardDescription => L("gunslinger.deaths_eye.card_description", (int)DeathsEye.Duration);

        public DeathsEyeActionCardInfo()
        {
            // Set uses to 1 (one-time use)
            AccessTools.Field(typeof(ActionCardInfo), "_uses").SetValue(this, 1);
            AccessTools.Field(typeof(ActionCardInfo), "canNotBeReplenished").SetValue(this, false);
        }
    }

    /// <summary>
    /// Patch to intercept CardName getter for DeathsEyeActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardName", MethodType.Getter)]
    public static class DeathsEyeActionCardInfo_CardName_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is DeathsEyeActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardName;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to intercept CardDescription getter for DeathsEyeActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardDescription", MethodType.Getter)]
    public static class DeathsEyeActionCardInfo_CardDescription_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is DeathsEyeActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardDescription;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch BattleUnitStatsExpressions.IsRangedHitAccurate to guarantee hits when Death's Eye is active
    /// </summary>
    [HarmonyPatch(typeof(BattleUnitStatsExpressions), "IsRangedHitAccurate")]
    public static class DeathsEye_IsRangedHitAccurate_Patch
    {
        public static bool Prefix(ref bool __result)
        {
            if (!DeathsEye.Instance.IsActive)
                return true;

            if (DeathsEye.IsBuffActive)
            {
                // Guarantee hit
                __result = true;
                return false; // Skip original method
            }

            return true; // Run original method
        }
    }

    /// <summary>
    /// Patch TestGame.Update to update Death's Eye timer
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "Update")]
    public static class DeathsEye_TestGame_Update_Patch
    {
        public static void Postfix()
        {
            if (!DeathsEye.Instance.IsActive)
                return;

            DeathsEye.UpdateBuff(Time.deltaTime);
        }
    }

    /// <summary>
    /// Clear buff when game ends
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "EndGame")]
    public static class DeathsEye_ClearBuff_Patch
    {
        public static void Postfix()
        {
            DeathsEye.DeactivateBuff();
        }
    }
}
