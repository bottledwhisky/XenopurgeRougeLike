using HarmonyLib;
using SpaceCommander;
using SpaceCommander.ActionCards;
using SpaceCommander.Area;
using SpaceCommander.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TimeSystem;
using UnityEngine;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    // 强化黑入：被黑入的门生命值提升100%，点燃房间伤害+100%，降压房间不再要求房间封闭。
    public class EnhancedHacking : Reinforcement
    {
        public static readonly float DoorHealthMultiplier = 3f; // 200% increase = 3x multiplier
        public static readonly float RigDamageMultiplier = 2f; // 100% increase = 2x multiplier

        public EnhancedHacking()
        {
            company = Company.Synthetics;
            stackable = true;
            maxStacks = 2;
            name = "Enhanced Breaching";
            description = "";
            rarity = Rarity.Elite;
            flavourText = "Military-grade intrusion software pushes compromised systems beyond their standard operational parameters.";
        }

        public override string Description
        {
            get
            {
                if (currentStacks == 1)
                {
                    return "Lv1: Hacked doors have +200% health, ignited rooms deal +100% damage, and decompressing room no longer requires sealed rooms.";
                }
                else
                {
                    return "Lv2: Download Schematics will reveal the extract point. Pull Camera Recordings will grant you vision in the revealed rooms. Subsonic Disruption will stun all enemies on map for 5 seconds.";
                }
            }
        }

        public static EnhancedHacking Instance => (EnhancedHacking)Synthetics.Reinforcements[typeof(EnhancedHacking)];
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

    public class PatchUtils
    {
        public void PatchAllImplementations(MethodInfo prefix, MethodInfo postfix, params Type[] interfaceTypes)
        {
            var harmony = XenopurgeRougeLike._HarmonyInstance;

            // Get all types that inherit from B
            var implementers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && interfaceTypes.All(baseType => baseType.IsAssignableFrom(t)));

            foreach (var implementer in implementers)
            {
                // Option 1: Get the class's own implementation
                var original = implementer.GetMethod("UpdateTime",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    [typeof(float)],
                    null);

                // Option 2: Handle explicit interface implementations
                if (original == null)
                {
                    foreach (var type in interfaceTypes)
                    {
                        var map = implementer.GetInterfaceMap(type);
                        for (int i = 0; i < map.InterfaceMethods.Length; i++)
                        {
                            if (map.InterfaceMethods[i].Name == "UpdateTime")
                            {
                                original = map.TargetMethods[i];
                                break;
                            }
                        }
                    }
                }

                if (original != null)
                {
                    harmony.Patch(original, prefix: new HarmonyMethod(prefix));
                    Console.WriteLine($"Patched {implementer.Name}.UpdateTime");
                }
            }
        }
    }
}

