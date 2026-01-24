using HarmonyLib;
using MelonLoader;
using SpaceCommander.PartyCustomization;
using SpaceCommander.PartyCustomization.UI;
using SpaceCommander.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XenopurgeRougeLike.RockstarReinforcements;

namespace XenopurgeRougeLike
{
    public class InspectReinforcementDirectory : MonoBehaviour, IDirectory<DirectoryData>
    {
        private readonly DirectoryTextData _directoryTextData = new();
        private Guid _directoryId;
        public DirectoryTextData NameTextDataOfDirectory => _directoryTextData;

        public DirectoryData Initialize()
        {
            try
            {
                MelonLogger.Msg($"InspectReinforcementDirectory.Initialize");
                _directoryTextData.Reset();
                _directoryId = Guid.NewGuid();
                List<CompanyAffinity> enabledAffinities = [];
                foreach (var company in Company.companies)
                {
                    if (company.Value.Affinities == null)
                    {
                        MelonLogger.Msg($"InspectReinforcementDirectory.Initialize: {company.Value.Name} has null affinities");
                        continue;
                    }
                    enabledAffinities.AddRange(company.Value.Affinities.Where(aff => aff.IsActive));
                }
                DirectoryData directoryData = new(_directoryId)
                {
                    ButtonData = [..enabledAffinities.Select(aff => new ButtonData(){
                    MainText = aff.ToMenuItem(),
                    Tooltip = aff.ToString(),
                    onSelectCallback = new Action(() =>
                    {
                        ShowDetails(aff);
                    })
                }), ..XenopurgeRougeLike.acquiredReinforcements.Select(x => new ButtonData()
                {
                    MainText = x.ToMenuItem(),
                    Tooltip = x.ToString(),
                    onSelectCallback = new Action(() =>
                    {
                        ShowDetails(x);
                    })
                })]
                };
                return directoryData;
            }
            catch (Exception ex)
            {
                MelonLogger.Error(ex);
                MelonLogger.Error(ex.StackTrace);
                throw ex;
            }
        }

        private DetailsArea_SquadManagement _detailsArea_SquadManagement;

        public void ShowDetails(CompanyAffinity x)
        {
            MelonLogger.Msg($"{x.ToMenuItem()} Selected");
            if (_detailsArea_SquadManagement == null)
            {
                var _index_SquadManagementDirectory = AccessTools.Field(typeof(SquadManagementWindowController), "_index_SquadManagementDirectory").GetValue(SquadManagementWindowController_Patch.Instance) as Index_SquadManagementDirectory;
                _detailsArea_SquadManagement = AccessTools.Field(typeof(Index_SquadManagementDirectory), "_detailsArea_SquadManagement").GetValue(_index_SquadManagementDirectory) as DetailsArea_SquadManagement;
            }
            _detailsArea_SquadManagement.gameObject.SetActive(true);
            _detailsArea_SquadManagement.SetDetails(x.ToFullDescription());
        }

        public void ShowDetails(Reinforcement x)
        {
            MelonLogger.Msg($"{x.ToMenuItem()} Selected");
            if (_detailsArea_SquadManagement == null)
            {
                var _index_SquadManagementDirectory = AccessTools.Field(typeof(SquadManagementWindowController), "_index_SquadManagementDirectory").GetValue(SquadManagementWindowController_Patch.Instance) as Index_SquadManagementDirectory;
                _detailsArea_SquadManagement = AccessTools.Field(typeof(Index_SquadManagementDirectory), "_detailsArea_SquadManagement").GetValue(_index_SquadManagementDirectory) as DetailsArea_SquadManagement;
            }
            _detailsArea_SquadManagement.gameObject.SetActive(true);
            _detailsArea_SquadManagement.SetDetails(x.ToFullDescription());
        }

        public void OnBackClicked()
        {
            _detailsArea_SquadManagement.gameObject.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(SquadManagementWindowController), "InitializeData")]
    public class SquadManagementWindowController_Patch
    {
        public static SquadManagementWindowController Instance;
        public static bool Prefix(SquadManagementWindowController __instance)
        {
            Instance = __instance;
            return true;
        }
    }

    [HarmonyPatch(typeof(MenuButtons_SquadManagementDirectory), "Initialize")]
    public class ReinforcementManagementUI
    {
        public static void Postfix(MenuButtons_SquadManagementDirectory __instance, ref DirectoryData __result)
        {
            MelonLogger.Msg($"ReinforcementManagementUI.Postfix: begin");
            var buttonData = __result.ButtonData.ToList();
            buttonData.Add(new()
            {
                MainText = "Inspect Reinfocements",
                Tooltip = "Inspect the acquired reinforcements and company affinities.",
                onClickCallback = new Action(InspectClicked)
            });

            if (RockstarAffinity2.IsAnyRockstarAffinityActive)
            {
                // Add fan count display
                buttonData.Add(new()
                {
                    MainText = "Fan Count",
                    Tooltip = $"Current fan count: {RockstarAffinityHelpers.fanCount}",
                    IsDisabled = true
                });
            }

            __result.ButtonData = buttonData;
            MelonLogger.Msg($"ReinforcementManagementUI.Postfix: end");
        }

        public static InspectReinforcementDirectory _InspectReinforcementDirectory = new();

        public static void InspectClicked()
        {
            MelonLogger.Msg($"ReinforcementManagementUI.InspectClicked");
            SquadManagementWindowController smwc = SquadManagementWindowController_Patch.Instance;
            smwc.DirectoriesFlowController.OnDirectoryChanged(_InspectReinforcementDirectory);
        }
    }
}
