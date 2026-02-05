using System;
using System.Collections.Generic;
using XenopurgeRougeLike.CommonReinforcements;

namespace XenopurgeRougeLike
{
    /// <summary>
    /// Common company (M.A.C.E. - Mercer's Advanced Combat Enterprises)
    /// Represents general-purpose reinforcements not tied to a specific path.
    /// </summary>
    public class Common
    {
        public static List<CompanyAffinity> _affinities;
        public static List<CompanyAffinity> Affinities => _affinities ??=
        [
            // No affinities for Common path - only Standard quality reinforcements
        ];

        public static Dictionary<Type, Reinforcement> _reinforcements;
        public static Dictionary<Type, Reinforcement> Reinforcements => _reinforcements ??= new()
        {
            { typeof(MarksmanTraining), MarksmanTraining.Instance },
            { typeof(EnduranceTraining), EnduranceTraining.Instance },
            { typeof(StrengthTraining), StrengthTraining.Instance },
            { typeof(Sponsorship), Sponsorship.Instance },
            { typeof(Reconsider), Reconsider.Instance },
            { typeof(Solidarity), Solidarity.Instance },
            // Weapon Specialists
            { typeof(DaggerSpecialist), DaggerSpecialist.Instance },
            { typeof(BluntWeaponSpecialist), BluntWeaponSpecialist.Instance },
            { typeof(BladeSpecialist), BladeSpecialist.Instance },
            { typeof(PistolSpecialist), PistolSpecialist.Instance },
            { typeof(ShotgunSpecialist), ShotgunSpecialist.Instance },
            { typeof(RifleSpecialist), RifleSpecialist.Instance },
            { typeof(SniperRifleSpecialist), SniperRifleSpecialist.Instance },
        };

        public static bool IsAvailable()
        {
            return false;
        }
    }
}
