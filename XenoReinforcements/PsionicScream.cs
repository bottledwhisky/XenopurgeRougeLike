using HarmonyLib;
using SpaceCommander;
using SpaceCommander.ActionCards;
using System.Collections.Generic;
using System.Linq;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.XenoReinforcements
{
    /// <summary>
    /// 灵能尖啸：眩晕所有异形10秒，可使用1/2次
    /// Psionic Scream: Stun all xenos for 5 seconds, usable 1/2 times
    /// </summary>
    public class PsionicScream : Reinforcement
    {
        public static readonly int[] UsesPerStack = [1, 2];
        public const float BaseStunDuration = 10f;
        public static float StunDuration => 10f + 5 * Xeno.GetControlDurationBonusLevel();

        public PsionicScream()
        {
            company = Company.Xeno;
            stackable = true;
            maxStacks = 2;
            name = L("xeno.psionic_scream.name");
            description = L("xeno.psionic_scream.description");
            flavourText = L("xeno.psionic_scream.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("xeno.psionic_scream.description", StunDuration, UsesPerStack[stacks - 1]);
        }

        protected static PsionicScream instance;
        public static PsionicScream Instance => instance ??= new();
    }

    /// <summary>
    /// Patch to inject PsionicScreamActionCard into InBattleActionCardsManager after initialization
    /// </summary>
    [HarmonyPatch(typeof(InBattleActionCardsManager), "Initialize")]
    public static class PsionicScream_InjectActionCard_Patch
    {
        public static void Postfix(InBattleActionCardsManager __instance)
        {
            if (!PsionicScream.Instance.IsActive)
                return;

            // Create custom ActionCardInfo
            var actionCardInfo = new PsionicScreamActionCardInfo();
            actionCardInfo.SetId("PsionicScream");

            // Create and add the PsionicScreamActionCard instance
            var screamCard = new PsionicScreamActionCard(actionCardInfo);

            // Add to the InBattleActionCards list
            __instance.InBattleActionCards.Add(screamCard);
        }
    }

    /// <summary>
    /// Psionic Scream action card - stuns all xenos on the map for 5 seconds.
    /// Implements INoTargetable as it doesn't require targeting a specific unit.
    /// </summary>
    public class PsionicScreamActionCard : ActionCard, INoTargetable
    {
        public PsionicScreamActionCard(ActionCardInfo actionCardInfo)
        {
            Info = actionCardInfo;
            // Set uses based on current stacks
            _usesLeft = PsionicScream.UsesPerStack[PsionicScream.Instance.currentStacks - 1];
        }

        public override ActionCard GetCopy()
        {
            return new PsionicScreamActionCard(Info);
        }

        public void ApplyCommand()
        {
            if (!PsionicScream.Instance.IsActive)
                return;

            var gameManager = GameManager.Instance;
            if (gameManager == null)
                return;

            // Find all enemy units on the battlefield
            var pm = gameManager.GetTeamManager(Team.EnemyAI);

            var enemies = pm.BattleUnits.Where(u => u.IsAlive).ToList();

            // Stun all enemies using the centralized stun controller
            foreach (var enemy in enemies)
            {
                CentralizedStunController.StunUnit(enemy, PsionicScream.StunDuration, "PsionicScream");
            }
        }

        IEnumerable<CommandsAvailabilityChecker.CardUnavailableReason> INoTargetable.IsCardValid()
        {
            var reasons = new List<CommandsAvailabilityChecker.CardUnavailableReason>();

            // Only available if PsionicScream reinforcement is active
            if (!PsionicScream.Instance.IsActive)
            {
                reasons.Add(CommandsAvailabilityChecker.CardUnavailableReason.ObjectiveNotFoundYet);
                return reasons;
            }

            // Check if there are any enemies to stun
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                var pm = gameManager.GetTeamManager(Team.EnemyAI);

                var enemies = pm.BattleUnits.Where(u => u.IsAlive).ToList();
                if (!enemies.Any(u => u.IsAlive))
                {
                    reasons.Add(CommandsAvailabilityChecker.CardUnavailableReason.ObjectiveNotFoundYet);
                }
            }

            return reasons;
        }
    }

    /// <summary>
    /// Custom ActionCardInfo for PsionicScream
    /// </summary>
    public class PsionicScreamActionCardInfo : ActionCardInfo
    {
        public string CustomCardName => L("xeno.psionic_scream.name");

        public string CustomCardDescription =>
            L("xeno.psionic_scream.card_description", PsionicScream.StunDuration);

        public PsionicScreamActionCardInfo()
        {
            // Set uses based on current stacks
            int uses = PsionicScream.UsesPerStack[PsionicScream.Instance.currentStacks - 1];
            AccessTools.Field(typeof(ActionCardInfo), "_uses").SetValue(this, uses);
            AccessTools.Field(typeof(ActionCardInfo), "canNotBeReplenished").SetValue(this, false);
        }
    }

    /// <summary>
    /// Patch to intercept CardName getter for PsionicScreamActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardName", MethodType.Getter)]
    public static class PsionicScreamActionCardInfo_CardName_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is PsionicScreamActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardName;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to intercept CardDescription getter for PsionicScreamActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardDescription", MethodType.Getter)]
    public static class PsionicScreamActionCardInfo_CardDescription_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is PsionicScreamActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardDescription;
                return false;
            }
            return true;
        }
    }

    // Note: Stun system is now handled by CentralizedStunController
    // No need for individual stun patches per reinforcement
}
