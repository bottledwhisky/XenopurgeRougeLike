using HarmonyLib;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Area;
using System.Collections.Generic;
using System.Linq;

namespace XenopurgeRougeLike.EngineerReinforcements
{
    // 塑形炸药：地雷，手雷，闪光弹友军惩罚-50%/-100%（可叠加2）
    // Shaped Charge: Mines, grenades, and flashbangs have -50%/-100% friendly fire penalty (stackable x2)
    public class ShapedCharge : Reinforcement
    {
        public static readonly float[] FriendlyFireReduction = [0.5f, 0.0f];

        public ShapedCharge()
        {
            company = Company.Engineer;
            rarity = Rarity.Standard;
            stackable = true;
            maxStacks = 2;
            name = "Shaped Charge";
            description = "Mines, grenades, and flashbangs deal {0}% reduced damage/effects to friendly units.";
            flavourText = "Precision explosives that focus the blast away from your own troops. Mostly.";
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            int reductionPercent = (int)(FriendlyFireReduction[stacks - 1] * 100);
            return string.Format(description, reductionPercent);
        }

        protected static ShapedCharge instance;
        public static ShapedCharge Instance => instance ??= new();
    }

    /// <summary>
    /// Context tracking for explosive damage sources
    /// </summary>
    public static class ShapedChargeContext
    {
        private static readonly HashSet<BattleUnit> _unitsBeingDamagedByExplosive = [];

        public static void MarkUnitsInExplosionContext(IEnumerable<BattleUnit> units)
        {
            _unitsBeingDamagedByExplosive.Clear();
            foreach (var unit in units)
            {
                _unitsBeingDamagedByExplosive.Add(unit);
            }
        }

        public static void ClearExplosionContext()
        {
            _unitsBeingDamagedByExplosive.Clear();
        }

        public static bool IsUnitInExplosionContext(BattleUnit unit)
        {
            return _unitsBeingDamagedByExplosive.Contains(unit);
        }
    }

    /// <summary>
    /// Patch to mark units affected by mine explosions
    /// </summary>
    [HarmonyPatch(typeof(Mine), "ExplodeMine")]
    public static class ShapedCharge_MarkMineExplosion_Patch
    {
        private static readonly AccessTools.FieldRef<Mine, Tile> _tileRef =
            AccessTools.FieldRefAccess<Mine, Tile>("_tile");

        public static void Prefix(Mine __instance)
        {
            if (!ShapedCharge.Instance.IsActive)
                return;

            Tile tile = _tileRef(__instance);
            if (tile?.CurrentStateOfTile == null)
                return;

            // Mark all units on the tile as being in explosion context
            ShapedChargeContext.MarkUnitsInExplosionContext(tile.CurrentStateOfTile.UnitsOnTile);
        }

        public static void Postfix()
        {
            if (!ShapedCharge.Instance.IsActive)
                return;

            // Clear the explosion context after mine explosion completes
            ShapedChargeContext.ClearExplosionContext();
        }
    }

    /// <summary>
    /// Patch to reduce damage to friendly units during explosion context
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), "Damage")]
    public static class ShapedCharge_ReduceExplosiveDamage_Patch
    {
        public static void Prefix(BattleUnit __instance, ref float damage)
        {
            if (!ShapedCharge.Instance.IsActive)
                return;

            // Only reduce damage to friendly units in explosion context
            if (ShapedChargeContext.IsUnitInExplosionContext(__instance) &&
                __instance.Team == Enumerations.Team.Player)
            {
                float reductionMultiplier = ShapedCharge.FriendlyFireReduction[ShapedCharge.Instance.currentStacks - 1];
                damage *= reductionMultiplier;
            }
        }
    }

    /// <summary>
    /// Patch to mark units affected by grenades (area damage cards)
    /// </summary>
    [HarmonyPatch(typeof(ChangeCurrentHealthArea_Card), "ApplyCommand")]
    public static class ShapedCharge_MarkGrenadeExplosion_Patch
    {
        private static readonly AccessTools.FieldRef<ChangeCurrentHealthArea_Card, float> _changeValueRef =
            AccessTools.FieldRefAccess<ChangeCurrentHealthArea_Card, float>("_changeValue");

        public static void Prefix(ChangeCurrentHealthArea_Card __instance, BattleUnit unit)
        {
            if (!ShapedCharge.Instance.IsActive)
                return;

            float changeValue = _changeValueRef(__instance);

            // Only track damage (negative health change), not healing
            if (changeValue >= 0f)
                return;

            // Mark all units on the target tile as being in explosion context
            Tile currentTile = unit.MovementManager.CurrentTile;
            if (currentTile?.CurrentStateOfTile != null)
            {
                ShapedChargeContext.MarkUnitsInExplosionContext(currentTile.CurrentStateOfTile.UnitsOnTile);
            }
        }

        public static void Postfix(ChangeCurrentHealthArea_Card __instance)
        {
            if (!ShapedCharge.Instance.IsActive)
                return;

            float changeValue = _changeValueRef(__instance);
            if (changeValue >= 0f)
                return;

            // Clear the explosion context after grenade damage completes
            ShapedChargeContext.ClearExplosionContext();
        }
    }

    /// <summary>
    /// Patch to mark units affected by flashbangs (area stat change cards)
    /// </summary>
    [HarmonyPatch(typeof(ChangeStatArea_Card), "ApplyCommand")]
    public static class ShapedCharge_MarkFlashbangEffect_Patch
    {
        private static readonly AccessTools.FieldRef<ChangeStatArea_Card, List<StatChange>> _statChangesRef =
            AccessTools.FieldRefAccess<ChangeStatArea_Card, List<StatChange>>("_statChanges");

        public static void Prefix(ChangeStatArea_Card __instance, BattleUnit unit)
        {
            if (!ShapedCharge.Instance.IsActive)
                return;

            List<StatChange> statChanges = _statChangesRef(__instance);

            // Only track debuffs (negative stat changes), not buffs
            bool hasDebuff = statChanges.Any(sc => sc.ChangeValueOfStat < 0f);
            if (!hasDebuff)
                return;

            // Mark all units on the target tile as being in explosion context
            Tile currentTile = unit.MovementManager.CurrentTile;
            if (currentTile?.CurrentStateOfTile != null)
            {
                ShapedChargeContext.MarkUnitsInExplosionContext(currentTile.CurrentStateOfTile.UnitsOnTile);
            }
        }

        public static void Postfix(ChangeStatArea_Card __instance)
        {
            if (!ShapedCharge.Instance.IsActive)
                return;

            List<StatChange> statChanges = _statChangesRef(__instance);
            bool hasDebuff = statChanges.Any(sc => sc.ChangeValueOfStat < 0f);
            if (!hasDebuff)
                return;

            // Clear the explosion context after flashbang effects complete
            ShapedChargeContext.ClearExplosionContext();
        }
    }

    /// <summary>
    /// Patch to reduce stat change effects on friendly units during explosion context
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), "ChangeStat")]
    public static class ShapedCharge_ReduceFlashbangEffect_Patch
    {
        public static void Prefix(BattleUnit __instance, ref float changeValue)
        {
            if (!ShapedCharge.Instance.IsActive)
                return;

            // Only reduce negative effects (debuffs) on friendly units in explosion context
            if (ShapedChargeContext.IsUnitInExplosionContext(__instance) &&
                __instance.Team == Enumerations.Team.Player &&
                changeValue < 0f)
            {
                float reductionMultiplier = ShapedCharge.FriendlyFireReduction[ShapedCharge.Instance.currentStacks - 1];
                changeValue *= reductionMultiplier;
            }
        }
    }
}
