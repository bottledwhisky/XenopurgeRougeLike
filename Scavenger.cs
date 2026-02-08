using System;
using System.Collections.Generic;
using XenopurgeRougeLike.ScavengerReinforcements;

namespace XenopurgeRougeLike
{
    // Scavenger path: Focus on exploration and collection mechanics
    public class Scavenger
    {
        public static List<CompanyAffinity> _affinities;
        public static List<CompanyAffinity> Affinities => _affinities ??=
        [
            // 2: 25% more collectibles spawn
            // 4: 50% more collectibles, -50% collection time, higher chance of same-company reinforcements
            // 6: 100% more collectibles, -90% collection time, higher chance of same-company reinforcements
            ScavengerAffinity2.Instance,
            ScavengerAffinity4.Instance,
            ScavengerAffinity6.Instance,
        ];

        public static Dictionary<Type, Reinforcement> _reinforcements;
        public static Dictionary<Type, Reinforcement> Reinforcements => _reinforcements ??= new()
        {
            { typeof(Loot), Loot.Instance },
            { typeof(DaggerSpecialist), DaggerSpecialist.Instance },
            { typeof(PistolSpecialist), PistolSpecialist.Instance },
            { typeof(CallDelivery), CallDelivery.Instance },
            { typeof(ShareLoot), ShareLoot.Instance },
            { typeof(CraftIED), CraftIED.Instance },
            { typeof(MakeshiftArmor), MakeshiftArmor.Instance },
            { typeof(DualPistols), DualPistols.Instance },
            { typeof(DualDaggers), DualDaggers.Instance },
        };

        public static bool IsAvailable()
        {
            // Scavenger path is always available
            return true;
        }
    }
}
