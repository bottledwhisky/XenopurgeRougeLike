using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.Commands;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.ScavengerReinforcements
{
    // 简易护甲: After collecting an item, gain +10 armor
    public class MakeshiftArmor : Reinforcement
    {
        public const float ArmorBonus = 10f;

        public MakeshiftArmor()
        {
            company = Company.Scavenger;
            rarity = Rarity.Standard;
            name = L("scavenger.makeshift_armor.name");
            description = L("scavenger.makeshift_armor.description", (int)ArmorBonus);
            flavourText = L("scavenger.makeshift_armor.flavour");
        }

        protected static MakeshiftArmor _instance;
        public static MakeshiftArmor Instance => _instance ??= new();

        public override void OnActivate()
        {
            // Patch is applied via Harmony - no action needed here
            MelonLogger.Msg("MakeshiftArmor reinforcement activated");
        }

        public override void OnDeactivate()
        {
            // No cleanup needed
        }
    }

    // Patch CollectCommand.CollectItem to grant armor when collecting items
    [HarmonyPatch(typeof(CollectCommand), "CollectItem", MethodType.Normal)]
    public static class MakeshiftArmor_CollectItem_Patch
    {
        public static void Postfix(CollectCommand __instance)
        {
            if (!MakeshiftArmor.Instance.IsActive)
                return;

            // Get the BattleUnit that is collecting
            var battleUnitField = AccessTools.Field(typeof(CollectCommand), "_battleUnit");
            BattleUnit collectingUnit = (BattleUnit)battleUnitField.GetValue(__instance);

            if (collectingUnit != null && collectingUnit.IsAlive && collectingUnit.Team == Enumerations.Team.Player)
            {
                UnitStatsTools.AddArmorToUnit(collectingUnit, MakeshiftArmor.ArmorBonus);
                MelonLogger.Msg($"MakeshiftArmor: {collectingUnit.UnitName} gained {MakeshiftArmor.ArmorBonus} armor from collecting item. Total armor: {UnitStatsTools.ArmorField.GetValue(collectingUnit)}");
            }
        }
    }
}
