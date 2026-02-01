using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace XenopurgeRougeLike
{
    using static ModLocalization;
    public enum Rarity
    {
        Standard,
        Elite,
        Expert
    }

    public struct ReinforcementPreview
    {
        public string Name;
        public string MenuItem;
        public string Description;
        public string FullString;
        public Company Company;
        public Rarity Rarity;
        public int Stacks;
    }

    public class Reinforcement : Activatable
    {
        public static Dictionary<Rarity, Color> RarityColors = new()
        {
            { Rarity.Standard, Color.white },
            { Rarity.Elite, Color.cyan },
            { Rarity.Expert, Color.yellow }
        };
        public static Dictionary<Rarity, LocalizedString> RarityNames = new()
        {
            { Rarity.Standard, L("ui.rarity.standard") },
            { Rarity.Elite, L("ui.rarity.elite") },
            { Rarity.Expert, L("ui.rarity.expert") }
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
        public static Dictionary<Rarity, int> RarityCosts = new()
        {
            { Rarity.Standard, 0 },
            { Rarity.Elite, 3 },
            { Rarity.Expert, 6 }
        };

        public Company company;
        public Rarity rarity = Rarity.Standard;
        public bool stackable = false;
        public int maxStacks = 1;
        public int currentStacks = 0;
        public LocalizedString name;
        protected LocalizedString description;
        public LocalizedString flavourText;
        public string Name
        {
            get
            {
                if (stackable)
                {
                    return $"{name} ({currentStacks}/{maxStacks})";
                }
                return name.ToString();
            }
            protected set { name = new LocalizedString(value); }
        }

        public virtual string Description
        {
            get { return description.ToString(); }
            protected set { description = new LocalizedString(value); }
        }

        public virtual string GetDescriptionForStacks(int stacks)
        {
            return Description;
        }

        public override string ToString()
        {
            return ToFullString(Name, Description);
        }

        public string ToMenuItem()
        {
            return ToMenuItem(Name);
        }

        protected string ToMenuItem(string displayName)
        {
            string colorHex = ColorUtility.ToHtmlStringRGB(company.BorderColor);
            string rarityColorHex = ColorUtility.ToHtmlStringRGB(RarityColors[rarity]);
            return $"<color=#{rarityColorHex}>{RarityNamesShort[rarity]}</color> <color=#{colorHex}>{displayName}</color>";
        }

        public string ToFullDescription()
        {
            string colorHex = ColorUtility.ToHtmlStringRGB(company.BorderColor);
            string rarityColorHex = ColorUtility.ToHtmlStringRGB(RarityColors[rarity]);
            return $@"<color=#{colorHex}>{Name}</color>
Rarity: <color=#{rarityColorHex}>{RarityNames[rarity]}</color>
Effects: {Description}
<i>{flavourText}</i>";
        }

        protected string ToFullString(string displayName, string displayDescription)
        {
            string colorHex = ColorUtility.ToHtmlStringRGB(company.BorderColor);
            string rarityColorHex = ColorUtility.ToHtmlStringRGB(RarityColors[rarity]);
            return $"<color=#{rarityColorHex}>{RarityNames[rarity]}</color> <color=#{colorHex}>{displayName}</color>: {displayDescription}";
        }

        public ReinforcementPreview GetNextLevelPreview()
        {
            int nextStacks = stackable ? Math.Min(currentStacks + 1, maxStacks) : currentStacks;
            string nextName = stackable ? $"{L(name)} ({nextStacks}/{maxStacks})" : Name;
            string nextDescription = GetDescriptionForStacks(nextStacks);

            return new ReinforcementPreview
            {
                Name = nextName,
                MenuItem = ToMenuItem(nextName),
                Description = nextDescription,
                FullString = ToFullString(nextName, nextDescription),
                Company = company,
                Rarity = rarity,
                Stacks = nextStacks,
            };
        }
    }
}
