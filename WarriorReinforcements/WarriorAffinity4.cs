using HarmonyLib;
using SpaceCommander;
using System;
using System.Collections.Generic;
using TimeSystem;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.WarriorReinforcements
{
    // 勇士路径天赋4：近战伤害+4，霰弹枪每一枪投射物数量+2，血量低于50%时会缓慢回复，同流派增援获得概率提升
    // Warrior Affinity 4: Melee damage +4, shotguns fire +2 projectiles per shot, regenerate health when below 50% HP, increased same-company reinforcement probability
    public class WarriorAffinity4 : CompanyAffinity
    {
        public const int MeleeDamageBonus = 4;
        public const int ShotgunProjectileBonus = 2;
        public const float ReinforcementChanceBonus = 2f;
        public const float RegenInterval = 5f; // 5 seconds per 1 HP (from spec: 5秒1点血量)
        public const float RegenAmount = 1f;

        public WarriorAffinity4()
        {
            unlockLevel = 4;
            company = Company.Warrior;
            description = L("warrior.affinity4.description", MeleeDamageBonus, ShotgunProjectileBonus, (int)RegenInterval, (int)ReinforcementChanceBonus);
        }

        public static WarriorAffinity4 _instance;
        public static WarriorAffinity4 Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register melee damage boost
            WarriorAffinityHelpers.RegisterMeleeDamageBoost("WarriorAffinity4_MeleeDamage", MeleeDamageBonus);

            // Register reinforcement probability modifier
            AwardSystem.WeightModifiers.Add(ModifyWeights);
        }

        public override void OnDeactivate()
        {
            // Remove melee damage boost
            WarriorAffinityHelpers.RemoveMeleeDamageBoost("WarriorAffinity4_MeleeDamage");

            // Remove reinforcement probability modifier
            AwardSystem.WeightModifiers.Remove(ModifyWeights);
        }

        private void ModifyWeights(List<Tuple<int, Reinforcement>> choices)
        {
            WarriorAffinityHelpers.ModifyReinforcementWeights(choices, ReinforcementChanceBonus);
        }
    }

    /// <summary>
    /// Tracks health regeneration state for units
    /// </summary>
    public class WarriorHealthRegenTracker
    {
        public BattleUnit Unit;
        public float TimeUntilNextRegen;
        public bool IsActive;

        public WarriorHealthRegenTracker(BattleUnit unit)
        {
            Unit = unit;
            TimeUntilNextRegen = WarriorAffinity4.RegenInterval;
            IsActive = true;
        }
    }

    /// <summary>
    /// Manages health regeneration for all units with Warrior Affinity 4 or 6 active.
    /// Affinity 4: Regenerates health when below 50% HP.
    /// Affinity 6: Regenerates health AND gains armor equal to the regenerated amount.
    /// </summary>
    public static class WarriorHealthRegenManager
    {
        private static readonly Dictionary<BattleUnit, WarriorHealthRegenTracker> _trackedUnits = [];
        private static bool _isSubscribed = false;

        public static void StartTracking(BattleUnit unit)
        {
            if (!_trackedUnits.ContainsKey(unit))
            {
                _trackedUnits[unit] = new WarriorHealthRegenTracker(unit);

                // Subscribe to time updates if not already subscribed
                if (!_isSubscribed && TempSingleton<TimeManager>.Instance != null)
                {
                    TempSingleton<TimeManager>.Instance.OnTimeUpdated += OnTimeUpdate;
                    _isSubscribed = true;
                }
            }
        }

        public static void StopTracking(BattleUnit unit)
        {
            if (_trackedUnits.ContainsKey(unit))
            {
                _trackedUnits.Remove(unit);
            }
        }

        public static void ClearAll()
        {
            _trackedUnits.Clear();

            // Unsubscribe from time updates
            if (_isSubscribed && TempSingleton<TimeManager>.Instance != null)
            {
                TempSingleton<TimeManager>.Instance.OnTimeUpdated -= OnTimeUpdate;
                _isSubscribed = false;
            }
        }

        private static void OnTimeUpdate(float deltaTime)
        {
            // Check if either affinity 4 or 6 is active
            bool affinity4Active = WarriorAffinity4.Instance.IsActive;
            bool affinity6Active = WarriorAffinity6.Instance.IsActive;

            if (!affinity4Active && !affinity6Active)
                return;

            // Clean up dead units
            List<BattleUnit> toRemove = [];
            foreach (var tracker in _trackedUnits.Values)
            {
                if (tracker.Unit == null || !tracker.Unit.IsAlive)
                {
                    toRemove.Add(tracker.Unit);
                }
            }
            foreach (var unit in toRemove)
            {
                _trackedUnits.Remove(unit);
            }

            float RegenInterval = affinity4Active ? WarriorAffinity4.RegenInterval : WarriorAffinity6.RegenInterval;

            // Process regeneration for living units
            foreach (var tracker in _trackedUnits.Values)
            {
                var unit = tracker.Unit;

                // Check if unit is below 50% health
                if (unit.CurrentHealth < unit.CurrentMaxHealth * 0.5f)
                {
                    tracker.TimeUntilNextRegen -= deltaTime;

                    if (tracker.TimeUntilNextRegen <= 0f)
                    {
                        float regenAmount = affinity4Active ? WarriorAffinity4.RegenAmount : WarriorAffinity6.RegenAmount;

                        // Regenerate health
                        unit.Heal(regenAmount);

                        // If affinity 6 is active, also gain armor equal to the regenerated amount
                        if (affinity6Active)
                        {
                            UnitStatsTools.AddArmorToUnit(unit, regenAmount);
                        }

                        // Reset timer
                        tracker.TimeUntilNextRegen = RegenInterval;
                    }
                }
                else
                {
                    // Reset timer when above 50% health
                    tracker.TimeUntilNextRegen = RegenInterval;
                }
            }
        }
    }

    /// <summary>
    /// Patch to start tracking player units for health regeneration when they spawn (for affinity 4 or 6)
    /// </summary>
    [HarmonyPatch(typeof(BattleUnitsManager), "AddBattleUnit")]
    public static class WarriorAffinity4_StartHealthRegenTracking_Patch
    {
        public static void Postfix(BattleUnit battleUnit)
        {
            // Only track player units
            if (battleUnit == null || battleUnit.Team != Enumerations.Team.Player)
                return;

            // Check if either affinity 4 or 6 is active
            if (!WarriorAffinity4.Instance.IsActive && !WarriorAffinity6.Instance.IsActive)
                return;

            WarriorHealthRegenManager.StartTracking(battleUnit);
        }
    }

    /// <summary>
    /// Patch to clean up health regen tracking when battle ends
    /// </summary>
    [HarmonyPatch(typeof(TestGame), "EndGame")]
    public static class WarriorAffinity4_ClearHealthRegenTracking_Patch
    {
        public static void Postfix()
        {
            WarriorHealthRegenManager.ClearAll();
        }
    }
}
