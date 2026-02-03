using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Weapons;
using System.Reflection;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.WarriorReinforcements
{
    // 霰弹近射：对距离1格的敌人使用霰弹枪射击时，此次开火+50瞄准
    // Shotgun Close Range: When shooting with a shotgun at enemies 1 tile away, +50 accuracy for this shot
    public class ShotgunCloseRange : Reinforcement
    {
        public const float AccuracyBonus = 0.50f; // 50% accuracy bonus
        public const int TriggerDistance = 1; // Trigger at 1 tile distance

        public ShotgunCloseRange()
        {
            company = Company.Warrior;
            rarity = Rarity.Standard;
            stackable = false;
            name = L("warrior.shotgun_close_range.name");
            description = L("warrior.shotgun_close_range.description", (int)(AccuracyBonus * 100), TriggerDistance);
            flavourText = L("warrior.shotgun_close_range.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("warrior.shotgun_close_range.description", (int)(AccuracyBonus * 100), TriggerDistance);
        }

        public override void OnActivate()
        {
            // Patch is automatically applied through Harmony
        }

        public override void OnDeactivate()
        {
            // Patch is automatically disabled when reinforcement is inactive
        }

        protected static ShotgunCloseRange instance;
        public static ShotgunCloseRange Instance => instance ??= new();
    }

    /// <summary>
    /// Patch to give accuracy bonus when shooting shotguns at close range (1 tile)
    /// </summary>
    [HarmonyPatch(typeof(RangedWeapon), "ShootOneBullet")]
    public static class ShotgunCloseRange_ShootOneBullet_Patch
    {
        // Cached field accessors
        private static readonly FieldInfo _battleUnitField = AccessTools.Field(typeof(RangedWeapon), "_battleUnit");
        private static readonly FieldInfo _distanceField = AccessTools.Field(typeof(RangedWeapon), "_distance");
        private static readonly FieldInfo _accuracyField = AccessTools.Field(typeof(RangedWeapon), "_accuracy");
        private static readonly FieldInfo _firingModeField = AccessTools.Field(typeof(RangedWeapon), "_firingMode");

        // Store original accuracy to restore in Postfix
        private static float _originalAccuracy = 0f;
        private static bool _bonusApplied = false;

        public static void Prefix(RangedWeapon __instance)
        {
            // Reset state
            _bonusApplied = false;

            if (!ShotgunCloseRange.Instance.IsActive)
                return;

            // Get the shooter
            BattleUnit shooter = _battleUnitField.GetValue(__instance) as BattleUnit;
            if (shooter == null || shooter.Team != Enumerations.Team.Player)
                return;

            // Get the firing mode and check if it's a shotgun
            IFiringMode firingMode = _firingModeField.GetValue(__instance) as IFiringMode;
            if (!WarriorAffinityHelpers.IsShotgun(firingMode))
                return;

            // Get the distance to target
            int distance = (int)_distanceField.GetValue(__instance);
            if (distance != ShotgunCloseRange.TriggerDistance)
                return;

            // Apply accuracy bonus
            _originalAccuracy = (float)_accuracyField.GetValue(__instance);
            float newAccuracy = _originalAccuracy + ShotgunCloseRange.AccuracyBonus;
            _accuracyField.SetValue(__instance, newAccuracy);
            _bonusApplied = true;
        }

        public static void Postfix(RangedWeapon __instance)
        {
            // Restore original accuracy if bonus was applied
            if (_bonusApplied)
            {
                _accuracyField.SetValue(__instance, _originalAccuracy);
                _bonusApplied = false;
            }
        }
    }
}
