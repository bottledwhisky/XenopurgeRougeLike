using System.Collections.Generic;
using SpaceCommander;
using SpaceCommander.Database;
using SpaceCommander.Weapons;
using UnityEngine;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    /// <summary>
    /// Base class for smart weapon reinforcements (Module and Bus)
    /// Provides common functionality for weapon selection and equipment
    /// </summary>
    public abstract class SmartWeaponReinforcementBase : Reinforcement
    {
        // Weapon IDs
        internal const string HEMOGRIP_ID = WeaponCategories.HEMOGRIP;
        internal const string BARK_ID = WeaponCategories.BARK_SYSTEM;
        internal const string TRAC_ID = WeaponCategories.TRAC_CARBINE;
        internal const string COMBAT_KNIFE_ID = WeaponCategories.COMBAT_KNIFE;
        internal const string MOP_PISTOL_ID1 = WeaponCategories.MOP_PISTOL;
        internal const string MOP_PISTOL_ID2 = WeaponCategories.MOP_PISTOL_VARIANT;

        protected SmartWeaponReinforcementBase()
        {
            company = Company.Synthetics;
            stackable = false;
            maxStacks = 1;
        }

        /// <summary>
        /// Randomly select one of the three smart weapons
        /// </summary>
        public string SelectRandomWeapon()
        {
            var weapons = new[] { HEMOGRIP_ID, BARK_ID, TRAC_ID };
            return weapons[Random.Range(0, weapons.Length)];
        }

        /// <summary>
        /// Try to equip the selected weapon to the most suitable squad member
        /// </summary>
        public void EquipWeapon()
        {
            var SelectedWeaponId = SelectRandomWeapon();
            EquipWeaponById(SelectedWeaponId);
        }

        /// <summary>
        /// Equip a specific weapon by ID to the most suitable squad member
        /// </summary>
        protected void EquipWeaponById(string weaponId)
        {
            var playerData = Singleton<Player>.Instance.PlayerData;
            var squadUnits = playerData.Squad.SquadUnits;

            UpgradableUnit bestUnit;
            EquipmentDataSO weaponToEquip;

            // Determine which equipment type we're replacing based on selected weapon
            Enumerations.EquipmentType equipmentType;
            if (weaponId == HEMOGRIP_ID)
            {
                equipmentType = Enumerations.EquipmentType.Melee;
                // Find unit with Combat Knife and highest Power
                bestUnit = FindBestUnitWithEquipment([.. squadUnits], COMBAT_KNIFE_ID, Enumerations.UnitStats.Power, Enumerations.EquipmentType.Melee);
            }
            else
            {
                equipmentType = Enumerations.EquipmentType.Ranged;
                // Find unit with MOP pistol and highest Accuracy
                bestUnit = FindBestUnitWithPistol([.. squadUnits]);
            }

            // Get the weapon data
            weaponToEquip = Singleton<AssetsDatabase>.Instance.GetWeaponDataSO(weaponId, equipmentType);
            if (weaponToEquip == null)
            {
                Debug.LogError($"{GetType().Name}: Could not find weapon with ID {weaponId}");
                return;
            }

            // If found suitable unit, equip the weapon
            if (bestUnit != null)
            {
                bestUnit.ReplaceEquipment(weaponToEquip);
                Debug.Log($"{GetType().Name}: Equipped {weaponToEquip.name} to {bestUnit.UnitName}");
            }
            else
            {
                // Give player coins equal to weapon's buying price
                int buyingPrice = weaponToEquip.BuyingPrice;
                playerData.PlayerWallet.ChangeCoinsByValue(buyingPrice);
                Debug.Log($"{GetType().Name}: No suitable unit found for {weaponToEquip.name}. Added {buyingPrice} coins to wallet.");
            }
        }

        /// <summary>
        /// Find the best unit with specific equipment and highest stat
        /// </summary>
        protected UpgradableUnit FindBestUnitWithEquipment(List<UpgradableUnit> units, string equipmentId, Enumerations.UnitStats statToCheck, Enumerations.EquipmentType equipmentType)
        {
            UpgradableUnit bestUnit = null;
            float highestStat = float.MinValue;

            foreach (var unit in units)
            {
                var currentEquipment = unit.GetCurrentEquipmentOfUnit(equipmentType);
                if (currentEquipment != null && currentEquipment.Id == equipmentId)
                {
                    var unitData = unit.GetCopyOfUnitData();
                    float statValue = unitData.GetStatValueWithEquipmentEffects(statToCheck);

                    if (statValue > highestStat)
                    {
                        highestStat = statValue;
                        bestUnit = unit;
                    }
                }
            }

            return bestUnit;
        }

        /// <summary>
        /// Find the best unit with MOP pistol (either variant) and highest Accuracy
        /// </summary>
        protected UpgradableUnit FindBestUnitWithPistol(List<UpgradableUnit> units)
        {
            UpgradableUnit bestUnit = null;
            float highestAccuracy = float.MinValue;

            foreach (var unit in units)
            {
                var currentEquipment = unit.GetCurrentEquipmentOfUnit(Enumerations.EquipmentType.Ranged);
                if (currentEquipment != null &&
                    (currentEquipment.Id == MOP_PISTOL_ID1 || currentEquipment.Id == MOP_PISTOL_ID2))
                {
                    var unitData = unit.GetCopyOfUnitData();
                    float accuracy = unitData.GetStatValueWithEquipmentEffects(Enumerations.UnitStats.Accuracy);

                    if (accuracy > highestAccuracy)
                    {
                        highestAccuracy = accuracy;
                        bestUnit = unit;
                    }
                }
            }

            return bestUnit;
        }
    }
}
