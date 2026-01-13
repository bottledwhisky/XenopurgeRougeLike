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

        public static Dictionary<Type, Reinforcement> Reinforcements
        {
            get
            {
                return new Dictionary<Type, Reinforcement>()
                {
                    { typeof(StarPower), new StarPower() },
                    { typeof(StreamDonations), new StreamDonations() },
                    { typeof(CelebrityAuction), new CelebrityAuction() },
                    { typeof(FandomRallies), new FandomRallies() },
                    { typeof(InTheSpotlight), new InTheSpotlight() },
                    { typeof(FanCheer), new FanCheer() },
                    { typeof(Superfan), new Superfan() },
                    { typeof(BuildingTheBrand), new BuildingTheBrand() },
                    { typeof(WhalePatron), new WhalePatron() }
                };
            }
        }

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
