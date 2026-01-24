using System;
using System.Collections.Generic;
using XenopurgeRougeLike.RockstarReinforcements;

namespace XenopurgeRougeLike
{
    public class Rockstar
    {
        public static List<CompanyAffinity> _affinities;
        public static List<CompanyAffinity> Affinities => _affinities ??= [
            RockstarAffinity2.Instance,
            RockstarAffinity4.Instance,
            RockstarAffinity6.Instance,
        ];

        public static Dictionary<Type, Reinforcement> _reinforcements;
        public static Dictionary<Type, Reinforcement> Reinforcements => _reinforcements ??= new()
        {
            { typeof(StarPower), StarPower.Instance },
            { typeof(StreamDonations), StreamDonations.Instance },
            { typeof(CelebrityAuction), CelebrityAuction.Instance },
            { typeof(FandomRallies), FandomRallies.Instance },
            { typeof(InTheSpotlight), InTheSpotlight.Instance },
            { typeof(FanCheer), FanCheer.Instance },
            { typeof(Superfan), Superfan.Instance },
            { typeof(BuildingTheBrand), BuildingTheBrand.Instance },
            { typeof(WhalePatron), WhalePatron.Instance }
        };

        public static StarPower StarPower => (StarPower)Reinforcements[typeof(StarPower)];
        public static StreamDonations StreamDonations => (StreamDonations)Reinforcements[typeof(StreamDonations)];
        public static CelebrityAuction CelebrityAuction => (CelebrityAuction)Reinforcements[typeof(CelebrityAuction)];
        public static FandomRallies FandomRallies => (FandomRallies)Reinforcements[typeof(FandomRallies)];
        public static InTheSpotlight InTheSpotlight => (InTheSpotlight)Reinforcements[typeof(InTheSpotlight)];
        public static FanCheer FanCheer => (FanCheer)Reinforcements[typeof(FanCheer)];
        public static Superfan Superfan => (Superfan)Reinforcements[typeof(Superfan)];
        public static BuildingTheBrand BuildingTheBrand => (BuildingTheBrand)Reinforcements[typeof(BuildingTheBrand)];
        public static WhalePatron WhalePatron => (WhalePatron)Reinforcements[typeof(WhalePatron)];

        public static bool IsAvailable()
        {
            return true;
        }
    }
}
