using Harmony;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike
{
    public enum CompanyType
    {
        Synthetics,
        Xeno,
        Rockstar,
        Engineer,
        Support,
        Warrior,
        Gunslinger,
        Scavenger,
        Clone,
        Common
    }

    public class Company
    {
        public Type ClassType { get; set; }
        public CompanyType Type { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Slogan { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        public Sprite Sprite { get; set; }
        public Color BorderColor { get; set; } = Color.white;
        public List<CompanyAffinity> Affinities { get; set; }
        public static Dictionary<CompanyType, Company> companies;
        public static Dictionary<CompanyType, Company> Companies
        {
            get
            {
                if (companies == null)
                {
                    companies = new Dictionary<CompanyType, Company>
                    {
                        { CompanyType.Synthetics, Synthetics },
                        { CompanyType.Xeno, Xeno },
                        { CompanyType.Rockstar, Rockstar },
                        { CompanyType.Engineer, Engineer },
                        { CompanyType.Support, Support },
                        { CompanyType.Warrior, Warrior },
                        { CompanyType.Gunslinger, Gunslinger },
                        { CompanyType.Scavenger, Scavenger },
                        { CompanyType.Clone, Clone },
                        { CompanyType.Common, Common }
                    };
                    return companies;
                }
                return companies;
            }
        }

        public static void LoadSprites()
        {
            // public Sprite LoadCustomSpriteAsset(string path)
            foreach (var company in Companies.Values)
            {
                company.Sprite = XenopurgeRougeLike.LoadCustomSpriteAsset(company.IconPath);
            }
        }

        public override string ToString()
        {
            string colorHex = ColorUtility.ToHtmlStringRGB(BorderColor);
            return $"<color=#{colorHex}>{Name}</color>: <i>{Slogan}</i> {Description}";
        }

        public string ToFullDescription()
        {
            string colorHex = ColorUtility.ToHtmlStringRGB(BorderColor);
            return $"<color=#{colorHex}>{Name}</color>\n<i>{Slogan}</i>\n{Description}";
        }

        private static Company _synthetics;
        public static Company Synthetics => _synthetics ??= new()
        {
            ClassType = typeof(Synthetics),
            Type = CompanyType.Synthetics,
            Name = L("company.synthetics.name"),
            ShortName = L("company.synthetics.short_name"),
            Slogan = L("company.synthetics.slogan"),
            Description = L("company.synthetics.description"),
            IconPath = "wayland-yutani.png",
            Sprite = null,
            BorderColor = new Color32(80, 130, 140, 255),
        };

        private static Company _xeno;
        public static Company Xeno => _xeno ??= new()
        {
            ClassType = typeof(Xeno),
            Type = CompanyType.Xeno,
            Name = L("company.xeno.name"),
            ShortName = L("company.xeno.short_name"),
            Slogan = L("company.xeno.slogan"),
            Description = L("company.xeno.description"),
            IconPath = "prometheus-institute.png",
            Sprite = null,
            BorderColor = new Color32(180, 220, 50, 255),
        };

        private static Company _rockstar;
        public static Company Rockstar => _rockstar ??= new()
        {
            ClassType = typeof(Rockstar),
            Type = CompanyType.Rockstar,
            Name = L("company.rockstar.name"),
            ShortName = L("company.rockstar.short_name"),
            Slogan = L("company.rockstar.slogan"),
            Description = L("company.rockstar.description"),
            IconPath = "nova-entertainment.png",
            Sprite = null,
            BorderColor = new Color32(255, 80, 180, 255),
        };

        private static Company _engineer;
        public static Company Engineer => _engineer ??= new()
        {
            ClassType = typeof(Engineer),
            Type = CompanyType.Engineer,
            Name = L("company.engineer.name"),
            ShortName = L("company.engineer.short_name"),
            Slogan = L("company.engineer.slogan"),
            Description = L("company.engineer.description"),
            IconPath = "sevastopol-systems.png",
            Sprite = null,
            BorderColor = new Color32(120, 140, 160, 255),
        };

        private static Company _support;
        public static Company Support => _support ??= new()
        {
            ClassType = typeof(Support),
            Type = CompanyType.Support,
            Name = L("company.support.name"),
            ShortName = L("company.support.short_name"),
            Slogan = L("company.support.slogan"),
            Description = L("company.support.description"),
            IconPath = "gateway-medical.png",
            Sprite = null,
            BorderColor = new Color32(100, 180, 100, 255),
        };

        private static Company _warrior;
        public static Company Warrior => _warrior ??= new()
        {
            ClassType = typeof(Warrior),
            Type = CompanyType.Warrior,
            Name = L("company.warrior.name"),
            ShortName = L("company.warrior.short_name"),
            Slogan = L("company.warrior.slogan"),
            Description = L("company.warrior.description"),
            IconPath = "hadley-security.png",
            Sprite = null,
            BorderColor = new Color32(180, 60, 60, 255),
        };

        private static Company _gunslinger;
        public static Company Gunslinger => _gunslinger ??= new()
        {
            ClassType = typeof(Gunslinger),
            Type = CompanyType.Gunslinger,
            Name = L("company.gunslinger.name"),
            ShortName = L("company.gunslinger.short_name"),
            Slogan = L("company.gunslinger.slogan"),
            Description = L("company.gunslinger.description"),
            IconPath = "sulaco-arms.png",
            Sprite = null,
            BorderColor = new Color32(200, 120, 40, 255),
        };

        private static Company _scavenger;
        public static Company Scavenger => _scavenger ??= new()
        {
            ClassType = typeof(Scavenger),
            Type = CompanyType.Scavenger,
            Name = L("company.scavenger.name"),
            ShortName = L("company.scavenger.short_name"),
            Slogan = L("company.scavenger.slogan"),
            Description = L("company.scavenger.description"),
            IconPath = "torrens-salvage.png",
            Sprite = null,
            BorderColor = new Color32(140, 120, 80, 255),
        };

        private static Company _clone;
        public static Company Clone => _clone ??= new()
        {
            ClassType = typeof(Clone),
            Type = CompanyType.Clone,
            Name = L("company.clone.name"),
            ShortName = L("company.clone.short_name"),
            Slogan = L("company.clone.slogan"),
            Description = L("company.clone.description"),
            IconPath = "acheron-biogen.png",
            Sprite = null,
            BorderColor = new Color32(100, 100, 140, 255),
        };

        private static Company _common;
        public static Company Common => _common ??= new()
        {
            ClassType = typeof(Common),
            Type = CompanyType.Common,
            Name = "M.A.C.E.",
            ShortName = "M.A.C.E.",
            Slogan = "Does M.A.C.E. have a slogan? Hmm thats a good question. I guess they don't",
            Description = "Mercer's Advanced Combat Enterprises. A.k.a. your employer.",
            IconPath = "mace.png",
            Sprite = null,
            BorderColor = new Color32(100, 100, 100, 255),
        };
    }

    public abstract class Activatable
    {
        protected bool isActive = false;

        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (value == isActive) return;

                isActive = value;
                if (isActive)
                    OnActivate();
                else
                    OnDeactivate();
            }
        }

        public virtual void OnActivate() { }
        public virtual void OnDeactivate() { }

        /// <summary>
        /// Override this method to save custom state to a dictionary.
        /// Return null if there is no state to save.
        /// </summary>
        public virtual Dictionary<string, object> SaveState() { return null; }

        /// <summary>
        /// Override this method to load custom state from a dictionary.
        /// This is called after the object is activated.
        /// </summary>
        public virtual void LoadState(Dictionary<string, object> state) { }
    }

    public class CompanyAffinity : Activatable
    {
        public int unlockLevel;
        public LocalizedString description;
        public Company company;

        public override string ToString()
        {
            return description;
        }

        public virtual string ToFullDescription()
        {
            return L("affinity.help") + "\n" + company.ToFullDescription() + "\n" + L("affinity.unlock_description", unlockLevel, description);
        }

        public virtual string ToMenuItem()
        {
            return $"{company.ShortName}({unlockLevel})";
        }
    }
}
