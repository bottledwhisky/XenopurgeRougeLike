using HarmonyLib;
using SpaceCommander.ActionCards;
using System;
using XenopurgeRougeLike.EngineerReinforcements;

namespace XenopurgeRougeLike.EngineerReinforcements
{
    // 战术腰带：地雷，手雷，闪光弹使用次数+1
    public class TacticalBelt : Reinforcement
    {
        public static readonly int BonusUses = 1;

        public TacticalBelt()
        {
            company = Company.Engineer;
            rarity = Rarity.Elite;
            stackable = false;
            maxStacks = 1;
            name = "Tactical Belt";
            description = "Mines, grenades, and flashbangs have +1 use.";
            flavourText = "Advanced load-bearing equipment with reinforced pouches for extra ordnance capacity.";
        }

        protected static TacticalBelt instance;
        public static TacticalBelt Instance => instance ??= new();
    }

    /// <summary>
    /// Patch ActionCard constructor to add bonus uses for engineer cards when TacticalBelt is active
    /// Shared logic with EngineerAffinity6
    /// </summary>
    [HarmonyPatch(typeof(ActionCard), MethodType.Constructor)]
    public static class TacticalBelt_ActionCard_Constructor_Patch
    {
        public static void Postfix(ActionCard __instance)
        {
            // Check if the card is an Engineer card (grenade, mine, or flashbang)
            if (__instance?.Info == null)
                return;

            string cardId = __instance.Info.Id;

            // Only boost uses for Engineer action cards
            if (!EngineerAffinity2.EngineerActionCards.Contains(cardId))
                return;

            int bonusUses = 0;

            // Add bonus from TacticalBelt reinforcement
            if (TacticalBelt.Instance.IsActive)
            {
                bonusUses += TacticalBelt.BonusUses;
            }

            // Add bonus from EngineerAffinity6
            if (EngineerAffinity6.Instance.IsActive)
            {
                bonusUses += EngineerAffinity6.BonusUses;
            }

            // Apply bonus uses if any
            if (bonusUses > 0)
            {
                // Get current uses
                int currentUses = __instance.UsesLeft;

                // Add bonus uses (only if card has limited uses)
                if (currentUses > 0)
                {
                    int newUses = currentUses + bonusUses;
                    AccessTools.Field(typeof(ActionCard), "_usesLeft").SetValue(__instance, newUses);
                }
            }
        }
    }
}
