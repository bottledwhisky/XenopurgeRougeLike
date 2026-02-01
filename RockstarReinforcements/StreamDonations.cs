using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Area;
using SpaceCommander.BattleManagement;
using SpaceCommander.BattleManagement.UI;
using SpaceCommander.Database;
using SpaceCommander.EndGame;
using SpaceCommander.GameFlow;
using SpaceCommander.UI;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.RockstarReinforcements
{
    // 直播打赏
    // Stream Donations
    // When eliminating enemies in battle, randomly gain consumable charges based on fan count.
    // 直播打赏II
    // Stream Donations II
    // Increases the chance of receiving donations.
    public class StreamDonations : Reinforcement
    {
        public static Dictionary<UnitTag, float> chanceMinDonations = new()
        {
            { UnitTag.Scout, 0.1f },
            { UnitTag.Sleeper, 0.1f },
            { UnitTag.Mauler, 0.1f },
            { UnitTag.Lord, 0.1f },
            { UnitTag.Hive, 0.1f },
        };

        public static Dictionary<UnitTag, float> chanceDonations = new()
        {
            { UnitTag.Scout, 0.1f },
            { UnitTag.Sleeper, 0.1f },
            { UnitTag.Mauler, 0.5f },
            { UnitTag.Lord, 0.5f },
            { UnitTag.Hive, 0.5f },
        };

        public static Dictionary<UnitTag, float> chanceStack2Donations = new()
        {
            { UnitTag.Scout, 0.2f },
            { UnitTag.Sleeper, 0.2f },
            { UnitTag.Mauler, 1f },
            { UnitTag.Lord, 1f },
            { UnitTag.Hive, 1f },
        };

        public static int MaxFanCountForDonations = 30000;

        public StreamDonations()
        {
            stackable = true;
            maxStacks = 2;
            company = Company.Rockstar;
            name = L("rockstar.stream_donations.name");
            description = L("rockstar.stream_donations.description");
        }


        public override string GetDescriptionForStacks(int stacks)
        {
            if (stacks == 1)
            {
                return L("rockstar.stream_donations.description_stack1");
            }
            else
            {
                return L("rockstar.stream_donations.description_stack2");
            }
        }

        private static StreamDonations _instance;
        public static StreamDonations Instance => _instance ??= new();

        internal static void OnEnemyDeath(BattleUnit enemy)
        {
            if (Instance.currentStacks == 0) return;

            // Get the chance based on enemy type and stack level
            if (!chanceMinDonations.TryGetValue(enemy.UnitTag, out float minChance))
            {
                MelonLogger.Msg($"StreamDonations: Unknown enemy type {enemy.UnitTag}");
                return;
            }

            var maxChanceDict = Instance.currentStacks >= 2 ? chanceStack2Donations : chanceDonations;
            if (!maxChanceDict.TryGetValue(enemy.UnitTag, out float maxChance))
            {
                MelonLogger.Msg($"StreamDonations: Unknown enemy type {enemy.UnitTag} in max chance dict");
                return;
            }

            // Calculate chance based on fan count
            // Interpolate between minChance and maxChance based on fan count
            // Assuming fan count ranges from 0 to some reasonable max (e.g., 10000)
            float fanCount = RockstarAffinityHelpers.fanCount;
            float normalizedFanCount = UnityEngine.Mathf.Clamp01(fanCount / MaxFanCountForDonations);
            float chance = UnityEngine.Mathf.Lerp(minChance, maxChance, normalizedFanCount);

            // Apply In the Spotlight boost if active and enemy is near Top Star
            if (InTheSpotlight.Instance.IsActive && InTheSpotlight.IsTopStarAttacking)
            {
                chance = UnityEngine.Mathf.Min(1.0f, chance * InTheSpotlight.TopStarDonationMultiplier);
                MelonLogger.Msg($"StreamDonations: In the Spotlight boost applied! New chance: {chance:P}");
            }

            MelonLogger.Msg($"StreamDonations: Enemy {enemy.UnitName} ({enemy.UnitTag}) died. Fan count: {fanCount}, Chance: {chance:P}");

            // Roll for donation
            if (UnityEngine.Random.value < chance)
            {
                MelonLogger.Msg($"StreamDonations: Receiving donation!");
                ReceiveDonation();
            }
        }

        public static void ReceiveDonation()
        {
            var gainedActionCards = InBattleActionCardsManager.Instance.InBattleActionCards;
            MelonLogger.Msg($"Gained Action Cards: {gainedActionCards.Count}");
            foreach (var card in gainedActionCards)
            {
                MelonLogger.Msg($"- {card.Info.CardName} (Uses: {card.Info.Uses})");
            }
            string startingSquadId = Singleton<Player>.Instance.PlayerData.Squad.StartingSquadId;
            CardsButtons_BattleManagementDirectory cardsUI = CardsButtons_BattleManagementDirectory_Instance.Instance;
            var availableCardsToAdd = Singleton<AssetsDatabase>.Instance.ActionCards.Where(card =>
                card.Info.Uses > 0 &&
                !card.AvailableOnDeploymentPhase &&
                !card.Info.SquadIdsThatCannotUseCommand.Contains(startingSquadId) &&
                (Singleton<Player>.Instance.PlayerData.AccessPointsSystemEnabled || card.Info.Group != ActionCardInfo.ActionCardGroup.accessPoints) &&
                (Singleton<Player>.Instance.PlayerData.BioweaveSystemEnabled || card.Info.Group != ActionCardInfo.ActionCardGroup.bioweavePoints)
            ).ToList();
            MelonLogger.Msg($"Available Action Cards in Database: {availableCardsToAdd.Count}");
            var chosenCard = availableCardsToAdd[UnityEngine.Random.Range(0, availableCardsToAdd.Count)];
            MelonLogger.Msg($"Adding Action Card: {chosenCard.Info.CardName}");
            // Add the chosen card to the in-battle action cards
            var existingCard = gainedActionCards.FirstOrDefault(c => c.Info.Id == chosenCard.Info.Id);
            if (existingCard != null)
            {
                // If it already exists, increase its uses
                // existingCard._usesLeft += 1;
                AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(existingCard, existingCard.UsesLeft + 1);
                MelonLogger.Msg($"Increased uses of existing card to {existingCard.Info.Uses}");
            }
            else
            {
                // Otherwise, add it as a new card
                var card = chosenCard.CreateInstance();
                // card.UsesLeft = 1;
                AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(card, 1);

                gainedActionCards.Add(card);
                MelonLogger.Msg($"Added new card {chosenCard.Info.CardName} with 1 use");
            }

            if (cardsUI != null && cardsUI.gameObject.activeInHierarchy)
            {
                var flowController = AccessTools.Field(typeof(CardsButtons_BattleManagementDirectory), "_directoriesFlowController")
.GetValue(cardsUI) as DirectoriesFlowController;

                if (flowController != null)
                {
                    flowController.ShowDirectory();  // This will call Initialize() and fire OnDirectoryUpdate -> CreateMenu()
                }
                var _directoryId = AccessTools.Field(typeof(CardsButtons_BattleManagementDirectory), "_directoryId").GetValue(cardsUI);
                var directoryData = cardsUI.Initialize();
                // directoryData.DirectoryId = _directoryId ?? Guid.NewGuid();
                if (_directoryId != null)
                {
                    AccessTools.Field(typeof(DirectoryData), "_directoryId").SetValue(directoryData, _directoryId);
                }
                cardsUI.UpdateDirectory(directoryData);
                //BattleManagementWindowController.DirectoryUpdated(directoryData);
                AccessTools.Method(typeof(BattleManagementWindowController), "DirectoryUpdated").Invoke(BattleManagementWindowController_Instance.Instance, [directoryData]);
                MelonLogger.Msg("Updated Cards UI");
            }
        }
    }


    [HarmonyPatch(typeof(BattleManagementWindowController), "InitializeBattleManagementController")]
    public class BattleManagementWindowController_Instance
    {
        public static BattleManagementWindowController Instance;
        public static void Prefix(BattleManagementWindowController __instance)
        {
            Instance = __instance;
        }
    }

    [HarmonyPatch(typeof(CardsButtons_BattleManagementDirectory), "Initialize")]
    public class CardsButtons_BattleManagementDirectory_Instance
    {
        public static CardsButtons_BattleManagementDirectory Instance;
        public static void Prefix(CardsButtons_BattleManagementDirectory __instance)
        {
            Instance = __instance;
        }
    }

    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor, [typeof(UnitData), typeof(Team), typeof(GridManager)])]
    public class BattleUnit_Constructor_Patch
    {
        public static void Postfix(BattleUnit __instance, Team team)
        {
            if (!StreamDonations.Instance.IsActive)
            {
                return;
            }
            if (team == Team.EnemyAI)
            {
                void action()
                {
                    StreamDonations.OnEnemyDeath(__instance);
                    __instance.OnDeath -= action;
                }

                __instance.OnDeath += action;
            }
        }
    }
}
