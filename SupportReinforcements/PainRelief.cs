using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using static SpaceCommander.Enumerations;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SupportReinforcements
{
    /// <summary>
    /// 镇痛：使用药剂时，获得10点护甲
    /// Pain Relief: When using injections (药剂), gain 10 armor
    /// </summary>
    public class PainRelief : Reinforcement
    {
        // Armor bonus when using injection cards
        public const float ArmorBonus = 10f;

        public PainRelief()
        {
            company = Company.Support;
            rarity = Rarity.Standard;
            name = L("support.pain_relief.name");
            description = L("support.pain_relief.description", (int)ArmorBonus);
            flavourText = L("support.pain_relief.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("support.pain_relief.description", (int)ArmorBonus);
        }

        private static PainRelief _instance;
        public static PainRelief Instance => _instance ??= new();
    }

    /// <summary>
    /// Patch ChangeStat_Card.ApplyCommand to grant armor when injections are used
    /// This covers all injection cards (Brutadyne, Kinetra, Optivex)
    /// </summary>
    [HarmonyPatch(typeof(ChangeStat_Card), "ApplyCommand")]
    public static class PainRelief_InjectionUsed_Patch
    {
        public static void Postfix(ChangeStat_Card __instance, BattleUnit unit)
        {
            if (!PainRelief.Instance.IsActive)
                return;

            if (unit == null || !unit.IsAlive || unit.Team != Team.Player)
                return;

            if (__instance?.Info == null)
                return;

            // Only grant armor if this is an injection card
            if (!SupportAffinityHelpers.IsInjectionCard(__instance.Info.Id))
                return;

            UnitStatsTools.AddArmorToUnit(unit, PainRelief.ArmorBonus);
            MelonLogger.Msg($"PainRelief: Unit {unit.UnitNameNoNumber} gained {PainRelief.ArmorBonus} armor from using injection");
        }
    }
}
