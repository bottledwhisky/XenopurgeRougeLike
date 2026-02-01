using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.Abilities;
using SpaceCommander.Area;
using SpaceCommander.Weapons;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.GunslingerReinforcements
{
    /// <summary>
    /// 跳弹：子弹未命中目标时，有50%概率跳弹到目标当前格或身后的另一个敌人
    /// Ricochet: When a bullet misses the target, there's a 50% chance to ricochet to another enemy on the target's current tile or behind it
    /// </summary>
    public class Ricochet : Reinforcement
    {
        public const float RicochetChance = 0.5f; // 50% chance

        public Ricochet()
        {
            company = Company.Gunslinger;
            rarity = Rarity.Elite;
            stackable = false;
            name = L("gunslinger.ricochet.name");
            flavourText = L("gunslinger.ricochet.flavour");
            description = L("gunslinger.ricochet.description", (int)(RicochetChance * 100));
        }

        protected static Ricochet _instance;
        public static Ricochet Instance => _instance ??= new();
    }

    /// <summary>
    /// Patch RangedWeapon constructor to subscribe to OnAfterAttackWithOneBullet event
    /// </summary>
    [HarmonyPatch(typeof(RangedWeapon), MethodType.Constructor, [typeof(RangedWeaponDataSO), typeof(float), typeof(float), typeof(BattleUnit)])]
    public static class Ricochet_RangedWeapon_Constructor_Patch
    {
        public static void Postfix(RangedWeapon __instance)
        {
            if (!Ricochet.Instance.IsActive)
                return;

            __instance.OnAfterAttackWithOneBullet += (attackInfo) =>
            {
                // Only process misses from player units shooting at enemies
                if (attackInfo.SuccessfulHit)
                    return;

                if (attackInfo.Attacker.Team != Enumerations.Team.Player)
                    return;

                if (attackInfo.Target is not BattleUnit targetUnit)
                    return;

                if (targetUnit.Team != Enumerations.Team.EnemyAI)
                    return;

                // Roll for ricochet chance
                if (Random.Range(0f, 1f) >= Ricochet.RicochetChance)
                    return;

                // Find a ricochet target
                BattleUnit ricochetTarget = RicochetSystem.FindRicochetTarget(attackInfo.Attacker, targetUnit);
                if (ricochetTarget == null)
                    return;

                // Apply ricochet damage
                RicochetSystem.ApplyRicochetDamage(__instance, ricochetTarget);
            };
        }
    }

    /// <summary>
    /// Helper class to handle ricochet logic
    /// </summary>
    public static class RicochetSystem
    {
        /// <summary>
        /// Finds a valid ricochet target on the original target's tile or behind it
        /// </summary>
        public static BattleUnit FindRicochetTarget(BattleUnit attacker, BattleUnit originalTarget)
        {
            if (originalTarget?.MovementManager?.CurrentTile == null)
                return null;

            Tile targetTile = originalTarget.MovementManager.CurrentTile;

            // Get potential targets:
            // 1. Other enemies on the same tile as the original target
            // 2. Enemies on the tile behind the original target
            List<BattleUnit> potentialTargets = [];

            // Get enemies on the same tile (excluding the original target)
            var unitsOnSameTile = targetTile.CurrentStateOfTile.GetUnitsOnTile(Enumerations.Team.EnemyAI)
                .Where(u => u != originalTarget && u.IsAlive);
            potentialTargets.AddRange(unitsOnSameTile);

            // Get the tile behind the original target
            Tile behindTile = GetTileBehind(attacker, targetTile);
            if (behindTile != null)
            {
                var unitsOnBehindTile = behindTile.CurrentStateOfTile.GetUnitsOnTile(Enumerations.Team.EnemyAI)
                    .Where(u => u.IsAlive);
                potentialTargets.AddRange(unitsOnBehindTile);
            }

            // Return a random target from the list
            if (potentialTargets.Count == 0)
                return null;

            int randomIndex = Random.Range(0, potentialTargets.Count);
            return potentialTargets[randomIndex];
        }

        /// <summary>
        /// Gets the tile behind the target from the attacker's perspective
        /// </summary>
        private static Tile GetTileBehind(BattleUnit attacker, Tile targetTile)
        {
            if (attacker?.MovementManager?.CurrentTile == null || targetTile == null)
                return null;

            Tile attackerTile = attacker.MovementManager.CurrentTile;

            // Calculate the direction vector from attacker to target
            Vector2Int attackerCoords = attackerTile.Coords;
            Vector2Int targetCoords = targetTile.Coords;
            Vector2Int direction = targetCoords - attackerCoords;

            // Normalize to get the direction (only consider the primary axis)
            // The game uses a grid system, so we need to find the dominant direction
            Vector2Int normalizedDirection;
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                normalizedDirection = new Vector2Int(direction.x > 0 ? 1 : -1, 0);
            }
            else if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
            {
                normalizedDirection = new Vector2Int(0, direction.y > 0 ? 1 : -1);
            }
            else
            {
                // If diagonal or equal, prefer horizontal
                normalizedDirection = new Vector2Int(direction.x > 0 ? 1 : -1, 0);
            }

            // Calculate the tile behind the target
            Vector2Int behindCoords = targetCoords + normalizedDirection;

            // Get the grid manager and check if the tile exists
            var gridManager = GameManager.Instance?.GridManager;
            if (gridManager == null)
                return null;

            // Check if the tile is within the grid
            return gridManager.IsTileContainedInGrid(behindCoords);
        }

        /// <summary>
        /// Applies ricochet damage to the target
        /// </summary>
        public static void ApplyRicochetDamage(RangedWeapon weapon, BattleUnit target)
        {
            if (target == null || !target.IsAlive)
                return;

            // Apply damage equal to the weapon's damage per bullet
            float damage = weapon.DamagePerBullet;
            target.Damage(damage);

            // Log for debugging (optional)
            MelonLogger.Msg($"Ricochet! Bullet ricocheted to {target.UnitName} for {damage} damage");
        }
    }
}
