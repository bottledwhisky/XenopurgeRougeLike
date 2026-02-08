using HarmonyLib;
using MelonLoader;
using SpaceCommander.PartyCustomization;
using SpaceCommander.PartyCustomization.UI;
using SpaceCommander.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XenopurgeRougeLike.RockstarReinforcements;

namespace XenopurgeRougeLike
{
    using static ModLocalization;

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

                // Group reinforcements by company
                var reinforcementsByCompany = AwardSystem.acquiredReinforcements
                    .GroupBy(r => r.company.Type)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Sort companies by number of reinforcements (descending)
                var sortedCompanies = Company.companies.Values
                    .OrderByDescending(c => reinforcementsByCompany.ContainsKey(c.Type) ? reinforcementsByCompany[c.Type].Count : 0)
                    .ToList();

                List<ButtonData> allButtons = [];

                foreach (var company in sortedCompanies)
                {
                    // Add affinities in order: level 2, 4, 6
                    if (company.Affinities != null)
                    {
                        var activeAffinities = company.Affinities
                            .Where(aff => aff.IsActive)
                            .OrderBy(aff => aff.unlockLevel)
                            .ToList();

                        foreach (var aff in activeAffinities)
                        {
                            allButtons.Add(new ButtonData()
                            {
                                MainText = aff.ToMenuItem(),
                                Tooltip = aff.ToString(),
                                onSelectCallback = new Action(() =>
                                {
                                    ShowDetails(aff);
                                })
                            });
                        }
                    }

                    // Add reinforcements for this company
                    if (reinforcementsByCompany.ContainsKey(company.Type))
                    {
                        foreach (var reinforcement in reinforcementsByCompany[company.Type])
                        {
                            allButtons.Add(new ButtonData()
                            {
                                MainText = reinforcement.ToMenuItem(),
                                Tooltip = reinforcement.ToString(),
                                onSelectCallback = new Action(() =>
                                {
                                    ShowDetails(reinforcement);
                                })
                            });
                        }
                    }
                }

                DirectoryData directoryData = new(_directoryId)
                {
                    ButtonData = allButtons
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
                MainText = L("ui.inspect_reinforcements").ToString(),
                Tooltip = L("ui.inspect_reinforcements_tooltip").ToString(),
                onClickCallback = new Action(InspectClicked)
            });

            if (RockstarAffinity2.IsAnyRockstarAffinityActive)
            {
                // Add fan count display
                buttonData.Add(new()
                {
                    MainText = L("ui.fan_count").ToString(),
                    Tooltip = L("ui.current_fan_count", RockstarAffinityHelpers.fanCount).ToString(),
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
