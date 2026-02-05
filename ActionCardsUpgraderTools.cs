using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.PartyCustomization;
using SpaceCommander.PersistentProgression;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XenopurgeRougeLike
{
    /// <summary>
    /// Configuration for modifying action card appearance probability in the shop
    /// </summary>
    public class ActionCardProbabilityModifier
    {
        /// <summary>
        /// List of action card IDs to boost
        /// </summary>
        public List<string> CardIds { get; set; } = new();

        /// <summary>
        /// Number of additional copies to add to the pool (increases probability)
        /// </summary>
        public int AdditionalCopies { get; set; } = 1;

        /// <summary>
        /// Condition to check if this modifier should be active
        /// </summary>
        public Func<bool> IsActive { get; set; } = () => true;

        public ActionCardProbabilityModifier(List<string> cardIds, int additionalCopies, Func<bool> isActive = null)
        {
            CardIds = cardIds ?? new();
            AdditionalCopies = additionalCopies;
            IsActive = isActive ?? (() => true);
        }
    }

    public static class ActionCardsUpgraderTools
    {
        /// <summary>
        /// Dictionary of registered probability modifiers
        /// Key: Unique identifier for the modifier
        /// Value: ActionCardProbabilityModifier configuration
        /// </summary>
        public static Dictionary<string, ActionCardProbabilityModifier> ProbabilityModifiers = new();

        /// <summary>
        /// Register a new probability modifier for specific action cards
        /// </summary>
        public static void RegisterProbabilityModifier(string id, List<string> cardIds, int additionalCopies, Func<bool> isActive = null)
        {
            ProbabilityModifiers[id] = new ActionCardProbabilityModifier(cardIds, additionalCopies, isActive);
            MelonLogger.Msg($"ActionCardsUpgraderTools: Registered probability modifier '{id}' for {cardIds.Count} card(s) with {additionalCopies} additional copies");
        }

        /// <summary>
        /// Remove a probability modifier by ID
        /// </summary>
        public static void UnregisterProbabilityModifier(string id)
        {
            if (ProbabilityModifiers.Remove(id))
            {
                MelonLogger.Msg($"ActionCardsUpgraderTools: Unregistered probability modifier '{id}'");
            }
        }

        /// <summary>
        /// Apply all active probability modifiers to weighted card choices
        /// </summary>
        internal static void ApplyProbabilityModifiers(List<Tuple<int, string>> weightedChoices)
        {
            if (weightedChoices == null || weightedChoices.Count == 0)
                return;

            foreach (var modifierPair in ProbabilityModifiers)
            {
                var id = modifierPair.Key;
                var modifier = modifierPair.Value;

                // Check if this modifier should be active
                if (!modifier.IsActive())
                    continue;

                // For each weighted choice
                for (int i = 0; i < weightedChoices.Count; i++)
                {
                    var cardId = weightedChoices[i].Item2;

                    // Check if this card should get a weight boost
                    if (modifier.CardIds.Contains(cardId))
                    {
                        // Increase the weight by adding additional copies worth of weight
                        int currentWeight = weightedChoices[i].Item1;
                        int newWeight = currentWeight * (1 + modifier.AdditionalCopies);
                        weightedChoices[i] = new Tuple<int, string>(newWeight, cardId);

                        MelonLogger.Msg($"ActionCardsUpgraderTools: Applied modifier '{id}' - boosted card {cardId} weight from {currentWeight} to {newWeight}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Patch for ActionCardsUpgrader.ChooseActionCards to modify card selection probability
    /// </summary>
    [HarmonyPatch(typeof(ActionCardsUpgrader), "ChooseActionCards")]
    public static class ActionCardsUpgrader_ChooseActionCards_Patch
    {
        // Access private field _actionCardsAvailableToBuy
        private static readonly AccessTools.FieldRef<ActionCardsUpgrader, List<string>> _actionCardsAvailableToBuyRef =
            AccessTools.FieldRefAccess<ActionCardsUpgrader, List<string>>("_actionCardsAvailableToBuy");

        // Access private field _playerWalletData
        private static readonly AccessTools.FieldRef<ActionCardsUpgrader, PlayerWalletData> _playerWalletDataRef =
            AccessTools.FieldRefAccess<ActionCardsUpgrader, PlayerWalletData>("_playerWalletData");

        // Access private field _amountOfRefreshes
        private static readonly AccessTools.FieldRef<ActionCardsUpgrader, int> _amountOfRefreshesRef =
            AccessTools.FieldRefAccess<ActionCardsUpgrader, int>("_amountOfRefreshes");

        public static bool Prefix(ActionCardsUpgrader __instance, bool isRefreshAction = false)
        {
            try
            {
                // Only apply our custom logic if there are active probability modifiers
                if (ActionCardsUpgraderTools.ProbabilityModifiers.Count == 0 ||
                    !ActionCardsUpgraderTools.ProbabilityModifiers.Any(m => m.Value.IsActive()))
                {
                    // No active modifiers, use original logic
                    return true;
                }

                MelonLogger.Msg("ActionCardsUpgrader_ChooseActionCards_Patch: Applying custom card selection with probability modifiers");

                // Get field references
                ref List<string> actionCardsAvailableToBuy = ref _actionCardsAvailableToBuyRef(__instance);
                ref PlayerWalletData playerWalletData = ref _playerWalletDataRef(__instance);
                ref int amountOfRefreshes = ref _amountOfRefreshesRef(__instance);

                // Handle refresh cost
                if (isRefreshAction)
                {
                    int refreshCost = amountOfRefreshes + 1;
                    playerWalletData.ChangeCoinsByValue(-refreshCost);
                    amountOfRefreshes++;
                }
                else
                {
                    amountOfRefreshes = 0;
                }

                // Clear previous selection
                actionCardsAvailableToBuy.Clear();

                // Get cards the player already owns
                var playerCards = Singleton<Player>.Instance.PlayerData.ActionCardListData.ActionCards
                    .Select(card => card.Info.Id).ToList();

                // Get all available cards from database
                var allCards = Singleton<SpaceCommander.Database.AssetsDatabase>.Instance.ActionCards
                    .Where(card => !card.AvailableOnDeploymentPhase).ToList();

                // Get persistent progression manager
                var persistentProgressionManager = Singleton<PersistentProgressionManager>.Instance;
                var startingSquadId = Singleton<Player>.Instance.PlayerData.Squad.StartingSquadId;

                // Build weighted list of cards that are unlocked and not owned
                // Each card starts with weight of 1
                List<Tuple<int, string>> weightedChoices = [];

                foreach (var item in allCards)
                {
                    bool isOwned = playerCards.Contains(item.Id);
                    bool isUnlocked = persistentProgressionManager.IsCommandUnlocked(item.Id);
                    bool canUseBySquad = !item.Info.SquadIdsThatCannotUseCommand.Contains(startingSquadId);
                    bool accessPointsAllowed = Singleton<Player>.Instance.PlayerData.AccessPointsSystemEnabled ||
                                               item.Info.Group != SpaceCommander.ActionCards.ActionCardInfo.ActionCardGroup.accessPoints;
                    bool bioweaveAllowed = Singleton<Player>.Instance.PlayerData.BioweaveSystemEnabled ||
                                          item.Info.Group != SpaceCommander.ActionCards.ActionCardInfo.ActionCardGroup.bioweavePoints;

                    if (!isOwned && isUnlocked && canUseBySquad && accessPointsAllowed && bioweaveAllowed)
                    {
                        weightedChoices.Add(new Tuple<int, string>(1, item.Id)); // Default weight = 1
                    }
                }

                MelonLogger.Msg($"ActionCardsUpgrader_ChooseActionCards_Patch: Found {weightedChoices.Count} available cards");

                // Apply probability modifiers to increase chances of specific cards
                ActionCardsUpgraderTools.ApplyProbabilityModifiers(weightedChoices);

                // Handle special priority for access points and bioweave cards
                string priorityCard = string.Empty;
                var availableCardIds = weightedChoices.Select(wc => wc.Item2).ToList();

                if (Singleton<Player>.Instance.PlayerData.AccessPointsSystemEnabled)
                {
                    var accessPointsCards = allCards
                        .Where(card => card.Info.Group == SpaceCommander.ActionCards.ActionCardInfo.ActionCardGroup.accessPoints &&
                                      availableCardIds.Contains(card.Id))
                        .ToList();
                    if (accessPointsCards.Count > 0)
                    {
                        priorityCard = accessPointsCards[UnityEngine.Random.Range(0, accessPointsCards.Count)].Id;
                        // Remove this card from the weighted pool
                        weightedChoices.RemoveAll(wc => wc.Item2 == priorityCard);
                    }
                }

                if (Singleton<Player>.Instance.PlayerData.BioweaveSystemEnabled && string.IsNullOrEmpty(priorityCard))
                {
                    availableCardIds = [.. weightedChoices.Select(wc => wc.Item2)];
                    var bioweaveCards = allCards
                        .Where(card => card.Info.Group == SpaceCommander.ActionCards.ActionCardInfo.ActionCardGroup.bioweavePoints &&
                                      availableCardIds.Contains(card.Id))
                        .ToList();
                    if (bioweaveCards.Count > 0)
                    {
                        priorityCard = bioweaveCards[UnityEngine.Random.Range(0, bioweaveCards.Count)].Id;
                        // Remove this card from the weighted pool
                        weightedChoices.RemoveAll(wc => wc.Item2 == priorityCard);
                    }
                }

                // Calculate how many cards to show in shop
                var upgradeDataSO = Singleton<SpaceCommander.Database.AssetsDatabase>.Instance.UpgradeDataSO;
                var difficultiesController = Singleton<SpaceCommander.Difficulties.DifficultiesController>.Instance;
                int difficultyReduction = difficultiesController.GetDifficultyPerLevel().StoreOptionDecreaseCommands;
                int extraSlots = persistentProgressionManager.GetStoreExtraSlots(Enumerations.StoreType.ActionCards);
                int amountToShow = upgradeDataSO.AmountOfActionCardsToBuy + extraSlots - difficultyReduction;

                // Select random cards using weighted random selection (like GetChoices)
                if (weightedChoices.Count > 0)
                {
                    int totalWeight = weightedChoices.Sum(wc => wc.Item1);
                    Random rng = new();

                    for (int i = 0; i < amountToShow && weightedChoices.Count > 0; i++)
                    {
                        int roll = rng.Next(totalWeight);
                        int cumulative = 0;

                        for (int j = 0; j < weightedChoices.Count; j++)
                        {
                            cumulative += weightedChoices[j].Item1;
                            if (roll < cumulative)
                            {
                                actionCardsAvailableToBuy.Add(weightedChoices[j].Item2);
                                totalWeight -= weightedChoices[j].Item1;
                                weightedChoices.RemoveAt(j); // Remove to avoid duplicates
                                break;
                            }
                        }
                    }

                    // Replace last slot with priority card if we have one
                    if (!string.IsNullOrEmpty(priorityCard))
                    {
                        if (actionCardsAvailableToBuy.Count > 0)
                        {
                            actionCardsAvailableToBuy.RemoveAt(actionCardsAvailableToBuy.Count - 1);
                        }
                        actionCardsAvailableToBuy.Add(priorityCard);
                    }
                }

                MelonLogger.Msg($"ActionCardsUpgrader_ChooseActionCards_Patch: Selected {actionCardsAvailableToBuy.Count} cards for shop");

                // Save game state
                Singleton<SaveSystem.SaveLoadManager>.Instance.SaveEntityState(Singleton<Player>.Instance);

                // Skip original method
                return false;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"ActionCardsUpgrader_ChooseActionCards_Patch error: {ex}");
                // Fall back to original method on error
                return true;
            }
        }
    }
}
