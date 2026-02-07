using System;
using System.Collections.Generic;
using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Database;
using SpaceCommander.PartyCustomization;
using SpaceCommander.UI;
using SpaceCommander.Weapons;
using Traptics.EventsSystem;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.RockstarReinforcements
{
    // 名人拍卖
    // Celebrity Auction
    // You can sell equipment. Sale price increases by 1 for each battle the equipment has been through.
    public class CelebrityAuction : Reinforcement
    {
        // Track if we're in sell mode
        public static bool IsSellMode { get; set; } = false;

        // Track battle counts for equipment (equipment ID -> battle count)
        public static Dictionary<string, int> EquipmentBattleCounts { get; set; } = [];

        public CelebrityAuction()
        {
            company = Company.Rockstar;
            name = L("rockstar.celebrity_auction.name");
            description = L("rockstar.celebrity_auction.description");
        }

        protected static CelebrityAuction _instance;
        public static CelebrityAuction Instance => _instance ??= new();

        public override void OnActivate()
        {
            EventBus.Subscribe<GameExitEvent>(new Action<GameExitEvent>(this.OnGameExit));
        }

        public override void OnDeactivate()
        {
            EventBus.Unsubscribe<GameExitEvent>(new Action<GameExitEvent>(this.OnGameExit));
            ResetStates();
        }

        private void OnGameExit(GameExitEvent evt)
        {
            // Clear equipment battle counts on game exit
            EquipmentBattleCounts.Clear();
        }

        public static void ResetStates()
        {
            IsSellMode = false;
            EquipmentBattleCounts.Clear();
        }

        public override Dictionary<string, object> SaveState()
        {
            if (EquipmentBattleCounts.Count == 0)
            {
                return null;
            }

            // Convert Dictionary<string, int> to Dictionary<string, object> for serialization
            var state = new Dictionary<string, object>();
            foreach (var kvp in EquipmentBattleCounts)
            {
                state[kvp.Key] = kvp.Value;
            }
            return state;
        }

        public override void LoadState(Dictionary<string, object> state)
        {
            EquipmentBattleCounts.Clear();
            foreach (var kvp in state)
            {
                EquipmentBattleCounts[kvp.Key] = Convert.ToInt32(kvp.Value);
            }
            MelonLoader.MelonLogger.Msg($"[CelebrityAuction] Loaded {EquipmentBattleCounts.Count} equipment battle counts");
        }

        // Get the list of initial equipment that cannot be sold
        public static HashSet<string> GetInitialEquipmentIds()
        {
            var initialEquipment = new HashSet<string>();
            var hireUnitGeneratorSettingsSO = Singleton<AssetsDatabase>.Instance.HireUnitGeneratorSettingsSO;

            var rangedWeapons = AccessTools.Field(typeof(HireUnitGeneratorSettingsSO), "_rangedWeaponsDataSOs").GetValue(hireUnitGeneratorSettingsSO) as List<RangedWeaponDataSO>;
            var meleeWeapons = AccessTools.Field(typeof(HireUnitGeneratorSettingsSO), "_meleeWeaponDataSOs").GetValue(hireUnitGeneratorSettingsSO) as List<MeleeWeaponDataSO>;
            var gears = AccessTools.Field(typeof(HireUnitGeneratorSettingsSO), "_gearDataSOs").GetValue(hireUnitGeneratorSettingsSO) as List<GearDataSO>;

            if (rangedWeapons != null)
            {
                foreach (var weapon in rangedWeapons)
                {
                    initialEquipment.Add(weapon.Id);
                }
            }

            if (meleeWeapons != null)
            {
                foreach (var weapon in meleeWeapons)
                {
                    initialEquipment.Add(weapon.Id);
                }
            }

            if (gears != null)
            {
                foreach (var gear in gears)
                {
                    initialEquipment.Add(gear.Id);
                }
            }

            return initialEquipment;
        }

        // Calculate sell price for equipment
        public static int GetSellPrice(UpgradableUnit unit, EquipmentDataSO equipment)
        {
            int basePrice = equipment.BuyingPrice; // Base sell price is half the buying price
            int battleCount = EquipmentBattleCounts.ContainsKey(unit.UnitId + equipment.Id) ? EquipmentBattleCounts[unit.UnitId + equipment.Id] : 0;
            return basePrice + battleCount;
        }
    }

    
    [HarmonyPatch(typeof(GameManager))]
    public class CelebrityAuction_GameManager_Patch
    {
        [HarmonyPatch("GiveEndGameRewards")]
        [HarmonyPrefix]
        public static bool GiveEndGameRewards(GameManager __instance, bool victory)
        {
            if (!CelebrityAuction.Instance.IsActive)
            {
                return true;
            }
            // Increment battle counts for all equipment in squad
            var playerData = Singleton<Player>.Instance.PlayerData;
            foreach (var unit in playerData.Squad.SquadUnits)
            {
                // Ranged weapon
                var rangedWeapon = unit.GetCurrentEquipmentOfUnit(EquipmentType.Ranged);
                if (rangedWeapon != null)
                {
                    string key = unit.UnitId + rangedWeapon.Id;
                    if (CelebrityAuction.EquipmentBattleCounts.ContainsKey(key))
                    {
                        CelebrityAuction.EquipmentBattleCounts[key]++;
                    }
                    else
                    {
                        CelebrityAuction.EquipmentBattleCounts[key] = 1;
                    }
                }
                // Melee weapon
                var meleeWeapon = unit.GetCurrentEquipmentOfUnit(EquipmentType.Melee);
                if (meleeWeapon != null)
                {
                    string key = unit.UnitId + meleeWeapon.Id;
                    if (CelebrityAuction.EquipmentBattleCounts.ContainsKey(key))
                    {
                        CelebrityAuction.EquipmentBattleCounts[key]++;
                    }
                    else
                    {
                        CelebrityAuction.EquipmentBattleCounts[key] = 1;
                    }
                }
                // Gear
                var gear = unit.GetCurrentEquipmentOfUnit(EquipmentType.Gear);
                if (gear != null)
                {
                    string key = unit.UnitId + gear.Id;
                    if (CelebrityAuction.EquipmentBattleCounts.ContainsKey(key))
                    {
                        CelebrityAuction.EquipmentBattleCounts[key]++;
                    }
                    else
                    {
                        CelebrityAuction.EquipmentBattleCounts[key] = 1;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MenuButtons_SquadManagementDirectory), "Initialize")]
    public class MenuButtons_SquadManagementDirectory_Initialize_Patch
    {
        public static void Postfix(MenuButtons_SquadManagementDirectory __instance, ref DirectoryData __result)
        {
            if (!CelebrityAuction.Instance.IsActive)
            {
                return;
            }

            var _upgrade = (UpgradeType)AccessTools.Field(typeof(MenuButtons_SquadManagementDirectory), "_upgrade").GetValue(__instance);
            if (_upgrade != UpgradeType.WeaponUpgrade)
            {
                return;
            }

            // Add Sell Equipment button to the directory
            if (__result.ButtonData is List<ButtonData> buttons)
            {
                ButtonData sellButton = new()
                {
                    MainText = L("rockstar.celebrity_auction.sell_equipment"),
                    Tooltip = L("rockstar.celebrity_auction.sell_equipment_tooltip"),
                    onClickCallback = () =>
                    {
                        // Enable sell mode
                        CelebrityAuction.IsSellMode = true;

                        // Trigger the OnWeaponUpgrade event to open the weapon selection screen
                        var directoryTextData = AccessTools.Property(typeof(MenuButtons_SquadManagementDirectory), "NameTextDataOfDirectory").GetValue(__instance) as DirectoryTextData;
                        directoryTextData?.SetText(L("rockstar.celebrity_auction.sell_equipment"));

                        var eventField = typeof(MenuButtons_SquadManagementDirectory).GetField("OnWeaponUpgrade", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        var onWeaponUpgrade = eventField?.GetValue(__instance) as Action;
                        onWeaponUpgrade?.Invoke();
                    }
                };

                // Insert the button after the Buy Equipment button (at position 1)
                buttons.Insert(1, sellButton);
            }
        }
    }

    // Patch SelectWeapon_SquadManagementDirectory to show sellable equipment
    [HarmonyPatch(typeof(SelectWeapon_SquadManagementDirectory), "Initialize")]
    public class SelectWeapon_SquadManagementDirectory_Initialize_Patch
    {
        public static void Postfix(SelectWeapon_SquadManagementDirectory __instance, ref DirectoryData __result)
        {
            if (!CelebrityAuction.Instance.IsActive || !CelebrityAuction.IsSellMode)
            {
                return;
            }
            CelebrityAuction.IsSellMode = false;

            // Get initial equipment that cannot be sold
            var initialEquipmentIds = CelebrityAuction.GetInitialEquipmentIds();

            // Get all equipment from squad members
            var playerData = Singleton<Player>.Instance.PlayerData;
            var sellableEquipment = new List<Tuple<UpgradableUnit, EquipmentDataSO>>();

            foreach (var unit in playerData.Squad.SquadUnits)
            {
                // Check ranged weapon
                var rangedWeapon = unit.GetCurrentEquipmentOfUnit(EquipmentType.Ranged);
                if (rangedWeapon != null && !initialEquipmentIds.Contains(rangedWeapon.Id))
                {
                    sellableEquipment.Add(new Tuple<UpgradableUnit, EquipmentDataSO>(unit, rangedWeapon));
                }

                // Check melee weapon
                var meleeWeapon = unit.GetCurrentEquipmentOfUnit(EquipmentType.Melee);
                if (meleeWeapon != null && !initialEquipmentIds.Contains(meleeWeapon.Id) && !sellableEquipment.Contains(new Tuple<UpgradableUnit, EquipmentDataSO>(unit, meleeWeapon)))
                {
                    sellableEquipment.Add(new Tuple<UpgradableUnit, EquipmentDataSO>(unit, meleeWeapon));
                }

                // Check gear
                var gear = unit.GetCurrentEquipmentOfUnit(EquipmentType.Gear);
                if (gear != null && !initialEquipmentIds.Contains(gear.Id) && !sellableEquipment.Contains(new Tuple<UpgradableUnit, EquipmentDataSO>(unit, gear)))
                {
                    sellableEquipment.Add(new Tuple<UpgradableUnit, EquipmentDataSO>(unit, gear));
                }
            }

            // Replace the button list with sell options
            var buttons = new List<ButtonData>();
            var directoriesFlowController = AccessTools.Field(typeof(SelectWeapon_SquadManagementDirectory), "_directoriesFlowController").GetValue(__instance) as DirectoriesFlowController;

            foreach (var pair in sellableEquipment)
            {
                var unit = pair.Item1;
                var equipment = pair.Item2;
                int sellPrice = CelebrityAuction.GetSellPrice(unit, equipment);
                buttons.Add(new ButtonData
                {
                    MainText = $"{equipment.EquipmentName} {unit.UnitName} (+{TextUtils.GetFormattedCoinsWithIcon(sellPrice, false)})",
                    Tooltip = $"Sell this equipment for {sellPrice} coins",
                    onClickCallback = () =>
                    {
                        // Sell the equipment
                        playerData.PlayerWallet.ChangeCoinsByValue(sellPrice);

                        // Find and unequip from all units that have this equipment
                        // Note: We need to replace it with initial equipment of the same type
                        var hireUnitGeneratorSettingsSO = Singleton<AssetsDatabase>.Instance.HireUnitGeneratorSettingsSO;

                        if (unit.GetCurrentEquipmentOfUnit(EquipmentType.Ranged) == equipment)
                        {
                            var defaultRanged = (AccessTools.Field(typeof(HireUnitGeneratorSettingsSO), "_rangedWeaponsDataSOs").GetValue(hireUnitGeneratorSettingsSO) as List<RangedWeaponDataSO>)?[0];
                            if (defaultRanged != null)
                            {
                                unit.ReplaceEquipment(defaultRanged);
                            }
                            else
                            {
                                var _unitData = AccessTools.Field(typeof(UpgradableUnit), "_unitData").GetValue(unit) as UnitData;
                                _unitData.UnitEquipmentManager.RangedWeaponDataSO = null;
                            }
                        }
                        else if (unit.GetCurrentEquipmentOfUnit(EquipmentType.Melee) == equipment)
                        {
                            var defaultMelee = (AccessTools.Field(typeof(HireUnitGeneratorSettingsSO), "_meleeWeaponDataSOs").GetValue(hireUnitGeneratorSettingsSO) as List<MeleeWeaponDataSO>)?[0];
                            if (defaultMelee != null)
                            {
                                unit.ReplaceEquipment(defaultMelee);
                            }
                            else
                            {
                                var _unitData = AccessTools.Field(typeof(UpgradableUnit), "_unitData").GetValue(unit) as UnitData;
                                _unitData.UnitEquipmentManager.MeleeWeaponDataSO = null;
                            }
                        }
                        else if (unit.GetCurrentEquipmentOfUnit(EquipmentType.Gear) == equipment)
                        {
                            var defaultGear = (AccessTools.Field(typeof(HireUnitGeneratorSettingsSO), "_gearDataSOs").GetValue(hireUnitGeneratorSettingsSO) as List<GearDataSO>)?[0];
                            if (defaultGear != null)
                            {
                                unit.ReplaceEquipment(defaultGear);
                            }
                            else
                            {
                                var _unitData = AccessTools.Field(typeof(UpgradableUnit), "_unitData").GetValue(unit) as UnitData;
                                _unitData.UnitEquipmentManager.GearDataSO = null;
                            }
                        }

                        // Disable sell mode and go back
                        directoriesFlowController?.GoBack();
                    }
                });
            }

            // Add a back button
            buttons.Add(new ButtonData
            {
                MainText = "Back",
                Tooltip = "Return to squad management",
                onClickCallback = () =>
                {
                    directoriesFlowController?.GoBack();
                }
            });

            __result.ButtonData = buttons;
        }
    }

    // Disable normal weapon buying in sell mode
    [HarmonyPatch(typeof(EquipSquadList_SquadManagementDirectory), "Initialize")]
    public class EquipSquadList_SquadManagementDirectory_Initialize_Patch
    {
        public static bool Prefix()
        {
            // Skip normal equipment buying if in sell mode
            if (CelebrityAuction.Instance.IsActive && CelebrityAuction.IsSellMode)
            {
                return false;
            }
            return true;
        }
    }
}
