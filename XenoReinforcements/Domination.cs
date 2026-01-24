using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using System.Collections.Generic;
using System.Linq;

namespace XenopurgeRougeLike.XenoReinforcements
{
    /// <summary>
    /// 统治：控制当前场上所有异形，可使用1次
    /// Domination: Take control of all xenos on the battlefield, usable 1 time per mission
    /// </summary>
    public class Domination : Reinforcement
    {
        public Domination()
        {
            company = Company.Xeno;
            rarity = Rarity.Expert;
            stackable = false;
            name = "Domination";
            description = "Take control of all xenos on the battlefield. Usable 1 time per mission.";
            flavourText = "The hive mind bows before your absolute will.";
        }

        protected static Domination instance;
public static Domination Instance => instance ??= new();
    }

    /// <summary>
    /// Patch to inject DominationActionCard into InBattleActionCardsManager after initialization
    /// </summary>
    [HarmonyPatch(typeof(InBattleActionCardsManager), "Initialize")]
    public static class Domination_InjectActionCard_Patch
    {
        public static void Postfix(InBattleActionCardsManager __instance)
        {
            if (!Domination.Instance.IsActive)
                return;

            var actionCardInfo = new DominationActionCardInfo();
            actionCardInfo.SetId("Domination");

            var dominationCard = new DominationActionCard(actionCardInfo);

            __instance.InBattleActionCards.Add(dominationCard);
        }
    }

    /// <summary>
    /// Domination action card - converts all enemy units to the player's team.
    /// Implements INoTargetActionCard since it doesn't require targeting.
    /// </summary>
    public class DominationActionCard : ActionCard, INoTargetable
    {
        public DominationActionCard(ActionCardInfo actionCardInfo)
        {
            Info = actionCardInfo;
            _usesLeft = 1;
        }

        public override ActionCard GetCopy()
        {
            return new DominationActionCard(Info);
        }

        public void ApplyCommand()
        {
            if (!Domination.Instance.IsActive)
                return;

            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                MelonLogger.Warning("Domination: GameManager not found");
                return;
            }

            var enemyManager = gameManager.GetTeamManager(Enumerations.Team.EnemyAI);
            if (enemyManager == null)
            {
                MelonLogger.Warning("Domination: Enemy team manager not found");
                return;
            }

            // Get all living enemy units (create a copy of the list since we'll be modifying it)
            var enemyUnits = enemyManager.BattleUnits
                .Where(u => u != null && u.IsAlive)
                .ToList();

            if (enemyUnits.Count == 0)
            {
                MelonLogger.Msg("Domination: No enemy units to convert");
                return;
            }

            MelonLogger.Msg($"Domination: Converting {enemyUnits.Count} enemy units to player team");

            // Convert each enemy unit
            foreach (var enemyUnit in enemyUnits)
            {
                MindControlSystem.ConvertUnitToPlayer(enemyUnit);
            }

            MelonLogger.Msg("Domination: All enemy units have been converted");
        }

        public IEnumerable<CommandsAvailabilityChecker.CardUnavailableReason> IsCardValid()
        {
            // Card is always valid if there are enemy units on the battlefield
            // The base system already checks for uses left
            return [];
        }
    }

    /// <summary>
    /// Custom ActionCardInfo for Domination
    /// </summary>
    public class DominationActionCardInfo : ActionCardInfo
    {
        public string CustomCardName => "Domination";

        public string CustomCardDescription =>
            "Take control of ALL enemy units on the battlefield, permanently converting them to your team.";
    }

    /// <summary>
    /// Patch to intercept CardName getter for DominationActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardName", MethodType.Getter)]
    public static class DominationActionCardInfo_CardName_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is DominationActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardName;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to intercept CardDescription getter for DominationActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardDescription", MethodType.Getter)]
    public static class DominationActionCardInfo_CardDescription_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is DominationActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardDescription;
                return false;
            }
            return true;
        }
    }
}
