using SpaceCommander;
using SpaceCommander.Database;
using UnityEngine;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.WarriorReinforcements
{
    // 近战大师：获得一个动能战锤（可叠加3：每次选择都可以获得一个动能战锤）
    // Melee Master: Gain a Kinetic Warhammer (stackable up to 3, each stack gives another Kinetic Warhammer)
    public class MeleeMaster : Reinforcement
    {
        internal const string KINETIC_WARHAMMER_ID = WeaponCategories.KINETIC_WARHAMMER;

        public MeleeMaster()
        {
            company = Company.Warrior;
            rarity = Rarity.Elite;
            stackable = true;
            maxStacks = 3;
            name = L("warrior.melee_master.name");
            flavourText = L("warrior.melee_master.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("warrior.melee_master.description");
        }

        public override void OnActivate()
        {
            EquipKineticWarhammer();
        }

        /// <summary>
        /// Equip a Kinetic Warhammer to the most suitable squad member
        /// Priority: Find unit with the highest Power stat
        /// </summary>
        private void EquipKineticWarhammer()
        {
            var playerData = Singleton<Player>.Instance.PlayerData;
            var squadUnits = playerData.Squad.SquadUnits;

            // Get the Kinetic Warhammer weapon data
            var warhammer = Singleton<AssetsDatabase>.Instance.GetWeaponDataSO(
                KINETIC_WARHAMMER_ID,
                Enumerations.EquipmentType.Melee
            );

            if (warhammer == null)
            {
                Debug.LogError($"{GetType().Name}: Could not find Kinetic Warhammer with ID {KINETIC_WARHAMMER_ID}");
                return;
            }

            // Find the unit with the highest Power stat
            UpgradableUnit bestUnit = null;
            float highestPower = float.MinValue;

            foreach (var unit in squadUnits)
            {
                var unitData = unit.GetCopyOfUnitData();
                float power = unitData.GetStatValueWithEquipmentEffects(Enumerations.UnitStats.Power);

                if (power > highestPower)
                {
                    highestPower = power;
                    bestUnit = unit;
                }
            }

            // If found suitable unit, equip the warhammer
            if (bestUnit != null)
            {
                bestUnit.ReplaceEquipment(warhammer);
                Debug.Log($"{GetType().Name}: Equipped Kinetic Warhammer to {bestUnit.UnitName}");
            }
            else
            {
                // Give player coins equal to weapon's buying price
                int buyingPrice = warhammer.BuyingPrice;
                playerData.PlayerWallet.ChangeCoinsByValue(buyingPrice);
                Debug.Log($"{GetType().Name}: No suitable unit found for Kinetic Warhammer. Added {buyingPrice} coins to wallet.");
            }
        }

        protected static MeleeMaster _instance;
        public static MeleeMaster Instance => _instance ??= new();
    }
}
