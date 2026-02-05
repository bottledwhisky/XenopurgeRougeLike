using MelonLoader;
using SpaceCommander.ActionCards;
using SpaceCommander.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XenopurgeRougeLike
{
    public static class LoggingUtils
    {
        private const string LogFilePath = @"D:\projects\xenopurge\ActionCardSO.txt";

        /// <summary>
        /// Logs all action cards available in the game database
        /// </summary>
        public static void LogAllDatabaseActionCards()
        {
            var database = Singleton<AssetsDatabase>.Instance;
            if (database == null)
            {
                MelonLogger.Warning("LogAllDatabaseActionCards: AssetsDatabase instance is null");
                return;
            }

            var actionCards = database.ActionCards;
            if (actionCards == null)
            {
                MelonLogger.Warning("LogAllDatabaseActionCards: Action cards collection is null");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"========================================");
            sb.AppendLine($"DATABASE ACTION CARDS ({actionCards.Count()} total)");
            sb.AppendLine($"========================================");

            foreach (var cardSO in actionCards)
            {
                LogActionCardSODetails(cardSO, sb);
            }

            sb.AppendLine($"========================================");
            sb.AppendLine($"END OF DATABASE ACTION CARDS");
            sb.AppendLine($"========================================");

            File.WriteAllText(LogFilePath, sb.ToString());
            MelonLogger.Msg($"Action cards logged to {LogFilePath}");
        }

        /// <summary>
        /// Logs detailed information about an ActionCardSO from the database
        /// </summary>
        public static void LogActionCardSODetails(ActionCardSO cardSO, StringBuilder sb)
        {
            if (cardSO == null)
            {
                sb.AppendLine("LogActionCardSODetails: CardSO is null");
                return;
            }

            sb.AppendLine();
            sb.AppendLine($"Asset Name: {cardSO.name}");
            sb.AppendLine($"Type: {cardSO.GetType().Name}");

            if (cardSO.Info != null)
            {
                sb.AppendLine($"Id: {cardSO.Info.Id}");
                sb.AppendLine($"Card Name: {cardSO.Info.CardName}");
                sb.AppendLine($"Description: {cardSO.Info.CardDescription}");
                sb.AppendLine($"Group: {cardSO.Info.Group}");
                sb.AppendLine($"Sorting Order: {cardSO.Info.SortingOrder}");
                sb.AppendLine($"Buying Cost: {cardSO.Info.BuyingCost}");
                sb.AppendLine($"Access Points Cost: {cardSO.Info.AccessPointsCost}");
                sb.AppendLine($"Bioweave Points Cost: {cardSO.Info.BioweavePointsCost}");
                sb.AppendLine($"Uses: {cardSO.Info.Uses}");
                sb.AppendLine($"Available On Deployment Phase: {cardSO.Info.AvailableOnDeploymentPhase}");
                sb.AppendLine($"Can Not Be Replenished: {cardSO.Info.CanNotBeReplenished}");

                if (cardSO.Info.SquadIdsThatCannotUseCommand != null && cardSO.Info.SquadIdsThatCannotUseCommand.Count > 0)
                {
                    sb.AppendLine($"Squad IDs That Cannot Use: {string.Join(", ", cardSO.Info.SquadIdsThatCannotUseCommand)}");
                }
            }
            else
            {
                sb.AppendLine("Info: null");
            }

            sb.AppendLine("----------------------------------------");
        }


        public static void DebugPrintAllReinforcementToString()
        {
            MelonLogger.Msg("========== DEBUG: All Reinforcement ToString() ==========");

            foreach (var companyEntry in Company.Companies)
            {
                var companyType = companyEntry.Key;
                var company = companyEntry.Value;

                MelonLogger.Msg($"\n--- Company: {companyType} ---");

                try
                {
                    // Get the Reinforcements property using reflection
                    var reinforcementsProperty = company.ClassType.GetProperty("Reinforcements",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                    if (reinforcementsProperty != null)
                    {
                        var reinforcements = reinforcementsProperty.GetValue(null) as Dictionary<Type, Reinforcement>;

                        if (reinforcements != null)
                        {
                            foreach (var reinforcementEntry in reinforcements)
                            {
                                var reinforcementType = reinforcementEntry.Key;
                                var reinforcement = reinforcementEntry.Value;

                                try
                                {
                                    // For stackable reinforcements with currentStacks = 0, iterate through all possible stacks
                                    if (reinforcement.stackable && reinforcement.currentStacks == 0)
                                    {
                                        for (int stack = 1; stack <= reinforcement.maxStacks; stack++)
                                        {
                                            string description = reinforcement.GetDescriptionForStacks(stack);
                                        }
                                    }
                                    else
                                    {
                                        string toStringResult = reinforcement.ToString();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MelonLogger.Error($"  ✗ {reinforcementType.Name}: ERROR - {ex.Message}");
                                    MelonLogger.Error($"    Stack trace: {ex.StackTrace}");
                                }
                            }
                        }
                        else
                        {
                            MelonLogger.Msg($"  (No reinforcements dictionary found)");
                        }
                    }
                    else
                    {
                        MelonLogger.Msg($"  (No Reinforcements property found)");
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"  Error accessing company {companyType}: {ex.Message}");
                }
            }

            MelonLogger.Msg("\n========== END DEBUG ==========");
        }
    }
}
