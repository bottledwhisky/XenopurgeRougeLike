using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Area;
using System.Collections.Generic;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SupportReinforcements
{
    /// <summary>
    /// 瘾君子：在药剂作用下的角色获得+2速度，+2近战伤害，+20命中。药剂效果消失后获得-1速度，-1近战伤害，持续到下次使用药剂。
    /// Addict: Units under injection effects gain +2 Speed, +2 Power, +20 Accuracy.
    /// After injection expires, suffer -1 Speed, -1 Power until next injection.
    /// </summary>
    public class Addict : Reinforcement
    {
        // Bonuses while injection is active
        public const float InjectionSpeedBonus = 2f;
        public const float InjectionPowerBonus = 2f;
        public const float InjectionAccuracyBonus = 0.2f; // +20 accuracy

        // Withdrawal penalties
        public const float WithdrawalSpeedPenalty = -1f;
        public const float WithdrawalPowerPenalty = -1f;

        // Track units with injection buffs and withdrawal debuffs
        public static HashSet<BattleUnit> unitsWithInjection = [];
        public static HashSet<BattleUnit> unitsWithWithdrawal = [];

        // Track which GUIDs belong to injection effects for each unit
        // Key: BattleUnit, Value: HashSet of GUIDs from injection cards
        public static Dictionary<BattleUnit, HashSet<string>> injectionGuidsByUnit = [];

        public Addict()
        {
            company = Company.Support;
            rarity = Rarity.Elite;
            name = L("support.addict.name");
            description = L("support.addict.description",
                (int)InjectionSpeedBonus,
                (int)InjectionPowerBonus,
                (int)(InjectionAccuracyBonus * 100),
                (int)WithdrawalSpeedPenalty,
                (int)WithdrawalPowerPenalty);
            flavourText = L("support.addict.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("support.addict.description",
                (int)InjectionSpeedBonus,
                (int)InjectionPowerBonus,
                (int)(InjectionAccuracyBonus * 100),
                (int)WithdrawalSpeedPenalty,
                (int)WithdrawalPowerPenalty);
        }

        private static Addict _instance;
        public static Addict Instance => _instance ??= new();
    }

    /// <summary>
    /// Patch to clear state when mission starts
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public class Addict_TestGame_StartGame_Patch
    {
        public static void Postfix()
        {
            Addict.unitsWithInjection.Clear();
            Addict.unitsWithWithdrawal.Clear();
            Addict.injectionGuidsByUnit.Clear();
        }
    }

    /// <summary>
    /// Patch to clear state when mission ends
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "EndGame")]
    public class Addict_TestGame_EndGame_Patch
    {
        public static void Postfix()
        {
            Addict.unitsWithInjection.Clear();
            Addict.unitsWithWithdrawal.Clear();
            Addict.injectionGuidsByUnit.Clear();
        }
    }

    /// <summary>
    /// Patch ChangeStat_Card.ApplyCommand to apply bonuses when injections are used
    /// and track the GUIDs of injection stat changes
    /// </summary>
    [HarmonyPatch(typeof(ChangeStat_Card), "ApplyCommand")]
    public static class Addict_InjectionApplied_Patch
    {
        public static void Postfix(ChangeStat_Card __instance, BattleUnit unit)
        {
            if (!Addict.Instance.IsActive)
                return;

            if (unit == null || !unit.IsAlive || unit.Team != Team.Player)
                return;

            if (__instance?.Info == null)
                return;

            // Only apply if this is an injection card
            if (!SupportAffinityHelpers.IsInjectionCard(__instance.Info.Id))
                return;

            // Get the GUIDs that were just created by this injection card
            var guidToReverseChangesField = AccessTools.Field(typeof(ChangeStat_Card), "_guidToReverseChanges");
            var guidToReverseChanges = guidToReverseChangesField.GetValue(__instance) as System.Collections.Generic.List<string>;

            if (guidToReverseChanges != null && guidToReverseChanges.Count > 0)
            {
                // Track these GUIDs for this unit
                if (!Addict.injectionGuidsByUnit.ContainsKey(unit))
                {
                    Addict.injectionGuidsByUnit[unit] = [];
                }

                // Add all GUIDs from this injection card
                foreach (var guid in guidToReverseChanges)
                {
                    Addict.injectionGuidsByUnit[unit].Add(guid);
                }
            }

            // Remove withdrawal debuff if present
            if (Addict.unitsWithWithdrawal.Contains(unit))
            {
                unit.ReverseChangeOfStat("Addict_Withdrawal_Speed");
                unit.ReverseChangeOfStat("Addict_Withdrawal_Power");
                Addict.unitsWithWithdrawal.Remove(unit);
                MelonLogger.Msg($"Addict: Unit {unit.UnitNameNoNumber} withdrawal ended from new injection");
            }

            // Apply injection bonuses (if not already present)
            if (!Addict.unitsWithInjection.Contains(unit))
            {
                unit.ChangeStat(UnitStats.Speed, Addict.InjectionSpeedBonus, "Addict_Injection_Speed");
                unit.ChangeStat(UnitStats.Power, Addict.InjectionPowerBonus, "Addict_Injection_Power");
                unit.ChangeStat(UnitStats.Accuracy, Addict.InjectionAccuracyBonus, "Addict_Injection_Accuracy");

                Addict.unitsWithInjection.Add(unit);
                MelonLogger.Msg($"Addict: Unit {unit.UnitNameNoNumber} gained injection bonuses (+{Addict.InjectionSpeedBonus} Speed, +{Addict.InjectionPowerBonus} Power, +{(int)(Addict.InjectionAccuracyBonus * 100)} Accuracy)");
            }
        }
    }

    /// <summary>
    /// Patch to subscribe to OnDeath event for player units and monitor injection expiration
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor, [typeof(UnitData), typeof(Team), typeof(GridManager)])]
    public class Addict_BattleUnit_Constructor_Patch
    {
        // Static method to check if injection expired by checking if ChangeStat_Card buff is gone
        public static void CheckInjectionStatus(BattleUnit unit)
        {
            if (!Addict.Instance.IsActive)
                return;

            if (unit == null || !unit.IsAlive)
                return;

            if (!Addict.unitsWithInjection.Contains(unit))
                return;

            // Check if any of the three injection stat changes are still active
            // We use reflection to access the private _temporaryStatChanges dictionary
            var temporaryStatChangesField = AccessTools.Field(typeof(BattleUnit), "_temporaryStatChanges");
            var temporaryStatChanges = temporaryStatChangesField.GetValue(unit) as System.Collections.Generic.Dictionary<string, float>;

            if (temporaryStatChanges == null)
                return;

            // Injection cards create their own stat changes, not our Addict stat changes
            // We need to check if the ORIGINAL injection stat changes are still active
            // The injection cards use specific IDs, but we need to check their effect duration
            // Since injections expire, we detect this by monitoring if the stat change reverses

            // Actually, we can't easily detect when the injection expires this way
            // Instead, we'll patch the ReverseChangeOfStat method to detect when injection effects end
        }

        public static void Postfix(BattleUnit __instance, Team team)
        {
            if (!Addict.Instance.IsActive)
                return;

            if (team == Team.Player)
            {
                void action()
                {
                    // Clean up on death
                    Addict.unitsWithInjection.Remove(__instance);
                    Addict.unitsWithWithdrawal.Remove(__instance);
                    Addict.injectionGuidsByUnit.Remove(__instance);
                    __instance.OnDeath -= action;
                }

                __instance.OnDeath += action;
            }
        }
    }

    /// <summary>
    /// Patch BattleUnit.ReverseChangeOfStat to detect when injection effects expire
    /// This allows us to apply withdrawal debuff when injection ends
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), "ReverseChangeOfStat")]
    public static class Addict_InjectionExpired_Patch
    {
        public static void Postfix(BattleUnit __instance, string guidOfChangeToReverseIt)
        {
            if (!Addict.Instance.IsActive)
                return;

            if (__instance == null || !__instance.IsAlive || __instance.Team != Team.Player)
                return;

            if (guidOfChangeToReverseIt == null)
                return;

            // Only proceed if we're tracking injection GUIDs for this unit
            if (!Addict.injectionGuidsByUnit.ContainsKey(__instance))
                return;

            // Check if this GUID belongs to an injection effect
            var injectionGuids = Addict.injectionGuidsByUnit[__instance];
            if (!injectionGuids.Contains(guidOfChangeToReverseIt))
                return;

            // Remove this GUID from tracking
            injectionGuids.Remove(guidOfChangeToReverseIt);

            // If no injection GUIDs remain, the injection has fully expired
            if (injectionGuids.Count == 0)
            {
                // Clean up tracking
                Addict.injectionGuidsByUnit.Remove(__instance);

                // Only apply withdrawal if unit had injection bonuses
                if (Addict.unitsWithInjection.Contains(__instance))
                {
                    // Remove injection bonuses
                    __instance.ReverseChangeOfStat("Addict_Injection_Speed");
                    __instance.ReverseChangeOfStat("Addict_Injection_Power");
                    __instance.ReverseChangeOfStat("Addict_Injection_Accuracy");
                    Addict.unitsWithInjection.Remove(__instance);

                    // Apply withdrawal debuff
                    if (!Addict.unitsWithWithdrawal.Contains(__instance))
                    {
                        __instance.ChangeStat(UnitStats.Speed, Addict.WithdrawalSpeedPenalty, "Addict_Withdrawal_Speed");
                        __instance.ChangeStat(UnitStats.Power, Addict.WithdrawalPowerPenalty, "Addict_Withdrawal_Power");
                        Addict.unitsWithWithdrawal.Add(__instance);

                        MelonLogger.Msg($"Addict: Unit {__instance.UnitNameNoNumber} entered withdrawal ({Addict.WithdrawalSpeedPenalty} Speed, {Addict.WithdrawalPowerPenalty} Power)");
                    }
                }
            }
        }
    }
}
