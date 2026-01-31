using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Commands;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.GunslingerReinforcements
{
    /// <summary>
    /// 稳固射击：原地迎敌，压制获得+20瞄准
    /// Steady Shot: Stand and Fight and Suppressive Fire commands get +20 accuracy
    /// </summary>
    public class SteadyShot : Reinforcement
    {
        public const float AccuracyBonus = .2f; // +20 accuracy

        public SteadyShot()
        {
            company = Company.Gunslinger;
            rarity = Rarity.Standard;
            stackable = false;
            name = L("gunslinger.steady_shot.name");
            flavourText = L("gunslinger.steady_shot.flavour");
            description = L("gunslinger.steady_shot.description", (int)(AccuracyBonus * 100));
        }

        protected static SteadyShot _instance;
        public static SteadyShot Instance => _instance ??= new();
    }

    /// <summary>
    /// Patch StandAndFight command to grant bonus accuracy
    /// Patches the InitializeValues method to apply the bonus when the command is created
    /// </summary>
    [HarmonyPatch(typeof(StandAndFight), nameof(StandAndFight.InitializeValues))]
    public static class SteadyShot_StandAndFight_Patch
    {
        public static void Postfix(StandAndFight __instance, BattleUnit ___battleUnit)
        {
            if (!SteadyShot.Instance.IsActive)
                return;

            // Only apply to player units
            if (___battleUnit.Team != Enumerations.Team.Player)
                return;

            // Apply accuracy bonus via GUID-based stat change
            string guid = "SteadyShot_StandAndFight_" + ___battleUnit.UnitId;
            ___battleUnit.ChangeStat(
                Enumerations.UnitStats.Accuracy,
                SteadyShot.AccuracyBonus,
                guid
            );

            // Subscribe to command finished to remove the bonus
            void RemoveBonus()
            {
                ___battleUnit.ReverseChangeOfStat(guid);
                __instance.OnCommandFinished -= RemoveBonus;
            }

            __instance.OnCommandFinished += RemoveBonus;
        }
    }

    /// <summary>
    /// Patch SuppressiveFire command to grant bonus accuracy
    /// Patches the InitializeValues method to apply the bonus when the command is created
    /// </summary>
    [HarmonyPatch(typeof(SuppressiveFire), nameof(SuppressiveFire.InitializeValues))]
    public static class SteadyShot_SuppressiveFire_Patch
    {
        public static void Postfix(SuppressiveFire __instance, BattleUnit ___battleUnit)
        {
            if (!SteadyShot.Instance.IsActive)
                return;

            // Only apply to player units
            if (___battleUnit.Team != Enumerations.Team.Player)
                return;

            // Apply accuracy bonus via GUID-based stat change
            string guid = "SteadyShot_SuppressiveFire_" + ___battleUnit.UnitId;
            ___battleUnit.ChangeStat(
                Enumerations.UnitStats.Accuracy,
                SteadyShot.AccuracyBonus,
                guid
            );

            // Subscribe to command finished to remove the bonus
            void RemoveBonus()
            {
                ___battleUnit.ReverseChangeOfStat(guid);
                __instance.OnCommandFinished -= RemoveBonus;
            }

            __instance.OnCommandFinished += RemoveBonus;
        }
    }
}
