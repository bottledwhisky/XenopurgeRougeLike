using HarmonyLib;
using MelonLoader;
using SpaceCommander.ActionCards;
using SpaceCommander.Database;
using System.Linq;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SupportReinforcements
{
    /// <summary>
    /// 治疗针：任务开始时获得治疗针指令，如果已经获得则改为可用次数+1
    /// Healing Needle: At the start of a mission, gain Health Stim command. If already obtained, increase uses by 1.
    /// </summary>
    public class HealthStim : Reinforcement
    {
        // Health Stim card ID
        public const string HealthStimCardId = ActionCardIds.INJECT_HEALTH_STIM;

        public HealthStim()
        {
            company = Company.Support;
            rarity = Rarity.Standard;
            name = L("support.health_stim.name");
            description = L("support.health_stim.description");
            flavourText = L("support.health_stim.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("support.health_stim.description");
        }

        private static HealthStim _instance;
        public static HealthStim Instance => _instance ??= new();

        /// <summary>
        /// Add Health Stim card or increase its uses at mission start
        /// </summary>
        public static void AddHealthStimAtMissionStart()
        {
            if (!Instance.IsActive)
                return;

            MelonLogger.Msg("HealthStim: Adding Health Stim card at mission start");

            var gainedActionCards = InBattleActionCardsManager.Instance.InBattleActionCards;

            // Get the Health Stim card from the database
            var healthStimCardSO = Singleton<AssetsDatabase>.Instance.GetActionCardSO(HealthStimCardId);
            if (healthStimCardSO == null)
            {
                MelonLogger.Error($"HealthStim: Failed to find Health Stim card with ID {HealthStimCardId}");
                return;
            }

            // Check if the card already exists in the player's current action cards
            var existingCard = gainedActionCards.FirstOrDefault(c => c.Info.Id == HealthStimCardId);
            if (existingCard != null)
            {
                // If it already exists, increase its uses by 1
                int currentUses = existingCard.UsesLeft;
                AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(existingCard, currentUses + 1);
                MelonLogger.Msg($"HealthStim: Increased Health Stim uses from {currentUses} to {currentUses + 1}");
            }
            else
            {
                // Otherwise, add it as a new card with 1 use
                var card = healthStimCardSO.CreateInstance();
                AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(card, 1);
                gainedActionCards.Add(card);
                MelonLogger.Msg($"HealthStim: Added new Health Stim card with 1 use");
            }
        }
    }

    /// <summary>
    /// Patch to add Health Stim card when mission starts
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    [HarmonyPriority(Priority.High)]
    public static class HealthStim_StartGame_Patch
    {
        public static void Postfix()
        {
            HealthStim.AddHealthStimAtMissionStart();
        }
    }
}
