using HarmonyLib;
using SpaceCommander;
using SpaceCommander.ActionCards;
using System.Collections.Generic;
using System.Linq;
using static SpaceCommander.Enumerations;

namespace XenopurgeRougeLike.XenoReinforcements
{
    /// <summary>
    /// 灵能尖啸：眩晕所有异形5秒，可使用1/2次
    /// Psionic Scream: Stun all xenos for 5 seconds, usable 1/2 times
    /// </summary>
    public class PsionicScream : Reinforcement
    {
        public static readonly int[] UsesPerStack = [1, 2];
        public const float StunDuration = 5f;

        public PsionicScream()
        {
            company = Company.Xeno;
            stackable = true;
            maxStacks = 2;
            name = "Psionic Scream";
            description = "Stun all xenos for {0} seconds. Usable {1} time(s) per mission.";
            flavourText = "A psychic shriek that reverberates through the hive mind, temporarily paralyzing all xenos in range.";
        }

        public override string Description
        {
            get { return string.Format(description, StunDuration, UsesPerStack[currentStacks - 1]); }
        }

        public static PsionicScream Instance => (PsionicScream)Xeno.Reinforcements[typeof(PsionicScream)];
    }

    /// <summary>
    /// Patch to inject PsionicScreamActionCard into InBattleActionCardsManager after initialization
    /// </summary>
    [HarmonyPatch(typeof(InBattleActionCardsManager), "Initialize")]
    public static class PsionicScream_InjectActionCard_Patch
    {
        public static void Postfix(InBattleActionCardsManager __instance)
        {
            if (!PsionicScream.Instance.IsActive)
                return;

            // Create custom ActionCardInfo
            var actionCardInfo = new PsionicScreamActionCardInfo();
            actionCardInfo.SetId("PsionicScream");

            // Create and add the PsionicScreamActionCard instance
            var screamCard = new PsionicScreamActionCard(actionCardInfo);

            // Add to the InBattleActionCards list
            __instance.InBattleActionCards.Add(screamCard);
        }
    }

    /// <summary>
    /// Psionic Scream action card - stuns all xenos on the map for 5 seconds.
    /// Implements INoTargetable as it doesn't require targeting a specific unit.
    /// </summary>
    public class PsionicScreamActionCard : ActionCard, INoTargetable
    {
        public PsionicScreamActionCard(ActionCardInfo actionCardInfo)
        {
            Info = actionCardInfo;
            // Set uses based on current stacks
            _usesLeft = PsionicScream.UsesPerStack[PsionicScream.Instance.currentStacks - 1];
        }

        public override ActionCard GetCopy()
        {
            return new PsionicScreamActionCard(Info);
        }

        public void ApplyCommand()
        {
            if (!PsionicScream.Instance.IsActive)
                return;

            var gameManager = GameManager.Instance;
            if (gameManager == null)
                return;

            // Find all enemy units on the battlefield
            var allUnits = gameManager.AllBattleUnits;
            if (allUnits == null)
                return;

            var enemies = allUnits.Where(u => u.Team == Team.EnemyAI && u.IsAlive).ToList();

            // Stun all enemies using the existing stun system from XenoAffinity6
            foreach (var enemy in enemies)
            {
                PsionicScream_StunSystem.StunUnit(enemy, PsionicScream.StunDuration);
            }
        }

        IEnumerable<CommandsAvailabilityChecker.CardUnavailableReason> INoTargetable.IsCardValid()
        {
            var reasons = new List<CommandsAvailabilityChecker.CardUnavailableReason>();

            // Only available if PsionicScream reinforcement is active
            if (!PsionicScream.Instance.IsActive)
            {
                return reasons;
            }

            // Check if there are any enemies to stun
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                var allUnits = gameManager.AllBattleUnits;
                if (allUnits == null || !allUnits.Any(u => u.Team == Team.EnemyAI && u.IsAlive))
                {
                    reasons.Add(CommandsAvailabilityChecker.CardUnavailableReason.NoEnemiesToTarget);
                }
            }

            return reasons;
        }
    }

    /// <summary>
    /// Custom ActionCardInfo for PsionicScream
    /// </summary>
    public class PsionicScreamActionCardInfo : ActionCardInfo
    {
        public string CustomCardName => "Psionic Scream";

        public string CustomCardDescription =>
            $"Stun all xenos for {PsionicScream.StunDuration} seconds.";
    }

    /// <summary>
    /// Patch to intercept CardName getter for PsionicScreamActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardName", MethodType.Getter)]
    public static class PsionicScreamActionCardInfo_CardName_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is PsionicScreamActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardName;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to intercept CardDescription getter for PsionicScreamActionCardInfo
    /// </summary>
    [HarmonyPatch(typeof(ActionCardInfo), "CardDescription", MethodType.Getter)]
    public static class PsionicScreamActionCardInfo_CardDescription_Patch
    {
        public static bool Prefix(ActionCardInfo __instance, ref string __result)
        {
            if (__instance is PsionicScreamActionCardInfo customInfo)
            {
                __result = customInfo.CustomCardDescription;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Stun system for PsionicScream - reuses the stun logic pattern from XenoAffinity6
    /// </summary>
    public static class PsionicScream_StunSystem
    {
        private static Dictionary<BattleUnit, float> _stunnedEnemies = new Dictionary<BattleUnit, float>();

        public static void StunUnit(BattleUnit unit, float duration)
        {
            if (unit == null || !unit.IsAlive || unit.Team != Team.EnemyAI)
                return;

            // If already stunned, extend the duration if the new one is longer
            if (_stunnedEnemies.TryGetValue(unit, out var existingDuration))
            {
                if (duration > existingDuration)
                {
                    _stunnedEnemies[unit] = duration;
                }
            }
            else
            {
                _stunnedEnemies[unit] = duration;
            }
        }

        public static bool IsStunned(BattleUnit unit)
        {
            return _stunnedEnemies.ContainsKey(unit) && unit.Team == Team.EnemyAI;
        }

        public static float UpdateStunTimers(BattleUnit unit, float deltaTime)
        {
            if (_stunnedEnemies.TryGetValue(unit, out var remainingTime))
            {
                remainingTime -= deltaTime;
                if (remainingTime <= 0f)
                {
                    _stunnedEnemies.Remove(unit);
                    return -remainingTime;
                }
                else
                {
                    _stunnedEnemies[unit] = remainingTime;
                    return 0;
                }
            }
            return 0f;
        }

        public static void ClearAllStuns()
        {
            _stunnedEnemies.Clear();
        }
    }

    /// <summary>
    /// Patch ICommand UpdateTime to prevent stunned xenos from acting (for PsionicScream)
    /// This uses the same pattern as XenoAffinity6 but with its own stun system
    /// </summary>
    [HarmonyPatch]
    public static class PsionicScream_StunUpdateTime_Patch
    {
        public static IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            var commandInterfaceType = typeof(SpaceCommander.Commands.ICommand);
            var timeListenerType = typeof(TimeSystem.ITimeUpdatedListener);

            var implementers = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => {
                    try { return a.GetTypes(); }
                    catch { return System.Type.EmptyTypes; }
                })
                .Where(t => t.IsClass && !t.IsAbstract &&
                           commandInterfaceType.IsAssignableFrom(t) &&
                           timeListenerType.IsAssignableFrom(t));

            foreach (var implementer in implementers)
            {
                var method = implementer.GetMethod("UpdateTime",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
                    null,
                    new System.Type[] { typeof(float) },
                    null);

                if (method != null && method.DeclaringType == implementer)
                {
                    yield return method;
                }
            }
        }

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<System.Type, System.Reflection.FieldInfo> _fieldCache = new();

        [HarmonyPrefix]
        public static bool Prefix(object __instance, ref float __0)
        {
            if (!PsionicScream.Instance.IsActive)
                return true;

            var type = __instance.GetType();

            if (!_fieldCache.TryGetValue(type, out var fieldInfo))
            {
                fieldInfo = type.GetField("_battleUnit", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                _fieldCache.Add(type, fieldInfo);
            }

            if (fieldInfo != null)
            {
                var battleUnit = fieldInfo.GetValue(__instance) as BattleUnit;
                if (battleUnit != null && PsionicScream_StunSystem.IsStunned(battleUnit))
                {
                    var remainingTime = PsionicScream_StunSystem.UpdateStunTimers(battleUnit, __0);
                    __0 = remainingTime;
                    return true;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Clear stuns when game ends
    /// </summary>
    [HarmonyPatch(typeof(SpaceCommander.GameFlow.TestGame), "EndGame")]
    public static class PsionicScream_ClearStuns_Patch
    {
        public static void Postfix()
        {
            PsionicScream_StunSystem.ClearAllStuns();
        }
    }
}
