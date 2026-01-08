using System;
using System.Collections.Generic;
using System.Text;
using XenopurgeRougeLike;

namespace XenopurgeRougeLike.XenoReinforcements
{
    public class NeuralLinks : Reinforcement
    {
        public NeuralLinks()
        {
            company = Company.Xeno;
            name = "Neural Links";
            description = "When a Xeno's health is reduced to 50% through a melee attack from your units, it becomes friendly.";
        }
    }
}
