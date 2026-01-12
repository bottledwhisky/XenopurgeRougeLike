using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.Audio;
using SpaceCommander.Commands;
using SpaceCommander.Database;
using SpaceCommander.EndGame;
using SpaceCommander.PartyCustomization;
using SpaceCommander.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using static SpaceCommander.Enumerations;

namespace XenopurgeRougeLike.RockstarReinforcements
{
    /// <summary>
    /// A fake UnitDataSO that wraps a prepared UnitData for spawning purposes
    /// </summary>
    public class FanUnitDataSO : UnitDataSO
    {
        private UnitData _preparedUnitData;

        public static FanUnitDataSO Create(UnitData unitData)
        {
            var instance = UnityEngine.ScriptableObject.CreateInstance<FanUnitDataSO>();
            instance._preparedUnitData = unitData;
            return instance;
        }

        public new UnitData CreateUnitInstance()
        {
            return _preparedUnitData;
        }
    }

    internal static class RockstarAffinityHelpers
    {
        public const string FAN_NAME = "Fan";
        public static int fanCount = 0;
        public static int fanGainLow = 500;
        public static int fanGainHigh = 1500;
        public static int fanPenaltyDead = 500;
        public static int fanBonusObjective = 500;
        public static int fanMoney = 10;

        static AssaultCommandDataSO assaultSO;
        public static AssaultCommandDataSO AssaultCommandDataSO => assaultSO ??= UnityEngine.Resources.FindObjectsOfTypeAll<AssaultCommandDataSO>().FirstOrDefault();

        static HitAndRunCommandDataSO hitAndRunSO;
        public static HitAndRunCommandDataSO HitAndRunCommandDataSO => hitAndRunSO ??= UnityEngine.Resources.FindObjectsOfTypeAll<HitAndRunCommandDataSO>().FirstOrDefault();

        static RunAndGunCommandDataSO runAndGunSO;
        public static RunAndGunCommandDataSO RunAndGunCommandDataSO => runAndGunSO ??= UnityEngine.Resources.FindObjectsOfTypeAll<RunAndGunCommandDataSO>().FirstOrDefault();

        static SuppressiveFireCommandDataSO suppressiveFireSO;
        public static SuppressiveFireCommandDataSO SuppressiveFireCommandDataSO => suppressiveFireSO ??= UnityEngine.Resources.FindObjectsOfTypeAll<SuppressiveFireCommandDataSO>().FirstOrDefault();

        static FallbackCommandDataSO fallbackSO;
        public static FallbackCommandDataSO FallbackCommandDataSO => fallbackSO ??= UnityEngine.Resources.FindObjectsOfTypeAll<FallbackCommandDataSO>().FirstOrDefault();

        static StandAndFightCommandDataSO standAndFightSO;
        public static StandAndFightCommandDataSO StandAndFightCommandDataSO => standAndFightSO ??= UnityEngine.Resources.FindObjectsOfTypeAll<StandAndFightCommandDataSO>().FirstOrDefault();

        static CommandDataSO[] ShootingCommands => [AssaultCommandDataSO, FallbackCommandDataSO, StandAndFightCommandDataSO];
        static CommandDataSO[] UpgradedShootingCommands => [HitAndRunCommandDataSO, RunAndGunCommandDataSO, SuppressiveFireCommandDataSO];

        static List<EquipmentDataSO> RangedWeapons => Singleton<AssetsDatabase>.Instance.UpgradeDataSO.UpgradeRangedWeaponsList;
        static List<EquipmentDataSO> MeleeWeapons => Singleton<AssetsDatabase>.Instance.UpgradeDataSO.UpgradeMeleeWeaponsList;
        static List<EquipmentDataSO> Gears => Singleton<AssetsDatabase>.Instance.UpgradeDataSO.UpgradeGearsList;

        public static void SetFanUnitStats(UnitData ud)
        {
            ud.Health = 30;
            ud.CurrentHealth = 30;
            ud.Accuracy = 0.5f;
            ud.Power = 5;
            ud.Speed = 25;

            HireUnitGeneratorSettingsSO hireUnitGeneratorSettingsSO = Singleton<AssetsDatabase>.Instance.HireUnitGeneratorSettingsSO;

            List<RangedWeaponDataSO> _rangedWeaponsDataSOs = AccessTools.Field(typeof(HireUnitGeneratorSettingsSO), "_rangedWeaponsDataSOs").GetValue(hireUnitGeneratorSettingsSO) as List<RangedWeaponDataSO>;
            List<MeleeWeaponDataSO> _meleeWeaponsDataSOs = AccessTools.Field(typeof(HireUnitGeneratorSettingsSO), "_meleeWeaponDataSOs").GetValue(hireUnitGeneratorSettingsSO) as List<MeleeWeaponDataSO>;
            List<GearDataSO> _gearDataSOs = AccessTools.Field(typeof(HireUnitGeneratorSettingsSO), "_gearDataSOs").GetValue(hireUnitGeneratorSettingsSO) as List<GearDataSO>;

            if (_rangedWeaponsDataSOs.Count > 0)
            {
                ud.UnitEquipmentManager.RangedWeaponDataSO = _rangedWeaponsDataSOs[UnityEngine.Random.Range(0, _rangedWeaponsDataSOs.Count)];
                MelonLogger.Msg($"Assigned ranged weapon: {ud.UnitEquipmentManager.RangedWeaponDataSO.name}");
            }
            else
            {
                ud.UnitEquipmentManager.RangedWeaponDataSO = null;
                MelonLogger.Msg("No ranged weapons found in the game database.");
            }
            if (_meleeWeaponsDataSOs.Count > 0)
            {
                ud.UnitEquipmentManager.MeleeWeaponDataSO = _meleeWeaponsDataSOs[UnityEngine.Random.Range(0, _meleeWeaponsDataSOs.Count)];
                MelonLogger.Msg($"Assigned melee weapon: {ud.UnitEquipmentManager.MeleeWeaponDataSO.name}");
            }
            else
            {
                ud.UnitEquipmentManager.MeleeWeaponDataSO = null;
                MelonLogger.Msg("No melee weapons found in the game database.");
            }
            if (_gearDataSOs.Count > 0)
            {
                ud.UnitEquipmentManager.GearDataSO = _gearDataSOs[UnityEngine.Random.Range(0, _gearDataSOs.Count)];
                MelonLogger.Msg($"Assigned gear: {ud.UnitEquipmentManager.GearDataSO.name}");
            }
            else
            {
                ud.UnitEquipmentManager.GearDataSO = null;
                MelonLogger.Msg("No gears found in the game database.");
            }

            bool boughtRanged = false;
            bool boughtMelee = false;
            bool boughtGear = false;
            int money = fanMoney;
            int statCost = 2;
            while (money > 0)
            {
                List<Action> actions = [];
                // Upgrade stats
                if (money >= statCost)
                {
                    actions.Add(() =>
                    {
                        ud.Health += 5;
                        ud.CurrentHealth += 5;
                        money -= statCost;
                        statCost++;
                        MelonLogger.Msg($"Increased health to {ud.Health}");
                    });
                    actions.Add(() =>
                    {
                        ud.Accuracy += 0.1f;
                        money -= statCost;
                        statCost++;
                        MelonLogger.Msg($"Increased accuracy to {ud.Accuracy}");
                    });
                    actions.Add(() =>
                    {
                        ud.Power += 1;
                        money -= statCost;
                        statCost++;
                        MelonLogger.Msg($"Increased power to {ud.Power}");
                    });
                    actions.Add(() =>
                    {
                        ud.Speed += 1;
                        money -= statCost;
                        statCost++;
                        MelonLogger.Msg($"Increased speed to {ud.Speed}");
                    });
                }
                // Buy ranged weapon
                if (!boughtRanged)
                {
                    foreach (var weapon in RangedWeapons)
                    {
                        if (weapon is RangedWeaponDataSO rangedWeapon && weapon.BuyingPrice <= money)
                        {
                            actions.Add(() =>
                            {
                                ud.UnitEquipmentManager.RangedWeaponDataSO = rangedWeapon;
                                money -= weapon.BuyingPrice;
                                boughtRanged = true;
                                MelonLogger.Msg($"Assigned ranged weapon: {rangedWeapon.name}");
                            });
                        }
                    }
                }
                // Buy melee weapon
                if (!boughtMelee)
                {
                    foreach (var weapon in MeleeWeapons)
                    {
                        if (weapon is MeleeWeaponDataSO meleeWeapon && weapon.BuyingPrice <= money)
                        {
                            actions.Add(() =>
                            {
                                ud.UnitEquipmentManager.MeleeWeaponDataSO = meleeWeapon;
                                money -= weapon.BuyingPrice;
                                boughtMelee = true;
                                MelonLogger.Msg($"Assigned melee weapon: {meleeWeapon.name}");
                            });
                        }
                    }
                }
                // Buy gear
                if (!boughtGear)
                {
                    foreach (var gear in Gears)
                    {
                        if (gear is GearDataSO gearData && gear.BuyingPrice <= money)
                        {
                            actions.Add(() =>
                            {
                                ud.UnitEquipmentManager.GearDataSO = gearData;
                                money -= gear.BuyingPrice;
                                boughtGear = true;
                                MelonLogger.Msg($"Assigned gear: {gearData.name}");
                            });
                        }
                    }
                }

                // Execute a random action
                if (actions.Count == 0)
                {
                    break;
                }
                var action = actions[UnityEngine.Random.Range(0, actions.Count)];
                action();
            }
            MelonLogger.Msg($"Assigned stats: {ud.Health}, {ud.Accuracy}, {ud.Power}, {ud.Speed}");
        }

        public static void SetShootingCommand(UnitData ud)
        {
            CommandDataSO attackCommand;

            // If In the Spotlight is active, fans use Run-and-Gun
            if (InTheSpotlight.Instance.IsActive)
            {
                if (RockstarAffinity4.Instance.IsActive || RockstarAffinity6.Instance.IsActive)
                {
                    attackCommand = RunAndGunCommandDataSO;
                }
                else
                {
                    attackCommand = AssaultCommandDataSO;
                }
            }
            else if (RockstarAffinity4.Instance.IsActive || RockstarAffinity6.Instance.IsActive)
            {
                attackCommand = UpgradedShootingCommands[UnityEngine.Random.Range(0, UpgradedShootingCommands.Count())];
            }
            else
            {
                attackCommand = ShootingCommands[UnityEngine.Random.Range(0, ShootingCommands.Count())];
            }

            var cmdList = ud.CommandsDataSOList;
            for (int i = 0; i < cmdList.Length; i++)
            {
                var cmd = cmdList[i];
                if (cmd.CommandCategory == CommandCategories.Shooting)
                {
                    cmdList[i] = attackCommand;
                    break;
                }
            }
        }

        /// <summary>
        /// Creates a new Fan UnitData with randomized stats, equipment, and voice
        /// </summary>
        public static UnitData CreateFanUnitData()
        {
            var playerData = Singleton<Player>.Instance.PlayerData;
            var ud = playerData.Squad.SquadUnits.First().GetCopyOfUnitData();
            ud.UnitId = Guid.NewGuid().ToString();
            ud.UnitName = FAN_NAME;
            ud.UnitTag = UnitTag.None;
            ud.UnitNameLocalizedStringIndex = -5;

            var hug = Singleton<AssetsDatabase>.Instance.HireUnitGeneratorSettingsSO;
            var _voiceActingListSO = AccessTools.Field(typeof(HireUnitGeneratorSettingsSO), "_voiceActingListSO").GetValue(hug) as VoiceActingListSO;

            bool gender = UnityEngine.Random.Range(0, 2) == 0;
            ud.VoiceActorGUID = _voiceActingListSO.GetRandomVoiceActor(gender ? Gender.female : Gender.male).AssetGUID;

            SetShootingCommand(ud);
            var cmdList = ud.CommandsDataSOList.ToList();
            cmdList.Insert(2, UnitsPlacementPhasePatch.HuntCommandDataSO);
            ud.CommandsDataSOList = cmdList.ToArray();
            SetFanUnitStats(ud);

            return ud;
        }
    }

    [HarmonyPatch(typeof(TestGame), "EndGame")]
    public static class RockstarAffinity2FanCount_Patch
    {
        public static void Postfix(TestGame __instance, EndGameResultData data)
        {
            if (!RockstarAffinity2.IsAnyRockstarAffinityActive)
            {
                return;
            }
            if (data.IsVictory)
            {
                var nDead = data.UnitsKilled.Count();
                var nObjectives = data.ObjectivesStatuses.Count(obj => obj.Item3);
                var baseNumber = UnityEngine.Random.Range(RockstarAffinityHelpers.fanGainLow, RockstarAffinityHelpers.fanGainHigh);

                var fanDelta = baseNumber - nDead * RockstarAffinityHelpers.fanPenaltyDead + nObjectives * RockstarAffinityHelpers.fanBonusObjective;

                if (Rockstar.StarPower.IsActive && Rockstar.StarPower.IsPerfectVictory(data))
                {
                    fanDelta = (int)(fanDelta * (1 + StarPower.FanBonusMultiplier));
                }

                if (BuildingTheBrand.Instance.IsActive)
                {
                    fanDelta = (int)(fanDelta * BuildingTheBrand.FanMultiplier);
                }

                RockstarAffinityHelpers.fanCount += fanDelta;
                MelonLogger.Msg($"RockstarAffinity2FanCount_Patch: gained {fanDelta} fans to {RockstarAffinityHelpers.fanCount}");
            }
        }
    }
}