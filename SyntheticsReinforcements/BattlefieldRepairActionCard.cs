using SpaceCommander;
using SpaceCommander.ActionCards;
using System.Collections.Generic;
using UnityEngine;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    /// <summary>
    /// Battlefield Repair action card - allows healing units in battle by consuming points.
    /// Excess healing converts to armor.
    /// </summary>
    public class BattlefieldRepairActionCard : ActionCard, IUnitTargetable
    {
        private readonly Enumerations.Team _teamToAffect;
        private int _lastKnownCoins;

        public Enumerations.Team TeamToAffect => _teamToAffect;

        public BattlefieldRepairActionCard(ActionCardInfo actionCardInfo, Enumerations.Team teamToAffect)
        {
            Info = actionCardInfo;
            _teamToAffect = teamToAffect;

            // Initialize uses based on current wallet amount
            var playerData = Singleton<Player>.Instance.PlayerData;
            if (playerData?.PlayerWallet != null)
            {
                _usesLeft = playerData.PlayerWallet.Coins;
                _lastKnownCoins = playerData.PlayerWallet.Coins;

                // Subscribe to coin change events
                playerData.PlayerWallet.OnCoinsChanged += OnCoinsChanged;
            }
            else
            {
                _usesLeft = 0;
                _lastKnownCoins = 0;
            }
        }

        public override ActionCard GetCopy()
        {
            return new BattlefieldRepairActionCard(Info, _teamToAffect);
        }

        public override void Use()
        {
            // Deduct points from player wallet
            var playerData = Singleton<Player>.Instance.PlayerData;
            if (playerData?.PlayerWallet != null && _usesLeft > 0)
            {
                playerData.PlayerWallet.ChangeCoinsByValue(-1);
                // _usesLeft will be updated by OnCoinsChanged callback
            }
        }

        /// <summary>
        /// Callback for when player coins change - synchronizes Uses with wallet
        /// </summary>
        private void OnCoinsChanged(int newCoins)
        {
            int delta = newCoins - _lastKnownCoins;
            _lastKnownCoins = newCoins;

            // Adjust uses based on the change
            _usesLeft += delta;
            if (_usesLeft < 0) _usesLeft = 0;
        }

        /// <summary>
        /// Cleanup event subscription
        /// </summary>
        ~BattlefieldRepairActionCard()
        {
            var playerData = Singleton<Player>.Instance?.PlayerData;
            if (playerData?.PlayerWallet != null)
            {
                playerData.PlayerWallet.OnCoinsChanged -= OnCoinsChanged;
            }
        }

        public void ApplyCommand(BattleUnit unit)
        {
            if (!BattlefieldRepair.Instance.IsActive)
                return;

            // Get heal amount based on current stacks (1 point = HealPerPoint HP)
            float totalHeal = BattlefieldRepair.HealPerPoint[BattlefieldRepair.Instance.currentStacks - 1];

            // Calculate how much healing is needed to reach max health
            float maxHealth = unit.CurrentMaxHealth;
            float currentHealth = unit.CurrentHealth;
            float healthDeficit = maxHealth - currentHealth;

            if (healthDeficit > 0f)
            {
                // Apply healing up to max health
                float actualHeal = Mathf.Min(totalHeal, healthDeficit);
                unit.Heal(actualHeal);

                // Calculate excess healing
                float excessHeal = totalHeal - actualHeal;

                // Convert excess healing to armor
                if (excessHeal > 0f)
                {
                    UnitStatsTools.AddArmorToUnit(unit, excessHeal);
                }
            }
            else
            {
                // Unit is at full health, all healing converts to armor
                UnitStatsTools.AddArmorToUnit(unit, totalHeal);
            }
        }

        IEnumerable<CommandsAvailabilityChecker.UnitAnavailableReasons> IUnitTargetable.IsUnitValid(BattleUnit unit)
        {
            var reasons = new List<CommandsAvailabilityChecker.UnitAnavailableReasons>();

            // Only available if BattlefieldRepair reinforcement is active
            if (!BattlefieldRepair.Instance.IsActive)
            {
                // Card shouldn't be available at all if reinforcement is not active
                return reasons;
            }

            // Uses is already synchronized with wallet coins, so no need for additional checks
            // Can target any player unit (healing and armor are always useful)
            return reasons;
        }
    }
}
