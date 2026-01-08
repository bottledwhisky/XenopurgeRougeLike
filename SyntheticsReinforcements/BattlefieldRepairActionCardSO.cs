using SpaceCommander;
using SpaceCommander.ActionCards;
using UnityEngine;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    [CreateAssetMenu(fileName = "BattlefieldRepairActionCardSO", menuName = "Scriptables/ActionCards/BattlefieldRepairActionCardSO", order = 1)]
    public class BattlefieldRepairActionCardSO : ActionCardSO
    {
        [SerializeField]
        private Enumerations.Team _teamToAffect = Enumerations.Team.Player;

        public override ActionCard CreateInstance()
        {
            return new BattlefieldRepairActionCard(_actionCardInfo, _teamToAffect);
        }
    }
}
