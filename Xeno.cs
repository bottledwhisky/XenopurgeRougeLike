using System;
using System.Collections.Generic;
using System.Text;
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

        public static Dictionary<Type, Reinforcement> Reinforcements
        {
            get
            {
                return new Dictionary<Type, Reinforcement>()
                {
                    { typeof(NeuralLinks), new NeuralLinks() },
                    { typeof(SensoryAssimilation), new SensoryAssimilation() },
                    { typeof(PsionicScream), new PsionicScream() }
                };
            }
        }

        public static NeuralLinks NeuralLinks => (NeuralLinks)Reinforcements[typeof(NeuralLinks)];
        public static SensoryAssimilation SensoryAssimilation => (SensoryAssimilation)Reinforcements[typeof(SensoryAssimilation)];
        public static PsionicScream PsionicScream => (PsionicScream)Reinforcements[typeof(PsionicScream)];
        public static bool IsAvailable()
        {
            return false;
        }
    }
}
