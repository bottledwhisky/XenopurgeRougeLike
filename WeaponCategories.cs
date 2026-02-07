using SpaceCommander;
using SpaceCommander.Weapons;
using System.Collections.Generic;

namespace XenopurgeRougeLike
{
    /// <summary>
    /// Helper class for categorizing weapons by type.
    /// Used by weapon specialist reinforcements to identify equipped weapon types.
    /// </summary>
    public static class WeaponCategories
    {
        // Pistols (手枪)
        private static readonly HashSet<string> PistolIds =
        [
            "11e1f4a6-a123-43d5-bed6-d0f2c832d249", // MOP多用途手枪
            "4527bc2d-acf7-4bfa-af21-d3338ca3a2c0", // HEX手枪
            "4133cb33-a728-43d4-93ac-8acc5a5aaf45", // BOLT 冲锋枪 (SMG but functions like pistol)
        ];

        // Shotguns (霰弹枪)
        private static readonly HashSet<string> ShotgunIds =
        [
            "cbbe367a-9dfe-4f0d-983e-57f7c6137414", // BASO 霰弹枪
            "139c7732-03bb-427f-bc3a-06bc0e9a6fd0", // PAX喷射型霰弹枪
        ];

        // Rifles (步枪)
        private static readonly HashSet<string> RifleIds =
        [
            "d0eeba04-1307-4af5-9e08-c40ab6509422", // RAT战术突击步枪
            "a90e5abf-e0c6-4f33-bef6-863a0b03d8fb", // BARK 系统
            "72d89958-eb04-4241-9edc-4f45c1170ceb", // SIN 迷你枪
        ];

        // Sniper Rifles (狙击枪)
        private static readonly HashSet<string> SniperRifleIds =
        [
            "b93fd27b-ad4d-4038-baa2-bcc5ea355eeb", // SAP半自动精确步枪
            "01d20dca-e096-49d6-87c3-29dfb25ea277", // SAP半自动精确步枪 (variant)
            "4eb6f6c8-5070-4725-a1bc-0dd98fadeddf", // TRAC 卡宾枪
            "7cea0fe5-f551-4dd4-ad17-2ed527228a87", // MASS 卡宾枪
        ];

        // Daggers (匕首)
        private static readonly HashSet<string> DaggerIds =
        [
            "fe8dd9a1-d175-4cd8-8210-5b41d2571e35", // Combat Knife
            "89e2f1bc-fe4d-44fc-85ba-28ace2a8a000", // Dataflow Bayonet
        ];

        // Blunt Weapons (钝器)
        private static readonly HashSet<string> BluntWeaponIds =
        [
            "6f74b65f-a0b8-4209-9a21-fa3168543ed7", // Sledgehammer
            "a5106524-66ab-4350-8c29-e98e2aaa1205", // Kinetic Warhammer
            "8ef62062-7389-4a6b-89f0-8e2798f5d499", // Combat Shield
            "d4102bbe-ffc0-4607-867f-6879686da2e3", // Hemogrip
            "14ac2d50-817b-4d5e-b894-12b5b045d45d", // Exo-Gauntlets
        ];

        // Bladed Weapons (剑刃)
        private static readonly HashSet<string> BladeIds =
        [
            "25899b0d-bc6c-4528-90f5-be84a0e9e9e6", // Powersword
            "3492a991-e763-4556-ad10-703bcb49bf54", // Powerclaw
            "3858b961-9a2b-49b4-92a1-e8421cd5c1ea", // Gunblade
            "857de244-6fa5-4a79-99ed-8aadf0577c6c", // MATE斧 (throwing axe)
        ];

        public static bool IsPistol(EquipmentDataSO weapon) => weapon != null && PistolIds.Contains(weapon.Id);
        public static bool IsShotgun(EquipmentDataSO weapon) => weapon != null && ShotgunIds.Contains(weapon.Id);
        public static bool IsRifle(EquipmentDataSO weapon) => weapon != null && RifleIds.Contains(weapon.Id);
        public static bool IsSniperRifle(EquipmentDataSO weapon) => weapon != null && SniperRifleIds.Contains(weapon.Id);
        public static bool IsDagger(EquipmentDataSO weapon) => weapon != null && DaggerIds.Contains(weapon.Id);
        public static bool IsBluntWeapon(EquipmentDataSO weapon) => weapon != null && BluntWeaponIds.Contains(weapon.Id);
        public static bool IsBlade(EquipmentDataSO weapon) => weapon != null && BladeIds.Contains(weapon.Id);

        // Check if unit is using a specific weapon category
        public static bool IsUsingPistol(BattleUnit unit) => IsPistol(unit?.WeaponDataSO);
        public static bool IsUsingShotgun(BattleUnit unit) => IsShotgun(unit?.WeaponDataSO);
        public static bool IsUsingRifle(BattleUnit unit) => IsRifle(unit?.WeaponDataSO);
        public static bool IsUsingSniperRifle(BattleUnit unit) => IsSniperRifle(unit?.WeaponDataSO);
        public static bool IsUsingDagger(BattleUnit unit) => IsDagger(unit?.MeleeWeaponDataSO);
        public static bool IsUsingBluntWeapon(BattleUnit unit) => IsBluntWeapon(unit?.MeleeWeaponDataSO);
        public static bool IsUsingBlade(BattleUnit unit) => IsBlade(unit?.MeleeWeaponDataSO);
    }
}
