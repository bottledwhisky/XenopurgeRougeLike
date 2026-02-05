using System;
using System.Collections.Generic;
using XenopurgeRougeLike.XenoReinforcements;

namespace XenopurgeRougeLike
{
    public class Xeno
    {
        public static List<CompanyAffinity> _affinities;
        public static List<CompanyAffinity> Affinities => _affinities ??=
        [
            // 2：对异形伤害+30%，控制/发狂持续时间+50%
            // 4：对异形伤害+45%，控制/发狂持续时间+75%，更高概率获得同流派增援
            // 6：对异形伤害+60%，控制/发狂持续时间+100%，更高概率获得同流派增援，异形死亡时会眩晕附近的异形
            XenoAffinity2.Instance,
            XenoAffinity4.Instance,
            XenoAffinity6.Instance,
        ];

        public static Dictionary<Type, Reinforcement> _reinforcements;
        public static Dictionary<Type, Reinforcement> Reinforcements => _reinforcements ??= new()
        {
            { typeof(SensoryAssimilation), SensoryAssimilation.Instance },
            { typeof(PsionicScream), PsionicScream.Instance },
            { typeof(ScentCamouflage), ScentCamouflage.Instance },
            { typeof(MindControl), MindControl.Instance },
            { typeof(DevourWill), DevourWill.Instance },
            { typeof(FearInstinct), FearInstinct.Instance },
            { typeof(Intimidation), Intimidation.Instance },
            { typeof(Submission), Submission.Instance },
            { typeof(Domination), Domination.Instance },
        };

        public static SensoryAssimilation SensoryAssimilation => (SensoryAssimilation)Reinforcements[typeof(SensoryAssimilation)];
        public static PsionicScream PsionicScream => (PsionicScream)Reinforcements[typeof(PsionicScream)];
        public static ScentCamouflage ScentCamouflage => (ScentCamouflage)Reinforcements[typeof(ScentCamouflage)];
        public static MindControl MindControl => (MindControl)Reinforcements[typeof(MindControl)];
        public static DevourWill DevourWill => (DevourWill)Reinforcements[typeof(DevourWill)];
        public static FearInstinct FearInstinct => (FearInstinct)Reinforcements[typeof(FearInstinct)];
        public static Intimidation Intimidation => (Intimidation)Reinforcements[typeof(Intimidation)];
        public static Submission Submission => (Submission)Reinforcements[typeof(Submission)];
        public static Domination Domination => (Domination)Reinforcements[typeof(Domination)];

        public static bool IsAvailable()
        {
            return false;
        }

        /// <summary>
        /// Gets the ControlDurationBonusLevel from the highest active XenoAffinity.
        /// Returns 0 if no XenoAffinity is active.
        /// </summary>
        public static int GetControlDurationBonusLevel()
        {
            // Check from highest to lowest affinity level
            if (XenoAffinity6.Instance.IsActive)
                return XenoAffinity6.ControlDurationBonusLevel;
            if (XenoAffinity4.Instance.IsActive)
                return XenoAffinity4.ControlDurationBonusLevel;
            if (XenoAffinity2.Instance.IsActive)
                return XenoAffinity2.ControlDurationBonusLevel;
            return 0;
        }
    }
}
