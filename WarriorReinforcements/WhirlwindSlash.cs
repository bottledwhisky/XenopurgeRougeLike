using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Abilities;
using SpaceCommander.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.WarriorReinforcements
{
    // 回旋斩：近战攻击会对同一格内所有其它敌人造成原本伤害的50%的附加伤害
    // Whirlwind Slash: Melee attacks deal 50% of the original damage as additional damage to all other enemies on the same tile
    public class WhirlwindSlash : Reinforcement
    {
        public const float DamageMultiplier = 0.5f;

        public WhirlwindSlash()
        {
            company = Company.Warrior;
            rarity = Rarity.Expert;
            stackable = false;
            maxStacks = 1;
            name = L("warrior.whirlwind_slash.name");
            flavourText = L("warrior.whirlwind_slash.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("warrior.whirlwind_slash.description", (int)(DamageMultiplier * 100));
        }

        protected static WhirlwindSlash _instance;
        public static WhirlwindSlash Instance => _instance ??= new();

        /// <summary>
        /// Called when a melee attack hits. Deals AoE damage to other enemies on the same tile.
        /// </summary>
        public static void OnMeleeAttackHit(AttackInfo attackInfo)
        {
            if (!Instance.IsActive)
                return;

            // Only apply to player melee attacks
            if (attackInfo.Attacker?.Team != Enumerations.Team.Player)
                return;

            // Only apply to successful melee attacks
            if (attackInfo.CommandCategory != Enumerations.CommandCategories.CloseCombat)
                return;

            if (!attackInfo.SuccessfulHit)
                return;

            // Get the target's tile
            var target = attackInfo.Target;
            if (target?.CurrentTile == null)
                return;

            var targetTile = target.CurrentTile;
            var attacker = attackInfo.Attacker;

            // Calculate the AoE damage (50% of attacker's Power stat)
            float aoeDamage = attacker.Power * DamageMultiplier;

            // Get all enemies on the same tile
            var enemiesOnTile = targetTile.CurrentStateOfTile.GetUnitsOnTile(Enumerations.Team.EnemyAI);

            // Apply damage to all other enemies on the tile (excluding the primary target)
            foreach (var enemy in new List<BattleUnit>(enemiesOnTile))
            {
                // Skip the primary target and dead units
                if (enemy == target || !enemy.IsAlive)
                    continue;

                // Skip units that are ignoring AOE damage
                if (enemy.IsIgnoringAOEDamage)
                    continue;

                // Deal the AoE damage
                enemy.Damage(aoeDamage);
            }
        }
    }

    /// <summary>
    /// Patch to detect when a melee attack command changes to hook into the attack events
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), "ChangeCommandCategory")]
    public static class WhirlwindSlash_ChangeCommandCategory_Patch
    {
        private static Dictionary<BattleUnit, IMeleeAttackCommand> _trackedMeleeCommands = new();

        public static void Postfix(BattleUnit __instance, ChangedCommandInfo changedCommandInfo)
        {
            if (!WhirlwindSlash.Instance.IsActive)
                return;

            // Only track player units
            if (__instance.Team != Enumerations.Team.Player)
                return;

            // Remove old listener if exists
            if (_trackedMeleeCommands.TryGetValue(__instance, out var oldCommand))
            {
                oldCommand.OnAfterAttack -= WhirlwindSlash.OnMeleeAttackHit;
                _trackedMeleeCommands.Remove(__instance);
            }

            // Add new listener for melee commands
            if (changedCommandInfo.Command is IMeleeAttackCommand meleeAttackCommand)
            {
                meleeAttackCommand.OnAfterAttack += WhirlwindSlash.OnMeleeAttackHit;
                _trackedMeleeCommands[__instance] = meleeAttackCommand;
            }
        }
    }

    /// <summary>
    /// Patch to subscribe to OnDeath event for player units to clean up listeners
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor)]
    public static class WhirlwindSlash_BattleUnit_Constructor_Patch
    {
        public static void Postfix(BattleUnit __instance, Enumerations.Team team)
        {
            if (!WhirlwindSlash.Instance.IsActive)
                return;

            if (team == Enumerations.Team.Player)
            {
                void action()
                {
                    // Clean up listeners when unit dies
                    var trackedCommands = AccessTools.Field(
                        typeof(WhirlwindSlash_ChangeCommandCategory_Patch),
                        "_trackedMeleeCommands"
                    ).GetValue(null) as Dictionary<BattleUnit, IMeleeAttackCommand>;

                    if (trackedCommands != null && trackedCommands.TryGetValue(__instance, out var command))
                    {
                        command.OnAfterAttack -= WhirlwindSlash.OnMeleeAttackHit;
                        trackedCommands.Remove(__instance);
                    }
                    __instance.OnDeath -= action;
                }

                __instance.OnDeath += action;
            }
        }
    }
}
