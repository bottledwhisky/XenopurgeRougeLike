using System;
using System.Collections.Generic;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.WarriorReinforcements
{
    // 勇士路径天赋6：近战伤害+6，霰弹枪每一枪投射物数量+4，血量低于50%时会缓慢回复，回复时获得等量的护甲，同流派增援获得概率提升
    // Warrior Affinity 6: Melee damage +6, shotguns fire +4 projectiles per shot, regenerate health when below 50% HP, gain armor equal to regenerated health, increased same-company reinforcement probability
    public class WarriorAffinity6 : CompanyAffinity
    {
        public const int MeleeDamageBonus = 6;
        public const int ShotgunProjectileBonus = 4;
        public const float ReinforcementChanceBonus = 2f;
        public const float RegenInterval = 5f; // 5 seconds per 1 HP (from spec: 5秒1点血量)
        public const float RegenAmount = 1f;

        public WarriorAffinity6()
        {
            unlockLevel = 6;
            company = Company.Warrior;
            description = L("warrior.affinity6.description", MeleeDamageBonus, ShotgunProjectileBonus, (int)RegenInterval, (int)ReinforcementChanceBonus);
        }

        public static WarriorAffinity6 _instance;
        public static WarriorAffinity6 Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Register melee damage boost
            WarriorAffinityHelpers.RegisterMeleeDamageBoost("WarriorAffinity6_MeleeDamage", MeleeDamageBonus);

            // Register reinforcement probability modifier
            AwardSystem.WeightModifiers.Add(ModifyWeights);
        }

        public override void OnDeactivate()
        {
            // Remove melee damage boost
            WarriorAffinityHelpers.RemoveMeleeDamageBoost("WarriorAffinity6_MeleeDamage");

            // Remove reinforcement probability modifier
            AwardSystem.WeightModifiers.Remove(ModifyWeights);
        }

        private void ModifyWeights(List<Tuple<int, Reinforcement>> choices)
        {
            WarriorAffinityHelpers.ModifyReinforcementWeights(choices, ReinforcementChanceBonus);
        }
    }
}
