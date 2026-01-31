using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.Abilities;
using SpaceCommander.Commands;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.WarriorReinforcements
{
    /// <summary>
    /// 嗜血：血量低于50%时，近战击杀回复10血量
    /// Bloodlust: When HP is below 50%, melee kills restore 10 HP
    /// </summary>
    public class Bloodlust : Reinforcement
    {
        public const float HealAmount = 10f;
        public const float HealthThreshold = 0.5f; // 50% health

        public Bloodlust()
        {
            company = Company.Warrior;
            rarity = Rarity.Expert;
            stackable = false;
            name = L("warrior.bloodlust.name");
            flavourText = L("warrior.bloodlust.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("warrior.bloodlust.description", (int)(HealthThreshold * 100), (int)HealAmount);
        }

        protected static Bloodlust _instance;
        public static Bloodlust Instance => _instance ??= new();
    }

    /// <summary>
    /// Patch the Melee.Attack() method to detect melee kills
    /// When a low-health player unit kills an enemy with melee, restore HP
    /// </summary>
    [HarmonyPatch(typeof(Melee), "Attack")]
    public static class Bloodlust_Melee_Attack_Patch
    {
        public static void Postfix(Melee __instance, BattleUnit ____battleUnit, IDamagable ____target)
        {
            if (!Bloodlust.Instance.IsActive)
                return;

            // Only apply to player units
            if (____battleUnit == null || ____battleUnit.Team != Enumerations.Team.Player)
                return;

            // Check if the target was killed by this attack
            if (____target == null || ____target.IsAlive)
                return;

            // Check if the attacker's health is below threshold
            float healthPercentage = ____battleUnit.CurrentHealth / ____battleUnit.CurrentMaxHealth;
            if (healthPercentage >= Bloodlust.HealthThreshold)
                return;

            // Heal the attacker
            UnitStatsTools.HealUnit(____battleUnit, Bloodlust.HealAmount);

            MelonLogger.Msg($"Bloodlust: {____battleUnit.UnitNameNoNumber} restored {Bloodlust.HealAmount} HP from melee kill (HP: {____battleUnit.CurrentHealth}/{____battleUnit.CurrentMaxHealth})");
        }
    }
}
