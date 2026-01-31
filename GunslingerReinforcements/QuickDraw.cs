using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Weapons;
using System.Reflection;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.GunslingerReinforcements
{
    /// <summary>
    /// 快速拔枪：所有远程武器会快速射出弹匣内的所有子弹
    /// Quick Draw: All ranged weapons fire all bullets in magazine rapidly
    /// </summary>
    public class QuickDraw : Reinforcement
    {
        public const float CompressionRatio = 0.2f; // Compress firing pattern to 20% of original time

        public QuickDraw()
        {
            company = Company.Gunslinger;
            rarity = Rarity.Standard;
            stackable = false;
            name = L("gunslinger.quick_draw.name");
            flavourText = L("gunslinger.quick_draw.flavour");
            description = L("gunslinger.quick_draw.description", (int)(CompressionRatio * 100));
        }

        protected static QuickDraw _instance;
        public static QuickDraw Instance => _instance ??= new();
    }

    /// <summary>
    /// Helper class to compress firing patterns for Quick Draw reinforcement
    /// </summary>
    public static class QuickDrawHelpers
    {
        // Cached field accessor for _timeOfBurstsPercent in BurstShotsFiringModeDataSO
        private static readonly FieldInfo _timeOfBurstsPercentField =
            AccessTools.Field(typeof(BurstShotsFiringModeDataSO), "_timeOfBurstsPercent");

        // Cached field accessor for _startShootingPercent in DistributedShotsFiringModeDataSO
        private static readonly FieldInfo _startShootingPercentField =
            AccessTools.Field(typeof(DistributedShotsFiringModeDataSO), "_startShootingPercent");

        // Cached field accessor for _endShootingPercent in DistributedShotsFiringModeDataSO
        private static readonly FieldInfo _endShootingPercentField =
            AccessTools.Field(typeof(DistributedShotsFiringModeDataSO), "_endShootingPercent");

        /// <summary>
        /// Compress the firing pattern of a BurstShotsFiringModeDataSO
        /// Multiplies all TimeOfBurstsPercent values by CompressionRatio
        /// Example: [0.3, 0.8] -> [0.06, 0.16]
        /// </summary>
        public static void CompressBurstShotsFiringMode(BurstShotsFiringModeDataSO dataSO)
        {
            if (dataSO == null)
                return;

            float[] timeOfBurstsPercent = _timeOfBurstsPercentField.GetValue(dataSO) as float[];
            if (timeOfBurstsPercent == null || timeOfBurstsPercent.Length == 0)
                return;

            // Create a new compressed array
            float[] compressedTimes = new float[timeOfBurstsPercent.Length];
            for (int i = 0; i < timeOfBurstsPercent.Length; i++)
            {
                compressedTimes[i] = timeOfBurstsPercent[i] * QuickDraw.CompressionRatio;
            }

            // Set the compressed values
            _timeOfBurstsPercentField.SetValue(dataSO, compressedTimes);
        }

        /// <summary>
        /// Compress the firing pattern of a DistributedShotsFiringModeDataSO
        /// Sets StartShootingPercent to 0.0 and EndShootingPercent to CompressionRatio
        /// Example: StartShootingPercent: 0.1, EndShootingPercent: 0.9 -> StartShootingPercent: 0.0, EndShootingPercent: 0.2
        /// </summary>
        public static void CompressDistributedShotsFiringMode(DistributedShotsFiringModeDataSO dataSO)
        {
            if (dataSO == null)
                return;

            // Set start to 0.0 and end to CompressionRatio
            _startShootingPercentField.SetValue(dataSO, 0.0f);
            _endShootingPercentField.SetValue(dataSO, QuickDraw.CompressionRatio);
        }

        /// <summary>
        /// Apply Quick Draw compression to a ranged weapon's firing mode
        /// </summary>
        public static void ApplyQuickDrawToWeapon(RangedWeapon weapon)
        {
            if (weapon == null)
                return;

            // Get the firing mode data SO
            var firingModeField = AccessTools.Field(typeof(RangedWeapon), "_firingMode");
            IFiringMode firingMode = firingModeField.GetValue(weapon) as IFiringMode;

            if (firingMode == null)
                return;

            // Handle BurstShotsFiringMode
            if (firingMode is BurstShotsFiringMode burstMode)
            {
                var burstDataField = AccessTools.Field(typeof(BurstShotsFiringMode), "_burstShotsFiringModeDataSO");
                BurstShotsFiringModeDataSO dataSO = burstDataField.GetValue(burstMode) as BurstShotsFiringModeDataSO;
                CompressBurstShotsFiringMode(dataSO);
            }
            // Handle DistributedShotsFiringMode
            else if (firingMode is DistributedShotsFiringMode distributedMode)
            {
                var distributedDataField = AccessTools.Field(typeof(DistributedShotsFiringMode), "_distributedShotsFiringModeDataSO");
                DistributedShotsFiringModeDataSO dataSO = distributedDataField.GetValue(distributedMode) as DistributedShotsFiringModeDataSO;
                CompressDistributedShotsFiringMode(dataSO);
            }
        }
    }

    /// <summary>
    /// Patch to compress firing patterns when RangedWeapon is constructed
    /// </summary>
    [HarmonyPatch(typeof(RangedWeapon), MethodType.Constructor)]
    [HarmonyPatch(new[] { typeof(RangedWeaponDataSO), typeof(float), typeof(float), typeof(BattleUnit) })]
    public static class QuickDraw_RangedWeapon_Patch
    {
        public static void Postfix(RangedWeapon __instance, BattleUnit battleUnit)
        {
            // Only apply to player units
            if (battleUnit == null || battleUnit.Team != Enumerations.Team.Player)
                return;

            if (!QuickDraw.Instance.IsActive)
                return;

            QuickDrawHelpers.ApplyQuickDrawToWeapon(__instance);
        }
    }
}
