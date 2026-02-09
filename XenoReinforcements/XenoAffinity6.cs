using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Area;
using System;
using System.Collections.Generic;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.XenoReinforcements
{
    public class XenoAffinity6 : CompanyAffinity
    {
        public XenoAffinity6()
        {
            unlockLevel = 6;
            company = Company.Xeno;
            description = L("xeno.affinity6.description");
        }

        // Damage bonus constants
        public const float XenoDamageBonus = 0f; // +60% damage to xenos

        public const int ControlDurationBonusLevel = 3;

        // Reinforcement chance multiplier
        public const float ReinforcementChanceBonus = 2f;

        // Stun duration when xeno dies
        public const float StunDuration = 5f; // 5 seconds stun

        public static XenoAffinity6 _instance;

        public static XenoAffinity6 Instance => _instance ??= new();

        public override void OnActivate()
        {
            AwardSystem.WeightModifiers.Add(ModifyWeights);
        }

        public override void OnDeactivate()
        {
            AwardSystem.WeightModifiers.Remove(ModifyWeights);
            CentralizedStunController.ClearStunsBySource("XenoAffinity6");
        }

        private void ModifyWeights(List<Tuple<int, Reinforcement>> choices)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                // Check if this reinforcement belongs to Xeno company
                if (choices[i].Item2.company.Type == CompanyType.Xeno)
                {
                    int newWeight = (int)(choices[i].Item1 * ReinforcementChanceBonus);
                    choices[i] = new Tuple<int, Reinforcement>(newWeight, choices[i].Item2);
                }
            }
        }
    }

    // Patch BattleUnit.Damage to increase damage dealt to xenos
    [HarmonyPatch(typeof(BattleUnit), "Damage")]
    public static class XenoAffinity6_Damage_Patch
    {
        public static void Prefix(BattleUnit __instance, ref float damage)
        {
            if (!XenoAffinity6.Instance.IsActive)
                return;

            // Check if the damaged unit is an enemy (xeno)
            if (__instance.Team != Team.EnemyAI)
                return;

            // Increase damage dealt to xenos
            damage *= (1f + XenoAffinity6.XenoDamageBonus);
        }
    }

    /// <summary>
    /// Stun system for XenoAffinity6
    /// Now uses CentralizedStunController for the actual stun management
    /// </summary>
    public static class XenoAffinity6_StunSystem
    {
        public static void StunNearbyXenos(BattleUnit deadXeno)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
                return;

            var gridManager = gameManager.GridManager;
            if (gridManager == null)
                return;

            // Get the tile where the xeno died
            var deadXenoTile = deadXeno.CurrentTile;
            if (deadXenoTile == null)
                return;

            // Get all adjacent tiles (including the current tile)
            var tilesToCheck = new List<Tile> { deadXenoTile };

            // Add adjacent tiles in all four directions
            foreach (var direction in DirectionUtilities.Directions)
            {
                var offset = direction.GetDirectionVector();
                var adjacentCoords = deadXenoTile.Coords + offset;

                if (gridManager.Tiles.TryGetValue(adjacentCoords, out var adjacentTile))
                {
                    tilesToCheck.Add(adjacentTile);
                }
            }

            // Find all enemy units on these tiles and stun them
            foreach (var tile in tilesToCheck)
            {
                var enemyUnits = tile.CurrentStateOfTile.GetUnitsOnTile(Team.EnemyAI);
                foreach (var unit in enemyUnits)
                {
                    // Don't stun the dead xeno itself
                    if (unit != deadXeno && unit.IsAlive)
                    {
                        CentralizedStunController.StunUnit(unit, XenoAffinity6.StunDuration, "XenoAffinity6");
                    }
                }
            }
        }
    }

    // Patch BattleUnit constructor to add OnDeath listener
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor, [typeof(UnitData), typeof(Enumerations.Team), typeof(GridManager)])]
    public static class XenoAffinity6_BattleUnit_Constructor_Patch
    {
        public static void Postfix(BattleUnit __instance, Team team)
        {
            if (!XenoAffinity6.Instance.IsActive)
                return;

            if (team == Team.EnemyAI)
            {
                void action()
                {
                    // When this xeno dies, stun nearby xenos
                    XenoAffinity6_StunSystem.StunNearbyXenos(__instance);
                    __instance.OnDeath -= action;
                }

                __instance.OnDeath += action;
            }
        }
    }

    // Note: Stun timing is now handled by CentralizedStunController
    // No need for individual UpdateTime patches per reinforcement
}
