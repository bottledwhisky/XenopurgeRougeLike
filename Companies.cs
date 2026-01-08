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
        Rockstar
    }

    public class Company
    {
        public Type ClassType { get; set; }
        public CompanyType Type { get; set; }
        public string Name { get; set; }
        public string Slogan { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        public Sprite Sprite { get; set; }
        public Color BorderColor { get; set; } = Color.white;
        public Dictionary<int, CompanyAffinity> Affinities { get; set; }
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
                        { CompanyType.Rockstar, Rockstar }
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
        private static Company _synthetics;
        public static Company Synthetics => _synthetics ??= new()
        {
            ClassType = typeof(Synthetics),
            Type = CompanyType.Synthetics,
            Name = "Wayland-Yutani",
            Slogan = "Building better worlds.",
            Description = "Weyland-Yutani provides various upgrades to synthetics.",
            IconPath = "wayland-yutani.png",
            Sprite = null,
            BorderColor = new Color32(80, 130, 140, 255),
            Affinities = global::XenopurgeRougeLike.Synthetics.Affinities
        };

        private static Company _xeno;
        public static Company Xeno => _xeno ??= new()
        {
            ClassType = typeof(Xeno),
            Type = CompanyType.Xeno,
            Name = "Prometheus Institute",
            Slogan = "To understand them, become them.",
            Description = "Prometheus Institute is a mysterious research organization specializing in Xeno biotechnology.",
            IconPath = "prometheus-institute.png",
            Sprite = null,
            BorderColor = new Color32(180, 220, 50, 255),
            Affinities = global::XenopurgeRougeLike.Xeno.Affinities
        };

        private static Company _rockstar;
        public static Company Rockstar => _rockstar ??= new()
        {
            ClassType = typeof(Rockstar),
            Type = CompanyType.Rockstar,
            Name = "Nova-Entertainment",
            Slogan = "Everything can be entertainment, including your death.",
            Description = "Your mission is now a galactical live TV show.",
            IconPath = "nova-entertainment.png",
            Sprite = null,
            BorderColor = new Color32(255, 80, 180, 255),
            Affinities = global::XenopurgeRougeLike.Rockstar.Affinities
        };

        public static CompanyAffinity GetAffinity(CompanyType companyType, int level)
        {
            if (Companies.TryGetValue(companyType, out Company company))
            {
                if (company.Affinities != null && company.Affinities.TryGetValue(level, out CompanyAffinity affinity))
                {
                    return affinity;
                }
            }
            return null;
        }
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
    }

    public class CompanyAffinity : Activatable
    {
        public int unlockLevel;
        public string description;
    }
}
