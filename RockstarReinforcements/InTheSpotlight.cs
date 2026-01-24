using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Abilities;
using SpaceCommander.Commands;
using SpaceCommander.EndGame;
using SpaceCommander.GameFlow;
using SpaceCommander.Weapons;
using System.Linq;
using static SpaceCommander.Enumerations;

namespace XenopurgeRougeLike.RockstarReinforcements
{
    // 聚光灯下
    // In the Spotlight
    // The first squad member becomes the "Top Star". Eliminating enemies has a higher chance to trigger Stream Donations.
    // As long as they successfully extract, the mission counts as a perfect victory.
    // "Passionate Fans" will follow and fight alongside them, locked to Run-and-Gun behavior.
    public class InTheSpotlight : Reinforcement
    {
        // Multiplier for Stream Donations chance when Top Star kills an enemy
        public static bool IsTopStarAttacking = false;
        public const float TopStarDonationMultiplier = 1.5f;

        public InTheSpotlight()
        {
            company = Company.Rockstar;
            rarity = Rarity.Elite;
            name = "In the Spotlight";
            description = "The first squad member becomes the \"Top Star\". Eliminating enemies has a higher chance to trigger Stream Donations. As long as they successfully extract, the mission counts as a perfect victory. \"Passionate Fans\" will follow and fight alongside them, locked to Run-and-Gun behavior.";
        }

        private static InTheSpotlight _instance;
        public static InTheSpotlight Instance => _instance ??= new();

        public static BattleUnit GetTopStar()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null) return null;

            var playerTeam = gameManager.GetTeamManager(Team.Player);
            if (playerTeam == null) return null;

            // Find the unit with DeploymentOrder == 1
            return playerTeam.BattleUnits.FirstOrDefault(u => u.DeploymentOrder == 1);
        }

        public static bool IsTopStar(BattleUnit unit)
        {
            return unit != null && unit.Team == Team.Player && unit.DeploymentOrder == 1;
        }

        // Check if the Top Star has extracted successfully
        public static bool HasTopStarExtracted(EndGameResultData data)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null) return false;

            var playerTeam = gameManager.GetTeamManager(Team.Player);
            if (playerTeam == null) return false;

            // Check if the Top Star is in the extracted units
            return playerTeam.ExtractedUnits.Any(u => u.DeploymentOrder == 1);
        }
    }

    [HarmonyPatch(typeof(RangedWeapon), "ShootOneBullet")]
    public class InTheSpotlight_RangedWeapon_ShootOneBullet_Patch
    {
        public static void Prefix(RangedWeapon __instance)
        {
            if (!InTheSpotlight.Instance.IsActive)
            {
                return;
            }

            var shooter = AccessTools.Field(typeof(RangedWeapon), "_battleUnit").GetValue(__instance) as BattleUnit;

            InTheSpotlight.IsTopStarAttacking = InTheSpotlight.IsTopStar(shooter);
        }

        public static void Postfix(RangedWeapon __instance)
        {
            InTheSpotlight.IsTopStarAttacking = false;
        }
    }
    

    [HarmonyPatch(typeof(Melee), "Attack")]
    public class InTheSpotlight_Melee_Attack_Patch
    {
        public static void Prefix(Melee __instance)
        {
            if (!InTheSpotlight.Instance.IsActive)
            {
                return;
            }

            var shooter = AccessTools.Field(typeof(Melee), "_battleUnit").GetValue(__instance) as BattleUnit;

            InTheSpotlight.IsTopStarAttacking = InTheSpotlight.IsTopStar(shooter);
        }

        public static void Postfix(Melee __instance)
        {
            InTheSpotlight.IsTopStarAttacking = false;
        }
    }
}
