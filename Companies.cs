using Harmony;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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
        Clone
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
                        { CompanyType.Clone, Clone }
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
            Name = "Wayland-Yutani",
            ShortName = "W-Y Corp.",
            Slogan = "Building better worlds.",
            Description = "Weyland-Yutani provides various upgrades to synthetics.",
            IconPath = "wayland-yutani.png",
            Sprite = null,
            BorderColor = new Color32(80, 130, 140, 255),
        };

        private static Company _xeno;
        public static Company Xeno => _xeno ??= new()
        {
            ClassType = typeof(Xeno),
            Type = CompanyType.Xeno,
            Name = "Prometheus Institute",
            ShortName = "Prom. Inst.",
            Slogan = "To understand them, become them.",
            Description = "Prometheus Institute is a mysterious research organization specializing in Xeno biotechnology.",
            IconPath = "prometheus-institute.png",
            Sprite = null,
            BorderColor = new Color32(180, 220, 50, 255),
        };

        private static Company _rockstar;
        public static Company Rockstar => _rockstar ??= new()
        {
            ClassType = typeof(Rockstar),
            Type = CompanyType.Rockstar,
            Name = "Nova-Entertainment",
            ShortName = "Nova Ent.",
            Slogan = "Everything can be entertainment, including your death.",
            Description = "Your mission is now a galactical live TV show.",
            IconPath = "nova-entertainment.png",
            Sprite = null,
            BorderColor = new Color32(255, 80, 180, 255),
        };

        private static Company _engineer;
        public static Company Engineer => _engineer ??= new()
        {
            ClassType = typeof(Engineer),
            Type = CompanyType.Engineer,
            Name = "Sevastopol Systems",
            ShortName = "Sevastopol",
            Slogan = "Tomorrow, together.",
            Description = "Sevastopol Systems specializes in defensive technology and automated systems.",
            IconPath = "sevastopol-systems.png",
            Sprite = null,
            BorderColor = new Color32(120, 140, 160, 255),
        };

        private static Company _support;
        public static Company Support => _support ??= new()
        {
            ClassType = typeof(Support),
            Type = CompanyType.Support,
            Name = "Gateway Medical",
            ShortName = "Gateway",
            Slogan = "Your lifeline in deep space.",
            Description = "Gateway Medical provides comprehensive medical support and supply services.",
            IconPath = "gateway-medical.png",
            Sprite = null,
            BorderColor = new Color32(100, 180, 100, 255),
        };

        private static Company _warrior;
        public static Company Warrior => _warrior ??= new()
        {
            ClassType = typeof(Warrior),
            Type = CompanyType.Warrior,
            Name = "Hadley Security",
            ShortName = "Hadley Sec.",
            Slogan = "Last stand, every time.",
            Description = "Hadley Security trains elite combat specialists for hostile environments.",
            IconPath = "hadley-security.png",
            Sprite = null,
            BorderColor = new Color32(180, 60, 60, 255),
        };

        private static Company _gunslinger;
        public static Company Gunslinger => _gunslinger ??= new()
        {
            ClassType = typeof(Gunslinger),
            Type = CompanyType.Gunslinger,
            Name = "Sulaco Arms",
            ShortName = "Sulaco",
            Slogan = "Absolute firepower.",
            Description = "Sulaco Arms manufactures military-grade weaponry and tactical equipment.",
            IconPath = "sulaco-arms.png",
            Sprite = null,
            BorderColor = new Color32(200, 120, 40, 255),
        };

        private static Company _scavenger;
        public static Company Scavenger => _scavenger ??= new()
        {
            ClassType = typeof(Scavenger),
            Type = CompanyType.Scavenger,
            Name = "Torrens Salvage",
            ShortName = "Torrens",
            Slogan = "One person's trash is our treasure.",
            Description = "Torrens Salvage is a loose collective of independent scavengers and opportunists operating under a common banner.",
            IconPath = "torrens-salvage.png",
            Sprite = null,
            BorderColor = new Color32(140, 120, 80, 255),
        };

        private static Company _clone;
        public static Company Clone => _clone ??= new()
        {
            ClassType = typeof(Clone),
            Type = CompanyType.Clone,
            Name = "Acheron BioGen",
            ShortName = "Acheron",
            Slogan = "Death is just the beginning.",
            Description = "Acheron BioGen pioneers resurrection and biological replication technology.",
            IconPath = "acheron-biogen.png",
            Sprite = null,
            BorderColor = new Color32(100, 100, 140, 255),
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
        public const string AffinityHelp = "As a token of trust, each company can provide unique support to your missions based on the number of distinct reinforments you accepted from them.";
        public int unlockLevel;
        public string description;
        public Company company;

        public override string ToString()
        {
            return description;
        }

        public virtual string ToFullDescription()
        {
            return AffinityHelp + "\n" + company.ToFullDescription() + $"\nUnlocked after acquiring {unlockLevel} reinforments:\n" + description;
        }

        public virtual string ToMenuItem()
        {
            return $"{company.ShortName}({unlockLevel})";
        }
    }
}
