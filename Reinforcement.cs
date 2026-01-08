using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace XenopurgeRougeLike
{
    public enum Rarity
    {
        Standard,
        Elite,
        Expert
    }

    public class Reinforcement: Activatable
    {
        public static Dictionary<Rarity, Color> RarityColors = new()
        {
            { Rarity.Standard, Color.white },
            { Rarity.Elite, Color.cyan },
            { Rarity.Expert, Color.yellow }
        };
        public static Dictionary<Rarity, string> RarityNames = new()
        {
            { Rarity.Standard, "Standard" },
            { Rarity.Elite, "Elite" },
            { Rarity.Expert, "Expert" }
        };
        public static Dictionary<Rarity, string> RarityNamesShort = new()
        {
            { Rarity.Standard, "☆" },
            { Rarity.Elite, "★" },
            { Rarity.Expert, "★" }
        };
        public static Dictionary<Rarity, int> RarityWeights = new()
        {
            { Rarity.Standard, 60 },
            { Rarity.Elite, 30 },
            { Rarity.Expert, 10 }
        };

        public Company company;
        public Rarity rarity = Rarity.Standard;
        public bool stackable = false;
        public int maxStacks = 1;
        public int currentStacks = 0;
        public string name;
        protected string description;
        public string flavourText;
        public string Name
        {
            get
            {
                if (stackable)
                {
                    return $"{name} ({currentStacks}/{maxStacks})";
                }
                return name;
            }
            protected set { name = value; }
        }

        public virtual string Description
        {
            get { return description; }
            protected set { description = value; }
        }

        public override string ToString()
        {
            string colorHex = ColorUtility.ToHtmlStringRGB(company.BorderColor);
            string rarityColorHex = ColorUtility.ToHtmlStringRGB(RarityColors[rarity]);
            return $"<color=#{rarityColorHex}>{RarityNames[rarity]}</color> <color=#{colorHex}>{Name}</color>: {Description}";
        }

        public string ToMenuItem()
        {
            string colorHex = ColorUtility.ToHtmlStringRGB(company.BorderColor);
            string rarityColorHex = ColorUtility.ToHtmlStringRGB(RarityColors[rarity]);
            return $"<color=#{rarityColorHex}>{RarityNamesShort[rarity]}</color> <color=#{colorHex}>{Name}</color>";
        }
        public string ToFullDescription()
        {
            string colorHex = ColorUtility.ToHtmlStringRGB(company.BorderColor);
            string rarityColorHex = ColorUtility.ToHtmlStringRGB(RarityColors[rarity]);
            return $@"<color=#{colorHex}>{Name}</color>
{company}
Rarity: <color=#{rarityColorHex}>{RarityNames[rarity]}</color>
Effects: {Description}
<i>{flavourText}</i>";
        }

        public Reinforcement NextLevel()
        {
            if (stackable && currentStacks < maxStacks)
            {
                return new()
                {
                    company = company,
                    rarity = rarity,
                    stackable = stackable,
                    maxStacks = maxStacks,
                    currentStacks = currentStacks + 1,
                    name = name,
                    description = description,
                    flavourText = flavourText,
                };
            }
            return this;
        }
    }
}
