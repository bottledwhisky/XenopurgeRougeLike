using HarmonyLib;
using SpaceCommander;
using SpaceCommander.Database;
using SpaceCommander.EndGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.SyntheticsReinforcements
{
    public class BioSteel : Reinforcement
    {
        public static readonly float HealthRestored = 5f;
        public BioSteel()
        {
            company = Company.Synthetics;
            stackable = true;
            maxStacks = 3;
            name = L("synthetics.bio_steel.name");
            description = L("synthetics.bio_steel.description");
            flavourText = L("synthetics.bio_steel.flavour");
        }


        public override string GetDescriptionForStacks(int stacks)
        {
            return L("synthetics.bio_steel.description", HealthRestored * stacks);
        }

        protected static BioSteel instance;
        public static BioSteel Instance => instance ??= new();
    }

    [HarmonyPatch(typeof(TestGame), "EndGame")]
    public static class BioSteel_Patch
    {
        public static void Postfix(TestGame __instance, EndGameResultData data)
        {
            if (!BioSteel.Instance.IsActive)
                return;
            var _playerData = Singleton<Player>.Instance.PlayerData;
            List<UpgradableUnit> list = [.. _playerData.Squad.SquadUnits];
            float unitCloneStartingHealth = Singleton<AssetsDatabase>.Instance.UpgradeDataSO.UnitCloneStartingHealth;
            GameManager gameManager = GameManager.Instance;
            var _teams = AccessTools.Field(typeof(GameManager), "_teams").GetValue(gameManager) as Dictionary<Enumerations.Team, BattleUnitsManager>;
            var battleUnits = _teams[Enumerations.Team.Player].BattleUnits.Concat(_teams[Enumerations.Team.Player].ExtractedUnits).Concat(_teams[Enumerations.Team.Player].DeadUnits);
            using IEnumerator<BattleUnit> enumerator = battleUnits.GetEnumerator();
            while (enumerator.MoveNext())
            {
                BattleUnit battleUnit = enumerator.Current;
                string unitId = battleUnit.UnitId;
                UpgradableUnit upgradableUnit = list.Find(u => u.UnitId == battleUnit.UnitId);
                if (upgradableUnit != null)
                {
                    if (battleUnit.CurrentHealth <= 0f)
                    {
                        upgradableUnit.SetCurrentHealthAfterBattle(unitCloneStartingHealth + BioSteel.HealthRestored * BioSteel.Instance.currentStacks);
                    }
                    else
                    {
                        upgradableUnit.SetCurrentHealthAfterBattle(battleUnit.CurrentHealth + BioSteel.HealthRestored * BioSteel.Instance.currentStacks);
                    }
                }
            }
        }
    }
}
