using SpaceCommander;
using System.Collections.Generic;
using static SpaceCommander.Enumerations;

namespace XenopurgeRougeLike.XenoReinforcements
{
    /// <summary>
    /// Unified tracking system for stunned and mind-controlled xenos.
    /// This centralizes tracking so DevourWill can check if a unit was stunned
    /// or mind-controlled when it dies.
    /// </summary>
    public static class XenoStunTracker
    {
        private static HashSet<BattleUnit> _stunnedUnits = new HashSet<BattleUnit>();
        private static HashSet<BattleUnit> _mindControlledUnits = new HashSet<BattleUnit>();

        /// <summary>
        /// Mark a unit as stunned (called by individual stun systems)
        /// </summary>
        public static void MarkAsStunned(BattleUnit unit)
        {
            if (unit != null && unit.Team == Team.EnemyAI)
            {
                _stunnedUnits.Add(unit);
            }
        }

        /// <summary>
        /// Remove a unit from stun tracking (called when stun expires)
        /// </summary>
        public static void MarkAsNotStunned(BattleUnit unit)
        {
            _stunnedUnits.Remove(unit);
        }

        /// <summary>
        /// Check if a unit is currently stunned (by any stun source)
        /// </summary>
        public static bool IsStunned(BattleUnit unit)
        {
            return _stunnedUnits.Contains(unit);
        }

        /// <summary>
        /// Mark a unit as mind-controlled (called when MindControl converts a unit)
        /// </summary>
        public static void MarkAsMindControlled(BattleUnit unit)
        {
            if (unit != null)
            {
                _mindControlledUnits.Add(unit);
            }
        }

        /// <summary>
        /// Check if a unit was mind-controlled
        /// </summary>
        public static bool IsMindControlled(BattleUnit unit)
        {
            return _mindControlledUnits.Contains(unit);
        }

        /// <summary>
        /// Check if a unit is either stunned or mind-controlled
        /// </summary>
        public static bool IsStunnedOrMindControlled(BattleUnit unit)
        {
            return IsStunned(unit) || IsMindControlled(unit);
        }

        /// <summary>
        /// Clear all tracking (called on mission end)
        /// </summary>
        public static void ClearAll()
        {
            _stunnedUnits.Clear();
            _mindControlledUnits.Clear();
        }
    }
}
