using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Database;
using SpaceCommander.Weapons;
using UnityEngine;
using static SpaceCommander.Enumerations;

namespace XenopurgeRougeLike.RockstarReinforcements
{
    // 榜一大哥
    // Whale Patron
    // After battle, gain a piece of equipment you don't have. If everyone already has a melee weapon, ranged weapon, and equipment, randomly sell a lower base-price item and gain one with a higher price.
    public class WhalePatron : Reinforcement
    {
        public WhalePatron()
        {
            company = Company.Rockstar;
            rarity = Rarity.Expert;
            name = "Whale Patron";
            description = "After battle, gain a piece of equipment you don't have. If everyone already has a melee weapon, ranged weapon, and equipment, randomly sell a lower base-price item and gain one with a higher price.";
        }

        protected static WhalePatron _instance;
        public static WhalePatron Instance => _instance ??= new();

        internal static void GiveEquipmentReward()
        {
            Debug.Log("WhalePatron: GiveEquipmentReward called");

            var playerData = Singleton<Player>.Instance.PlayerData;
            var squadUnits = playerData.Squad.SquadUnits.ToList();

            Debug.Log($"WhalePatron: Found {squadUnits.Count} squad units");

            // Get all purchasable equipment (not enemy-only equipment)
            var upgradeDataSO = Singleton<AssetsDatabase>.Instance.UpgradeDataSO;
            var allPurchasableEquipment = new List<EquipmentDataSO>();
            allPurchasableEquipment.AddRange(upgradeDataSO.UpgradeRangedWeaponsList);
            allPurchasableEquipment.AddRange(upgradeDataSO.UpgradeMeleeWeaponsList);
            allPurchasableEquipment.AddRange(upgradeDataSO.UpgradeGearsList);

            Debug.Log($"WhalePatron: Found {allPurchasableEquipment.Count} total purchasable equipment pieces");

            // Get initial/default equipment IDs that should be considered as "not owned"
            var initialEquipmentIds = CelebrityAuction.GetInitialEquipmentIds();
            Debug.Log($"WhalePatron: Initial equipment IDs count: {initialEquipmentIds.Count}");

            // Get all equipment currently owned by squad members (excluding default equipment)
            List<EquipmentType> missingEquipmentSlots = [];

            foreach (var unit in squadUnits)
            {
                var ranged = unit.GetCurrentEquipmentOfUnit(EquipmentType.Ranged);
                var melee = unit.GetCurrentEquipmentOfUnit(EquipmentType.Melee);
                var gear = unit.GetCurrentEquipmentOfUnit(EquipmentType.Gear);

                Debug.Log($"WhalePatron: Unit has - Ranged: {ranged?.EquipmentName ?? "null"}, Melee: {melee?.EquipmentName ?? "null"}, Gear: {gear?.EquipmentName ?? "null"}");

                // Only count as owned if it's not default equipment
                if (ranged == null || initialEquipmentIds.Contains(ranged.Id))
                {
                    missingEquipmentSlots.Add(EquipmentType.Ranged);
                    Debug.Log($"WhalePatron: Unit missing ranged weapon (or has default)");
                }
                if (melee == null || initialEquipmentIds.Contains(melee.Id))
                {
                    missingEquipmentSlots.Add(EquipmentType.Melee);
                    Debug.Log($"WhalePatron: Unit missing melee weapon (or has default)");
                }
                if (gear == null || initialEquipmentIds.Contains(gear.Id))
                {
                    missingEquipmentSlots.Add(EquipmentType.Gear);
                    Debug.Log($"WhalePatron: Unit missing gear (or has default)");
                }
            }

            Debug.Log($"WhalePatron: Total missing equipment slots: {missingEquipmentSlots.Count}");

            if (missingEquipmentSlots.Count == 0)
            {
                Debug.Log("WhalePatron: Everyone has all equipment types, upgrading equipment");
                // Everyone has all equipment types, so upgrade by selling cheaper and buying more expensive
                UpgradeEquipment(squadUnits, allPurchasableEquipment, initialEquipmentIds, playerData);
            }
            else
            {
                EquipmentType chosenType = missingEquipmentSlots[UnityEngine.Random.Range(0, missingEquipmentSlots.Count)];
                Debug.Log($"WhalePatron: Chosen equipment type to give: {chosenType}");

                // Find equipment we don't have (excluding default equipment)
                // FIXED: Get all equipment of this type, not just unowned ones
                var availableEquipment = allPurchasableEquipment.Where(e => e.EquipmentType == chosenType).ToList();

                Debug.Log($"WhalePatron: Found {availableEquipment.Count} available equipment of type {chosenType}");

                if (availableEquipment.Count > 0)
                {
                    // Give a random piece of equipment
                    var newEquipment = availableEquipment[UnityEngine.Random.Range(0, availableEquipment.Count)];
                    Debug.Log($"WhalePatron: Selected equipment: {newEquipment.EquipmentName}");
                    TryEquipToSuitableUnit(newEquipment, squadUnits, playerData, initialEquipmentIds);
                    Debug.Log($"WhalePatron: Gave new equipment {newEquipment.EquipmentName}");
                }
                else
                {
                    Debug.LogWarning($"WhalePatron: No available equipment of type {chosenType} found!");
                }
            }
        }

        private static void TryEquipToSuitableUnit(EquipmentDataSO equipment, List<UpgradableUnit> squadUnits, PlayerData playerData, HashSet<string> initialEquipmentIds)
        {
            Debug.Log($"WhalePatron: TryEquipToSuitableUnit called for {equipment.EquipmentName}");

            // Try to find a unit that doesn't have this equipment type, or has cheaper equipment of this type
            UpgradableUnit bestUnit = null;

            foreach (var unit in squadUnits)
            {
                var currentEquipment = unit.GetCurrentEquipmentOfUnit(equipment.EquipmentType);

                // If unit doesn't have this equipment type, prioritize them
                if (currentEquipment == null || initialEquipmentIds.Contains(currentEquipment.Id))
                {
                    bestUnit = unit;
                    Debug.Log($"WhalePatron: Found suitable unit (missing equipment or has default)");
                    break;
                }
            }

            if (bestUnit == null)
            {
                Debug.LogWarning("WhalePatron: No suitable unit found!");
                return;
            }

            // Equip the new equipment
            bestUnit.ReplaceEquipment(equipment);
            Debug.Log($"WhalePatron: Successfully equipped {equipment.EquipmentName} to unit");
        }

        private static void UpgradeEquipment(List<UpgradableUnit> squadUnits, List<EquipmentDataSO> allPurchasableEquipment, HashSet<string> initialEquipmentIds, PlayerData playerData)
        {
            Debug.Log("WhalePatron: UpgradeEquipment called");

            // Collect all currently equipped items with their units (excluding default equipment)
            var equippedItems = new List<Tuple<UpgradableUnit, EquipmentDataSO>>();

            foreach (var unit in squadUnits)
            {
                var ranged = unit.GetCurrentEquipmentOfUnit(EquipmentType.Ranged);
                var melee = unit.GetCurrentEquipmentOfUnit(EquipmentType.Melee);
                var gear = unit.GetCurrentEquipmentOfUnit(EquipmentType.Gear);

                // Only add non-default equipment to the list of items we can sell
                if (ranged != null && !initialEquipmentIds.Contains(ranged.Id))
                    equippedItems.Add(new Tuple<UpgradableUnit, EquipmentDataSO>(unit, ranged));
                if (melee != null && !initialEquipmentIds.Contains(melee.Id))
                    equippedItems.Add(new Tuple<UpgradableUnit, EquipmentDataSO>(unit, melee));
                if (gear != null && !initialEquipmentIds.Contains(gear.Id))
                    equippedItems.Add(new Tuple<UpgradableUnit, EquipmentDataSO>(unit, gear));
            }

            Debug.Log($"WhalePatron: Found {equippedItems.Count} equipped non-default items");

            if (equippedItems.Count == 0)
            {
                Debug.LogWarning("WhalePatron: No equipped items found, cannot upgrade");
                return;
            }

            // Sort by buying price (ascending)
            equippedItems.Sort((a, b) => a.Item2.BuyingPrice.CompareTo(b.Item2.BuyingPrice));

            // Select a random item from the cheaper half
            int cheaperHalfCount = Mathf.Max(1, equippedItems.Count / 2);
            var itemToSell = equippedItems[UnityEngine.Random.Range(0, cheaperHalfCount)];

            Debug.Log($"WhalePatron: Selected item to sell: {itemToSell.Item2.EquipmentName} (price: {itemToSell.Item2.BuyingPrice})");

            // Get the equipment type and find more expensive alternatives
            var equipmentType = itemToSell.Item2.EquipmentType;
            var currentPrice = itemToSell.Item2.BuyingPrice;

            // Account for CelebrityAuction reinforcement when selling
            int sellPrice = itemToSell.Item2.BuyingPrice;
            if (CelebrityAuction.Instance.IsActive)
            {
                sellPrice = CelebrityAuction.GetSellPrice(itemToSell.Item1, itemToSell.Item2);
                Debug.Log($"WhalePatron: CelebrityAuction is active, adjusted sell price: {sellPrice}");
            }

            // Find all equipment of the same type that's more expensive
            var moreExpensiveOptions = allPurchasableEquipment
                .Where(e => e.EquipmentType == equipmentType && e.BuyingPrice > currentPrice)
                .ToList();

            Debug.Log($"WhalePatron: Found {moreExpensiveOptions.Count} more expensive options");

            if (moreExpensiveOptions.Count > 0)
            {
                // Sell the current item
                playerData.PlayerWallet.ChangeCoinsByValue(sellPrice);

                // Buy a random more expensive item
                var newEquipment = moreExpensiveOptions[UnityEngine.Random.Range(0, moreExpensiveOptions.Count)];
                itemToSell.Item1.ReplaceEquipment(newEquipment);

                Debug.Log($"WhalePatron: Sold {itemToSell.Item2.EquipmentName} for {sellPrice} coins and equipped {newEquipment.EquipmentName}");
            }
            else
            {
                // No more expensive options available, give coins instead
                playerData.PlayerWallet.ChangeCoinsByValue(sellPrice);
                Debug.Log($"WhalePatron: No upgrade available for {itemToSell.Item2.EquipmentName}, gave {sellPrice} coins instead");
            }
        }
    }

    [HarmonyPatch(typeof(GameManager))]
    public class WhalePatron_GameManager_Patch
    {
        [HarmonyPatch("GiveEndGameRewards")]
        [HarmonyPostfix]
        public static void GiveEndGameRewards(GameManager __instance, bool victory)
        {
            Debug.Log($"WhalePatron: GiveEndGameRewards patch called, victory={victory}");

            if (!WhalePatron.Instance.IsActive)
            {
                Debug.Log("WhalePatron: Reinforcement is not active, skipping");
                return;
            }

            Debug.Log("WhalePatron: Reinforcement is active, giving equipment reward");
            // Give equipment reward after each battle
            WhalePatron.GiveEquipmentReward();
        }
    }
}