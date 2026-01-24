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
        public static WhalePatron Instance =>  _instance ??= new();

        internal static void GiveEquipmentReward()
        {
            var playerData = Singleton<Player>.Instance.PlayerData;
            var squadUnits = playerData.Squad.SquadUnits.ToList();

            // Get all purchasable equipment (not enemy-only equipment)
            var upgradeDataSO = Singleton<AssetsDatabase>.Instance.UpgradeDataSO;
            var allPurchasableEquipment = new List<EquipmentDataSO>();
            allPurchasableEquipment.AddRange(upgradeDataSO.UpgradeRangedWeaponsList);
            allPurchasableEquipment.AddRange(upgradeDataSO.UpgradeMeleeWeaponsList);
            allPurchasableEquipment.AddRange(upgradeDataSO.UpgradeGearsList);

            // Get initial/default equipment IDs that should be considered as "not owned"
            var initialEquipmentIds = CelebrityAuction.GetInitialEquipmentIds();

            // Get all equipment currently owned by squad members (excluding default equipment)
            var ownedEquipmentIds = new HashSet<string>();
            foreach (var unit in squadUnits)
            {
                var ranged = unit.GetCurrentEquipmentOfUnit(EquipmentType.Ranged);
                var melee = unit.GetCurrentEquipmentOfUnit(EquipmentType.Melee);
                var gear = unit.GetCurrentEquipmentOfUnit(EquipmentType.Gear);

                // Only count as owned if it's not default equipment
                if (ranged != null && !initialEquipmentIds.Contains(ranged.Id))
                    ownedEquipmentIds.Add(ranged.Id);
                if (melee != null && !initialEquipmentIds.Contains(melee.Id))
                    ownedEquipmentIds.Add(melee.Id);
                if (gear != null && !initialEquipmentIds.Contains(gear.Id))
                    ownedEquipmentIds.Add(gear.Id);
            }

            // Find equipment we don't have (excluding default equipment)
            var unownedEquipment = allPurchasableEquipment.Where(e => !ownedEquipmentIds.Contains(e.Id)).ToList();

            if (unownedEquipment.Count > 0)
            {
                // Give a random piece of equipment we don't have
                var newEquipment = unownedEquipment[UnityEngine.Random.Range(0, unownedEquipment.Count)];
                TryEquipToSuitableUnit(newEquipment, squadUnits, playerData);
                Debug.Log($"WhalePatron: Gave new equipment {newEquipment.EquipmentName}");
            }
            else
            {
                // Everyone has all equipment types, so upgrade by selling cheaper and buying more expensive
                UpgradeEquipment(squadUnits, allPurchasableEquipment, initialEquipmentIds, playerData);
            }
        }

        private static void TryEquipToSuitableUnit(EquipmentDataSO equipment, List<UpgradableUnit> squadUnits, PlayerData playerData)
        {
            // Try to find a unit that doesn't have this equipment type, or has cheaper equipment of this type
            UpgradableUnit bestUnit = null;
            int lowestPrice = int.MaxValue;

            foreach (var unit in squadUnits)
            {
                var currentEquipment = unit.GetCurrentEquipmentOfUnit(equipment.EquipmentType);

                // If unit doesn't have this equipment type, prioritize them
                if (currentEquipment == null)
                {
                    bestUnit = unit;
                    break;
                }

                // Otherwise, find the unit with the cheapest equipment of this type
                if (currentEquipment.BuyingPrice < lowestPrice)
                {
                    lowestPrice = currentEquipment.BuyingPrice;
                    bestUnit = unit;
                }
            }

            if (bestUnit != null)
            {
                // Equip the new equipment
                bestUnit.ReplaceEquipment(equipment);
            }
            else
            {
                // Fallback: give coins equal to equipment value (should never happen)
                playerData.PlayerWallet.ChangeCoinsByValue(equipment.BuyingPrice);
                Debug.Log($"WhalePatron: Could not equip {equipment.EquipmentName}, gave {equipment.BuyingPrice} coins instead");
            }
        }

        private static void UpgradeEquipment(List<UpgradableUnit> squadUnits, List<EquipmentDataSO> allPurchasableEquipment, HashSet<string> initialEquipmentIds, PlayerData playerData)
        {
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

            // Get the equipment type and find more expensive alternatives
            var equipmentType = itemToSell.Item2.EquipmentType;
            var currentPrice = itemToSell.Item2.BuyingPrice;

            // Account for CelebrityAuction reinforcement when selling
            int sellPrice = itemToSell.Item2.BuyingPrice;
            if (CelebrityAuction.Instance.IsActive)
            {
                sellPrice = CelebrityAuction.GetSellPrice(itemToSell.Item1, itemToSell.Item2);
            }

            // Find all equipment of the same type that's more expensive
            var moreExpensiveOptions = allPurchasableEquipment
                .Where(e => e.EquipmentType == equipmentType && e.BuyingPrice > currentPrice)
                .ToList();

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
                // No more expensive options available, just give coins
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
            if (!WhalePatron.Instance.IsActive)
            {
                return;
            }

            // Give equipment reward after each battle
            WhalePatron.GiveEquipmentReward();
        }
    }
}
