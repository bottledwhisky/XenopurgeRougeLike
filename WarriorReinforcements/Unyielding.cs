using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Area;
using System.Collections.Generic;
using UnityEngine;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.WarriorReinforcements
{
    // 不屈：受到致死伤害时保留1血，每场1/2次（可叠加2）
    // Unyielding: When taking lethal damage, survive with 1 HP, 1/2 times per battle (stackable up to 2)
    public class Unyielding : Reinforcement
    {
        public static readonly int[] UsesPerBattle = [1, 2];

        // Track remaining uses per unit during the current battle
        private static Dictionary<BattleUnit, int> _unitRemainingUses = new();

        public Unyielding()
        {
            company = Company.Warrior;
            rarity = Rarity.Elite;
            stackable = true;
            maxStacks = 2;
            name = L("warrior.unyielding.name");
            flavourText = L("warrior.unyielding.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("warrior.unyielding.description", UsesPerBattle[stacks - 1]);
        }

        public override void OnActivate()
        {
            // Patches are automatically applied when Harmony is initialized
        }

        public override void OnDeactivate()
        {
            // Clear tracking data
            _unitRemainingUses.Clear();
        }

        /// <summary>
        /// Initialize remaining uses for a unit at battle start
        /// </summary>
        public static void InitializeUnitUses(BattleUnit unit)
        {
            if (!Instance.IsActive)
                return;

            int uses = UsesPerBattle[Instance.currentStacks - 1];
            _unitRemainingUses[unit] = uses;
        }

        /// <summary>
        /// Check if unit has uses remaining and consume one if available
        /// </summary>
        public static bool TryConsumeUse(BattleUnit unit)
        {
            if (!_unitRemainingUses.ContainsKey(unit))
            {
                InitializeUnitUses(unit);
            }

            if (_unitRemainingUses[unit] > 0)
            {
                _unitRemainingUses[unit]--;
                Debug.Log($"Unyielding: {unit.UnitName} survived lethal damage! Remaining uses: {_unitRemainingUses[unit]}");
                return true;
            }

            return false;
        }

        protected static Unyielding _instance;
        public static Unyielding Instance => _instance ??= new();
    }

    /// <summary>
    /// Initialize uses for each unit at battle start
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor, [typeof(UnitData), typeof(Enumerations.Team), typeof(GridManager)])]
    public static class Unyielding_InitializeUses_Patch
    {
        public static void Postfix(BattleUnit __instance, Enumerations.Team team)
        {
            if (!Unyielding.Instance.IsActive || team != Enumerations.Team.Player)
                return;

            Unyielding.InitializeUnitUses(__instance);
        }
    }

    /// <summary>
    /// Prevent lethal damage and keep unit at 1 HP
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), "Damage")]
    public static class Unyielding_PreventLethalDamage_Patch
    {
        public static void Prefix(BattleUnit __instance, ref float damage)
        {
            if (!Unyielding.Instance.IsActive)
                return;

            // Only apply to player team units
            if (__instance.Team != Enumerations.Team.Player)
                return;

            // Check if this damage would be lethal
            float currentHealth = __instance.CurrentHealth;
            if (currentHealth - damage <= 0f && currentHealth > 0f)
            {
                // Try to consume an Unyielding use
                if (Unyielding.TryConsumeUse(__instance))
                {
                    // Reduce damage so unit survives with 1 HP
                    damage = currentHealth - 1f;

                    // Visual/audio feedback would be nice but may require more game knowledge
                    Debug.Log($"Unyielding: {__instance.UnitName} refuses to fall!");
                }
            }
        }
    }
}
