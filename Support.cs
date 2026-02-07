using System;
using System.Collections.Generic;
using XenopurgeRougeLike.SupportReinforcements;

namespace XenopurgeRougeLike
{
    public class Support
    {
        public static List<CompanyAffinity> _affinities;
        public static List<CompanyAffinity> Affinities => _affinities ??=
        [
            // 2：药剂持续时间+25%，指令商店出现药剂和回复品的概率提升
            // 4：药剂持续时间+50%，回复品效果+50%，指令商店出现药剂和回复品的概率提升，同流派增援获得概率提升
            // 6：药剂持续时间+100%，回复品效果+100%，指令商店出现药剂和回复品的概率提升，同流派增援获得概率提升，药剂和回复品使用次数+1
            SupportAffinity2.Instance,
            SupportAffinity4.Instance,
            SupportAffinity6.Instance,
        ];

        public static Dictionary<Type, Reinforcement> _reinforcements;
        public static Dictionary<Type, Reinforcement> Reinforcements => _reinforcements ??= new()
        {
            { typeof(PainRelief), PainRelief.Instance },
            { typeof(HealthStim), HealthStim.Instance },
            { typeof(CargoPants), CargoPants.Instance },
            { typeof(CardiacDefibrillator), CardiacDefibrillator.Instance },
            { typeof(TacticalHarness), TacticalHarness.Instance },
            { typeof(BiggerBackpack), BiggerBackpack.Instance },
            { typeof(FieldMedic), FieldMedic.Instance },
            { typeof(Overdose), Overdose.Instance },
            { typeof(Addict), Addict.Instance },
        };

        public static bool IsAvailable()
        {
            // Support company is always available (no special requirements)
            return true;
        }
    }
}
