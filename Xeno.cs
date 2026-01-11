using System;
using System.Collections.Generic;
using System.Text;
using XenopurgeRougeLike.XenoReinforcements;

namespace XenopurgeRougeLike
{
    public class Xeno
    {
        public static List<CompanyAffinity> _affinities;
        public static List<CompanyAffinity> Affinities => _affinities ??= [];

        public static Dictionary<Type, Reinforcement> Reinforcements
        {
            get
            {
                return new Dictionary<Type, Reinforcement>()
                {
                    { typeof(NeuralLinks), new NeuralLinks() }
                };
            }
        }

        public static NeuralLinks NeuralLinks => (NeuralLinks)Reinforcements[typeof(NeuralLinks)];
        public static bool IsAvailable()
        {
            return false;
        }
    }
}
