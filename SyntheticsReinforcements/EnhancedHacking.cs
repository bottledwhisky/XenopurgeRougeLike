using HarmonyLib;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Area;
using SpaceCommander.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TimeSystem;
using UnityEngine;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    // Enhanced Breaching: Hacked doors have +200% health, ignited rooms deal +100% damage, and decompressing room no longer requires sealed rooms.
    // Lv2: Reveal spaceship also reveals extraction, explore rooms grants vision in revealed rooms, and delay enemy wave stuns all enemies for 5s.
    public class EnhancedHacking : Reinforcement
    {
        public static readonly float DoorHealthMultiplier = 3f; // 200% increase = 3x multiplier
        public static readonly float RigDamageMultiplier = 2f; // 100% increase = 2x multiplier
        public static float StunDuration = 5f;

        public EnhancedHacking()
        {
            company = Company.Synthetics;
            stackable = true;
            maxStacks = 2;
            name = L("synthetics.enhanced_hacking.name");
            description = "";
            rarity = Rarity.Elite;
            flavourText = L("synthetics.enhanced_hacking.flavour");
        }

        public override string GetDescriptionForStacks(int stacks)
        {
            if (stacks == 1)
            {
                return L("synthetics.enhanced_hacking.description_lv1", (int)((DoorHealthMultiplier - 1) * 100), (int)((RigDamageMultiplier - 1) * 100));
            }
            else
            {
                return L("synthetics.enhanced_hacking.description_lv2", (int)((DoorHealthMultiplier - 1) * 100), (int)((RigDamageMultiplier - 1) * 100), (int)StunDuration);
            }
        }

        protected static EnhancedHacking instance;
        public static EnhancedHacking Instance => instance ??= new();
    }

    /// <summary>
    /// Patch to increase door health by 100% when doors are connected (during initialization)
    /// This patches the Door.ConnectToDoor method to double the door's health
    /// </summary>
    [HarmonyPatch(typeof(Door), "ConnectToDoor")]
    public static class EnhancedHacking_DoorHealth_Patch
    {
        public static void Prefix(ref DoorData doorData)
        {
            if (!EnhancedHacking.Instance.IsActive)
                return;

            // Double the door health
            doorData = new DoorData(doorData.Health * EnhancedHacking.DoorHealthMultiplier);
        }
    }

    /// <summary>
    /// Patch to increase rig damage by 100%
    /// This patches the Room.RigRoom method to double the damage range
    /// </summary>
    [HarmonyPatch(typeof(Room), "RigRoom")]
    public static class EnhancedHacking_RigDamage_Patch
    {
        public static void Prefix(ref Vector2 damage)
        {
            if (!EnhancedHacking.Instance.IsActive)
                return;

            // Double the damage range
            damage = new Vector2(
                damage.x * EnhancedHacking.RigDamageMultiplier,
                damage.y * EnhancedHacking.RigDamageMultiplier
            );
        }
    }

    /// <summary>
    /// Patch to remove the "doors must be closed" requirement for venting
    /// This patches VentRoom_Card.IsRoomValid to skip the open doors check
    /// </summary>
    [HarmonyPatch(typeof(VentRoom_Card), "SpaceCommander.ActionCards.IRoomTargetable.IsRoomValid")]
    public static class EnhancedHacking_VentNoSeal_Patch
    {
        public static void Postfix(Room room, ref IEnumerable<CommandsAvailabilityChecker.RoomAnavailableReasons> __result)
        {
            if (!EnhancedHacking.Instance.IsActive)
                return;

            // Remove the RoomHasOpenDoors reason from the validation results
            var reasons = __result.ToList();
            reasons.RemoveAll(r => r == CommandsAvailabilityChecker.RoomAnavailableReasons.RoomHasOpenDoors);
            __result = reasons;
        }
    }

    /// <summary>
    /// Lv2: Patch RevealSpaceship_Card to also reveal extraction tiles
    /// </summary>
    [HarmonyPatch(typeof(RevealSpaceship_Card), "ApplyCommand")]
    public static class EnhancedHacking_RevealExtraction_Patch
    {
        public static void Postfix()
        {
            if (EnhancedHacking.Instance.currentStacks < 2)
                return;

            var gameManager = GameManager.Instance;
            var extractionTiles = gameManager.ObjectivesVisibilityManager?.GridExtractionTilesFinder?.SpecialTiles;

            if (extractionTiles != null)
            {
                foreach (var tile in extractionTiles)
                {
                    tile.Tile.Room.SetExplored(Enumerations.Team.Player);
                }
            }
        }
    }

    /// <summary>
    /// Lv2: Patch ExploreRooms_Card to grant vision in revealed rooms
    /// Static storage for revealed room tiles
    /// </summary>
    public static class EnhancedHacking_VisionStorage
    {
        public static HashSet<Tile> RevealedRoomTiles = [];
    }

    // Patch to clear buffs when mission ends
    [HarmonyPatch(typeof(TestGame), "EndGame")]
    public class EnhancedHacking_TestGame_EndGame_Patch
    {
        public static void Postfix()
        {
            EnhancedHacking_VisionStorage.RevealedRoomTiles.Clear();
        }
    }

    [HarmonyPatch(typeof(ExploreRooms_Card), "ApplyCommand")]
    public static class EnhancedHacking_GrantVision_Patch
    {
        public static void Prefix(out List<Room> __state)
        {
            var gameManager = GameManager.Instance;
            var rooms = gameManager.GridManager.Rooms.Where(r => r.IsExploredByPlayer);
            __state = [.. rooms];
        }

        public static void Postfix(List<Room> __state)
        {
            if (EnhancedHacking.Instance.currentStacks < 2)
                return;

            var gameManager = GameManager.Instance;
            var rooms = gameManager.GridManager.Rooms.Where(r => r.IsExploredByPlayer);

            var newRooms = rooms.Except(__state);

            // Add all tiles from explored rooms to the vision list
            foreach (var room in newRooms)
            {
                foreach (var tile in room.TilesOfRoom)
                {
                    EnhancedHacking_VisionStorage.RevealedRoomTiles.Add(tile);
                }
            }
        }
    }

    /// <summary>
    /// Lv2: Grant vision for tiles in explored rooms by patching PlayerLineOfSightTiles
    /// </summary>
    [HarmonyPatch(typeof(PlayerLineOfSightTiles), "UpdateLineOfSightTiles")]
    public static class EnhancedHacking_AddVisionTiles_Patch
    {
        public static void Postfix(ref IEnumerable<Tile> __result)
        {
            if (EnhancedHacking.Instance.currentStacks < 2)
                return;

            if (EnhancedHacking_VisionStorage.RevealedRoomTiles.Count == 0)
                return;

            // Combine the original line of sight with our revealed room tiles
            var combinedTiles = __result.ToList();
            combinedTiles.AddRange(EnhancedHacking_VisionStorage.RevealedRoomTiles);
            __result = [.. combinedTiles.Distinct()];
        }
    }

    /// <summary>
    /// Lv2: Stun system for DelayEnemyWave_Card
    /// Maintains a cache of stunned enemies
    /// </summary>
    public static class EnhancedHacking_StunSystem
    {
        private static Dictionary<BattleUnit, float> _stunnedEnemies = new Dictionary<BattleUnit, float>();

        public static void StunAllEnemies()
        {
            var gameManager = GameManager.Instance;
            var enemyTeam = gameManager.GetTeamManager(Enumerations.Team.EnemyAI);

            if (enemyTeam != null)
            {
                foreach (var enemy in enemyTeam.BattleUnits)
                {
                    if (enemy.IsAlive)
                    {
                        _stunnedEnemies[enemy] = EnhancedHacking.StunDuration;
                    }
                }
            }
        }

        public static float GetRemainingStunTime(BattleUnit unit)
        {
            if (_stunnedEnemies.TryGetValue(unit, out float remainingTime))
            {
                return remainingTime;
            }
            return 0f;
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

        public static bool IsStunned(BattleUnit unit)
        {
            return _stunnedEnemies.ContainsKey(unit) && unit.Team == Enumerations.Team.EnemyAI;
        }
    }

    /// <summary>
    /// Lv2: Patch DelayEnemyWave_Card to stun all enemies
    /// </summary>
    [HarmonyPatch(typeof(DelayEnemyWave_Card), "ApplyCommand")]
    public static class EnhancedHacking_StunEnemies_Patch
    {
        public static void Postfix()
        {
            if (EnhancedHacking.Instance.currentStacks < 2)
                return;

            EnhancedHacking_StunSystem.StunAllEnemies();
        }
    }

    /// <summary>
    /// Lv2: Patch ICommand and ITimeUpdatedListener implementations to handle stunning
    /// We need to patch the UpdateTime method to prevent execution when stunned
    /// </summary>
    [HarmonyPatch]
    public static class EnhancedHacking_StunUpdateTime_Patch
    {
        // Dynamically find all types that implement both ICommand and ITimeUpdatedListener
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var commandInterfaceType = typeof(SpaceCommander.Commands.ICommand);
            var timeListenerType = typeof(TimeSystem.ITimeUpdatedListener);

            var implementers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Type.EmptyTypes; }
                })
                .Where(t => t.IsClass && !t.IsAbstract &&
                           commandInterfaceType.IsAssignableFrom(t) &&
                           timeListenerType.IsAssignableFrom(t));

            foreach (var implementer in implementers)
            {
                // Try to get the UpdateTime method
                var method = implementer.GetMethod("UpdateTime",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    [typeof(float)],
                    null);

                if (method != null && method.DeclaringType == implementer)
                {
                    yield return method;
                }
            }
        }

        private static readonly ConditionalWeakTable<Type, FieldInfo> _fieldCache = [];

        [HarmonyPrefix]
        public static bool Prefix(object __instance, ref float __0)
        {
            if (EnhancedHacking.Instance.currentStacks < 2)
                return true;

            // Try to get the BattleUnit from the command
            // Most commands have a _battleUnit field

            var type = __instance.GetType();

            if (!_fieldCache.TryGetValue(type, out var fieldInfo))
            {
                fieldInfo = type.GetField("_battleUnit", BindingFlags.Instance | BindingFlags.NonPublic);
                _fieldCache.Add(type, fieldInfo);
            }


            if (fieldInfo != null)
            {
                var battleUnit = fieldInfo.GetValue(__instance) as BattleUnit;
                if (battleUnit != null && EnhancedHacking_StunSystem.IsStunned(battleUnit))
                {
                    // Update the stun timer but don't execute the command
                    var remainingTime = EnhancedHacking_StunSystem.UpdateStunTimers(battleUnit, __0);
                    __0 = remainingTime;
                    return true; // Skip the original method
                }
            }
            return true; // Execute the original method
        }
    }
}

