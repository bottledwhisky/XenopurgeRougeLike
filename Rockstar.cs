using System;
using System.Collections.Generic;
using XenopurgeRougeLike.RockstarReinforcements;

namespace XenopurgeRougeLike
{
    public class Rockstar
    {
        //Rockstar / Rock Star

        //流派被动(Genre Passive)
        //等级英文名效果翻译2-A "Passionate Fan" is automatically deployed at the start of battle and will find their own fun.Unlock Fan Count; gain 1k-2k fans after each battle.4-"Passionate Fan" combat AI upgrades to enhanced mode.Gain 2k-3k fans after each battle.6-"Passionate Fan" stats slightly increase.Gain 3k-4k fans after each battle.

        //普通 (Common)
        //中文名英文名效果翻译明星效应Star PowerGain +50% fans on perfect victory. Every 1,000 fans grants the first squad member one random stat: +5 HP, +5 Aim, +1 Speed, or +1 Melee Damage.明星效应IIStar Power IIUncollected digital collectibles no longer count against perfect victory.Gain 1 point after battle for every 1,000 fans.直播打赏Stream DonationsWhen eliminating enemies in battle, randomly gain consumable charges based on fan count.直播打赏IIStream Donations IIIncreases the chance of receiving donations.名人拍卖Celebrity AuctionYou can sell equipment.Sale price increases by 1 for each battle the equipment has been through.

        //精锐 (Elite)
        //中文名英文名效果翻译饭圈出征Fandom RalliesWhen a "Passionate Fan" dies, another "Passionate Fan" joins the battlefield.聚光灯下In the SpotlightThe first squad member becomes the "Top Star". Eliminating enemies has a higher chance to trigger Stream Donations. As long as they successfully extract, the mission counts as a perfect victory. "Passionate Fans" will follow and fight alongside them, locked to Run-and-Gun behavior.应援Fan CheerThe first squad member becomes the "Top Star". When the "Top Star" takes damage, "Passionate Fan" stats greatly increase (non-stackable). Effect duration +1 second for every 1,000 fans.

        //专家(Expert)
        //中文名英文名效果翻译粉丝头目SuperfanAn additional "Passionate Fan" is automatically deployed at the start of battle.构造人设Building the BrandDouble the amount of fans gained.榜一大哥Whale PatronAfter battle, gain a piece of equipment you don't have. If everyone already has a melee weapon, ranged weapon, and equipment, randomly sell a lower base-price item and gain one with a higher price.

        //其他术语
        //中文英文热情的粉丝Passionate Fan一哥Top Star粉丝数Fan Count完美胜利Perfect Victory数字收藏Digital Collectible

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
                    { typeof(FanCheer), new FanCheer() }
                };
            }
        }

        public static StarPower StarPower => (StarPower)Reinforcements[typeof(StarPower)];
        public static StreamDonations StreamDonations => (StreamDonations)Reinforcements[typeof(StreamDonations)];
        public static CelebrityAuction CelebrityAuction => (CelebrityAuction)Reinforcements[typeof(CelebrityAuction)];
        public static FandomRallies FandomRallies => (FandomRallies)Reinforcements[typeof(FandomRallies)];
        public static InTheSpotlight InTheSpotlight => (InTheSpotlight)Reinforcements[typeof(InTheSpotlight)];
        public static FanCheer FanCheer => (FanCheer)Reinforcements[typeof(FanCheer)];

        public static bool IsAvailable()
        {
            return true;
        }
    }
}
