using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.Area;
using SpaceCommander.Weapons;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.GunslingerReinforcements
{
    /// <summary>
    /// 穿透弹：暴击时子弹穿透目标，对目标当前格及其后方的所有敌人造成等同于非暴击伤害的额外伤害
    /// Penetrating Rounds: On critical hit, bullets penetrate the target, dealing additional damage equal to non-crit damage to all enemies on the target's tile and behind it
    /// </summary>
    public class PenetratingRounds : Reinforcement
    {
        public PenetratingRounds()
        {
            company = Company.Gunslinger;
            rarity = Rarity.Elite;
            stackable = false;
            name = L("gunslinger.penetrating_rounds.name");
            flavourText = L("gunslinger.penetrating_rounds.flavour");
            description = L("gunslinger.penetrating_rounds.description");
        }

        protected static PenetratingRounds _instance;
        public static PenetratingRounds Instance => _instance ??= new();
    }

    /// <summary>
    /// Patch RangedWeapon constructor to subscribe to OnAfterAttackWithOneBullet event
    /// </summary>
    [HarmonyPatch(typeof(RangedWeapon), MethodType.Constructor)]
    public static class PenetratingRounds_RangedWeapon_Constructor_Patch
    {
        public static void Postfix(RangedWeapon __instance)
        {
            if (!PenetratingRounds.Instance.IsActive)
                return;

            __instance.OnAfterAttackWithOneBullet += (attackInfo) =>
            {
                // Only process successful hits from player units
                if (!attackInfo.SuccessfulHit)
                    return;

                if (attackInfo.Attacker.Team != Enumerations.Team.Player)
                    return;

                if (attackInfo.Target is not BattleUnit targetUnit)
                    return;

                if (targetUnit.Team != Enumerations.Team.EnemyAI)
                    return;

                // Check if this was a critical hit
                if (!GunslingerCritSystem.ShouldCrit())
                    return;

                // Apply penetration damage
                PenetratingRoundsSystem.ApplyPenetrationDamage(__instance, attackInfo.Attacker, targetUnit);
            };
        }
    }

    /// <summary>
    /// Helper class to handle penetrating rounds logic
    /// </summary>
    public static class PenetratingRoundsSystem
    {
        /// <summary>
        /// Applies penetration damage to enemies on the target's tile and behind it
        /// </summary>
        public static void ApplyPenetrationDamage(RangedWeapon weapon, BattleUnit attacker, BattleUnit primaryTarget)
        {
            if (primaryTarget?.MovementManager?.CurrentTile == null)
                return;

            Tile targetTile = primaryTarget.MovementManager.CurrentTile;

            // Get all enemies to apply penetration damage to
            List<BattleUnit> penetrationTargets = FindPenetrationTargets(attacker, targetTile, primaryTarget);

            if (penetrationTargets.Count == 0)
                return;

            // Calculate non-crit damage (base weapon damage)
            float baseDamage = weapon.DamagePerBullet;

            // Apply damage to all penetration targets
            foreach (BattleUnit target in penetrationTargets)
            {
                if (target != null && target.IsAlive)
                {
                    target.Damage(baseDamage);
                    MelonLogger.Msg($"Penetrating Round! Dealt {baseDamage} damage to {target.UnitName}");
                }
            }
        }

        /// <summary>
        /// Finds all enemies on the target's tile (excluding the primary target) and the tile behind it
        /// </summary>
        private static List<BattleUnit> FindPenetrationTargets(BattleUnit attacker, Tile targetTile, BattleUnit primaryTarget)
        {
            List<BattleUnit> targets = [];

            // Get other enemies on the same tile as the primary target
            var unitsOnSameTile = targetTile.CurrentStateOfTile.GetUnitsOnTile(Enumerations.Team.EnemyAI)
                .Where(u => u != primaryTarget && u.IsAlive);
            targets.AddRange(unitsOnSameTile);

            // Get the tile behind the target
            Tile behindTile = GetTileBehind(attacker, targetTile);
            if (behindTile != null)
            {
                var unitsOnBehindTile = behindTile.CurrentStateOfTile.GetUnitsOnTile(Enumerations.Team.EnemyAI)
                    .Where(u => u.IsAlive);
                targets.AddRange(unitsOnBehindTile);
            }

            return targets;
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
    }
}
