using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Abilities;
using SpaceCommander.Area;
using SpaceCommander.Commands;
using System.Collections.Generic;
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
            description = L("warrior.whirlwind_slash.description", (int)(DamageMultiplier * 100));
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
    /// Patch BattleUnit constructor to subscribe to melee attack events and set up cleanup on death
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor, [typeof(UnitData), typeof(Enumerations.Team), typeof(GridManager)])]
    public static class WhirlwindSlash_BattleUnit_Constructor_Patch
    {
        public static void Postfix(BattleUnit __instance, Enumerations.Team team)
        {
            if (!WhirlwindSlash.Instance.IsActive)
                return;

            // Only track player units
            if (team != Enumerations.Team.Player)
                return;

            // Get the commands from CommandsManager
            var commandsManager = __instance.CommandsManager;
            if (commandsManager == null)
                return;

            // Subscribe to melee attack events for all melee commands
            var commands = commandsManager.Commands;
            foreach (var command in commands)
            {
                if (command is IMeleeAttackCommand meleeAttackCommand)
                {
                    meleeAttackCommand.OnAfterAttack += WhirlwindSlash.OnMeleeAttackHit;
                }
            }

            // Set up cleanup when unit dies
            void action()
            {
                // Clean up listeners when unit dies
                var cmds = __instance.CommandsManager?.Commands;
                if (cmds != null)
                {
                    foreach (var command in cmds)
                    {
                        if (command is IMeleeAttackCommand meleeAttackCommand)
                        {
                            meleeAttackCommand.OnAfterAttack -= WhirlwindSlash.OnMeleeAttackHit;
                        }
                    }
                }
                __instance.OnDeath -= action;
            }

            __instance.OnDeath += action;
        }
    }
}
