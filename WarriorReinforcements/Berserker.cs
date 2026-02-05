using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Area;
using System.Collections.Generic;
using TimeSystem;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.WarriorReinforcements
{
    /// <summary>
    /// 狂战士：获得"狂战士"指令，让一名队员进入狂暴，+10近战伤害，+5速度，持续30秒。期间其无法接受指令。
    /// Berserker: Gain "Berserker" command, allows a soldier to enter rage mode, +10 melee damage, +5 speed, lasts 30 seconds. During this time they cannot receive commands.
    /// </summary>
    public class Berserker : Reinforcement
    {
        public static readonly int[] MeleeDamageBonus = [10, 15];
        public static readonly float[] SpeedBonus = [5f, 7.5f];
        public static readonly float Duration = 30f; // seconds

        public Berserker()
        {
            company = Company.Warrior;
            rarity = Rarity.Elite;
            stackable = true;
            maxStacks = 2;
            name = L("warrior.berserker.name");
            flavourText = L("warrior.berserker.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            return L("warrior.berserker.description", MeleeDamageBonus[stacks - 1], (int)SpeedBonus[stacks - 1], (int)Duration);
        }

        protected static Berserker instance;
        public static Berserker Instance => instance ??= new();

        public static HashSet<BattleUnit> BerserkedUnits = [];
        public static Dictionary<BattleUnit, float> BerserkTimers = new();
    }

    // Patch to clear state when mission starts
    [HarmonyPatch(typeof(TestGame), "StartGame")]
    public class Berserker_TestGame_StartGame_Patch
    {
        public static void Postfix()
        {
            Berserker.BerserkedUnits.Clear();
            Berserker.BerserkTimers.Clear();
        }
    }

    /// <summary>
    /// Patch to inject BerserkerActionCard into InBattleActionCardsManager after initialization
    /// </summary>
    [HarmonyPatch(typeof(InBattleActionCardsManager), "Initialize")]
    public static class Berserker_InjectActionCard_Patch
    {
        public static void Postfix(InBattleActionCardsManager __instance)
        {
            if (!Berserker.Instance.IsActive)
                return;

            var actionCardInfo = new BerserkerActionCardInfo();
            actionCardInfo.SetId("Berserker");

            var berserkerCard = new BerserkerActionCard(actionCardInfo);

            __instance.InBattleActionCards.Add(berserkerCard);
        }
    }

    /// <summary>
    /// Berserker action card - puts a unit into rage mode
    /// Implements IUnitTargetable to target player units
    /// </summary>
    public class BerserkerActionCard : ActionCard, IUnitTargetable
    {
        public Enumerations.Team TeamToAffect => Enumerations.Team.Player;

        public BerserkerActionCard(ActionCardInfo actionCardInfo)
        {
            Info = actionCardInfo;
            // Set uses to 1 (one-time use per mission)
            _usesLeft = 1;
        }

        public override ActionCard GetCopy()
        {
            return new BerserkerActionCard(Info);
        }

        public void ApplyCommand(BattleUnit unit)
        {
            if (!Berserker.Instance.IsActive)
                return;

            if (unit == null || !unit.IsAlive || unit.Team != Enumerations.Team.Player)
                return;

            // Don't allow berserking already berserked units
            if (Berserker.BerserkedUnits.Contains(unit))
                return;

            ApplyBerserkMode(unit);
        }

        private void ApplyBerserkMode(BattleUnit unit)
        {
            Berserker.BerserkedUnits.Add(unit);
            Berserker.BerserkTimers[unit] = Berserker.Duration;

            // Apply stat buffs
            int stackLevel = Berserker.Instance.currentStacks - 1;
            unit.ChangeStat(Enumerations.UnitStats.Power, Berserker.MeleeDamageBonus[stackLevel], "Berserker_MeleeDamage");
            unit.ChangeStat(Enumerations.UnitStats.Speed, Berserker.SpeedBonus[stackLevel], "Berserker_Speed");

            MelonLogger.Msg($"Berserker: {unit.UnitNameNoNumber} entered rage mode!");

            // Stop current commands
            unit.StopCommandsExecution();
        }

        IEnumerable<CommandsAvailabilityChecker.UnitAnavailableReasons> IUnitTargetable.IsUnitValid(BattleUnit unit)
        {
            var reasons = new List<CommandsAvailabilityChecker.UnitAnavailableReasons>();

            if (!Berserker.Instance.IsActive)
            {
                return reasons;
            }

            // Can only target alive units
            if (!unit.IsAlive)
            {
                reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.UnitIsDead);
            }

            // Can't berserk already berserked units
            if (Berserker.BerserkedUnits.Contains(unit))
            {
                reasons.Add(CommandsAvailabilityChecker.UnitAnavailableReasons.AlreadyHasLogic);
            }

            return reasons;
        }
    }

    /// <summary>
    /// Custom ActionCardInfo for Berserker
    /// </summary>
    public class BerserkerActionCardInfo : ActionCardInfo
    {
        public string CustomCardName => L("warrior.berserker.card_name");

        public string CustomCardDescription => L("warrior.berserker.card_description");

        public BerserkerActionCardInfo()
        {
            AccessTools.Field(typeof(ActionCardInfo), "_uses").SetValue(this, 1);
            AccessTools.Field(typeof(ActionCardInfo), "canNotBeReplenished").SetValue(this, false);
        }
    }

    /// <summary>
    /// Patch to intercept CardName getter for BerserkerActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardName", MethodType.Getter)]
    public static class BerserkerActionCardInfo_CardName_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is BerserkerActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardName;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to intercept CardDescription getter for BerserkerActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardDescription", MethodType.Getter)]
    public static class BerserkerActionCardInfo_CardDescription_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is BerserkerActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardDescription;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to subscribe to OnDeath event for player units
    /// </summary>
    [HarmonyPatch(typeof(BattleUnit), MethodType.Constructor, [typeof(UnitData), typeof(Enumerations.Team), typeof(GridManager)])]
    public class Berserker_BattleUnit_Constructor_Patch
    {
        public static void OnUpdate(BattleUnit __instance, float deltaTime)
        {
            if (!Berserker.Instance.IsActive)
                return;

            if (!Berserker.BerserkTimers.ContainsKey(__instance))
                return;

            // Decrease timer
            Berserker.BerserkTimers[__instance] -= deltaTime;

            // Check if berserk expired
            if (Berserker.BerserkTimers[__instance] <= 0)
            {
                RemoveBerserkMode(__instance);
            }
        }

        private static void RemoveBerserkMode(BattleUnit unit)
        {
            Berserker.BerserkedUnits.Remove(unit);
            Berserker.BerserkTimers.Remove(unit);

            // Remove stat buffs
            unit.ReverseChangeOfStat("Berserker_MeleeDamage");
            unit.ReverseChangeOfStat("Berserker_Speed");

            // Restart commands execution
            unit.StartCommandsExecution();

            MelonLogger.Msg($"Berserker: {unit.UnitNameNoNumber} rage mode ended.");
        }

        public static void Postfix(BattleUnit __instance, Enumerations.Team team)
        {
            if (!Berserker.Instance.IsActive)
                return;

            if (team == Enumerations.Team.Player)
            {
                void onUpdateAction(float deltaTime)
                {
                    OnUpdate(__instance, deltaTime);
                }
                TempSingleton<TimeManager>.Instance.OnTimeUpdated += onUpdateAction;

                void action()
                {
                    if (Berserker.BerserkedUnits.Contains(__instance))
                    {
                        Berserker.BerserkedUnits.Remove(__instance);
                        Berserker.BerserkTimers.Remove(__instance);
                    }
                    __instance.OnDeath -= action;
                    TempSingleton<TimeManager>.Instance.OnTimeUpdated -= onUpdateAction;
                }

                __instance.OnDeath += action;
            }
        }
    }
}
