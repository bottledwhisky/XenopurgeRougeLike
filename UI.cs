using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using SpaceCommander.BattleManagement.UI;
using SpaceCommander.EndGame;
using SpaceCommander.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XenopurgeRougeLike.CommonReinforcements;

namespace XenopurgeRougeLike
{
    using static ModLocalization;
    // Helper class for PlayerWallet access
    public static class PlayerWalletHelper
    {
        public static int GetCoins()
        {
            var playerData = Singleton<Player>.Instance?.PlayerData;
            return playerData?.PlayerWallet?.Coins ?? 0;
        }

        public static void ChangeCoins(int amount)
        {
            var playerData = Singleton<Player>.Instance?.PlayerData;
            if (playerData?.PlayerWallet != null)
            {
                playerData.PlayerWallet.ChangeCoinsByValue(amount);
            }
        }
    }

    [HarmonyPatch(typeof(EndGameWindowButtons_BattleManagementDirectory), "Initialize")]
    public static class EndGameWindowButtons_BattleManagementDirectory_Initialize_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(EndGameWindowButtons_BattleManagementDirectory __instance, ref DirectoryData __result)
        {
            try
            {
                MelonLogger.Msg("Patching EndGameWindowButtons_BattleManagementDirectory.Initialize to add reinforcement choices.");
                if (!EndGameWindowView_SetResultText_Patch.isGameContinue) return;
                MelonLogger.Msg("Game is continuing, adding reinforcement choice buttons.");
                var ProceedClicked = AccessTools.Method(typeof(EndGameWindowButtons_BattleManagementDirectory), "ProceedClicked");
                List<ButtonData> buttonDataList = [];
                // Add reinforcement choices as buttons
                for (int i = 0; i < AwardSystem.choices.Count; i++)
                {
                    int capturedIndex = i; // Capture loop variable for closure
                    MelonLogger.Msg($"Adding reinforcement choice button for index {i}");
                    var choice = AwardSystem.choices[i];
                    var preview = choice.GetNextLevelPreview();
                    int cost = Reinforcement.RarityCosts[preview.Rarity];
                    bool canAfford = PlayerWalletHelper.GetCoins() >= cost;

                    ButtonData buttonData = new()
                    {
                        MainText = preview.MenuItem + $"({L("ui.cost_format", cost)})",
                        IsDisabled = !canAfford,
                        onSelectCallback = () =>
                        {
                            try
                            {
                                var choice = AwardSystem.choices[capturedIndex];
                                var preview = choice.GetNextLevelPreview();
                                var company = choice.company.Type;
                                var nExsitingCompanyReinforces = AwardSystem.acquiredReinforcements.Where(r => r.company.Type == company).Count() + 1;
                                CompanyAffinity affinityToEnable = null;
                                foreach (var affiny in choice.company.Affinities)
                                {
                                    int requiredNReinforces = affiny.unlockLevel;
                                    if (requiredNReinforces >= nExsitingCompanyReinforces)
                                    {
                                        affinityToEnable = affiny;
                                        break;
                                    }
                                }
                                if (affinityToEnable == null && choice.company.Affinities.Count > 0)
                                {
                                    affinityToEnable = choice.company.Affinities.Last();
                                }
                                string progressLabel = "";
                                if (affinityToEnable != null)
                                {
                                    string nextUnlockAffinyText = affinityToEnable.ToString();

                                    progressLabel = affinityToEnable.unlockLevel < nExsitingCompanyReinforces
                                        ? L("ui.max_level_reached")
                                        : L("ui.next_unlock") + $": ({nExsitingCompanyReinforces}/{affinityToEnable.unlockLevel}) " + nextUnlockAffinyText;
                                }

                                EndGameWindowView_SetResultText_Patch.selectedChoiceIndex = capturedIndex;
                                // Update description text when selected
                                EndGameWindowView_SetResultText_Patch._descriptionText.text = preview.FullString + "\n" + progressLabel;

                                // Update border highlights
                                for (int j = 0; j < EndGameWindowView_SetResultText_Patch._choiceOutlines.Length; j++)
                                {
                                    var outline = EndGameWindowView_SetResultText_Patch._choiceOutlines[j];
                                    if (j == capturedIndex)
                                    {
                                        outline.effectColor = Color.yellow;
                                    }
                                    else
                                    {
                                        outline.effectColor = Color.gray;
                                    }
                                    var graphic = outline.GetComponent<Graphic>();
                                    if (graphic != null)
                                    {
                                        graphic.SetVerticesDirty();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MelonLogger.Error(ex);
                                MelonLogger.Error(ex.StackTrace);
                            }
                        },
                        onClickCallback = () =>
                        {
                            var choice = AwardSystem.choices[capturedIndex];
                            var preview = choice.GetNextLevelPreview();
                            int cost = Reinforcement.RarityCosts[preview.Rarity];

                            // Check if player can afford this reinforcement
                            int currentCoins = PlayerWalletHelper.GetCoins();
                            if (currentCoins < cost)
                            {
                                MelonLogger.Warning($"Cannot afford reinforcement {choice.ToMenuItem()} - costs {cost}, have {currentCoins}");
                                return;
                            }

                            // Logic to add the reinforcement to the player's roster
                            MelonLogger.Msg($"Player selected reinforcement from {choice.ToMenuItem()}");

                            // Deduct cost
                            PlayerWalletHelper.ChangeCoins(-cost);
                            MelonLogger.Msg($"Spent {cost} coins, {PlayerWalletHelper.GetCoins()} remaining");

                            // Reset reroll cost for next session
                            AwardSystem.currentRerollCost = 1;

                            // Add reinforcement logic here
                            AwardSystem.AcquireReinforcement(choice);
                            ProceedClicked.Invoke(__instance, null);
                        }
                    };
                    buttonDataList.Add(buttonData);
                }
                MelonLogger.Msg("Finished adding reinforcement choice buttons.");
                // Add a "Reroll" button
                int freeRerollsAvailable = Reconsider.GetFreeRerollsAvailable();
                int rerollCost = freeRerollsAvailable > 0 ? 0 : AwardSystem.currentRerollCost;
                bool canAffordReroll = rerollCost == 0 || PlayerWalletHelper.GetCoins() >= rerollCost;

                ButtonData rerollButtonData = new ButtonData
                {
                    MainText = freeRerollsAvailable > 0
                        ? L("ui.reroll_button_free", freeRerollsAvailable)
                        : L("ui.reroll_button", rerollCost, PlayerWalletHelper.GetCoins()),
                    IsDisabled = !canAffordReroll,
                    IgnoreClickCount = true,
                };

                // Set the callback after creating the button so we can capture a reference to it
                rerollButtonData.onClickCallback = () =>
                {
                    int freeRerolls = Reconsider.GetFreeRerollsAvailable();
                    bool isFreeReroll = freeRerolls > 0;
                    int currentRerollCost = isFreeReroll ? 0 : AwardSystem.currentRerollCost;

                    // Check if player can afford reroll (free rerolls always succeed)
                    int currentCoins = PlayerWalletHelper.GetCoins();
                    if (!isFreeReroll && currentCoins < currentRerollCost)
                    {
                        MelonLogger.Warning($"Cannot afford reroll - costs {currentRerollCost}, have {currentCoins}");
                        return;
                    }

                    MelonLogger.Msg($"Player chose to reroll reinforcement choices. Free: {isFreeReroll}");
                    try
                    {
                        if (isFreeReroll)
                        {
                            // Use a free reroll
                            Reconsider.UseFreeReroll();
                            MelonLogger.Msg($"Used free reroll, {Reconsider.GetFreeRerollsAvailable()} free rerolls remaining");
                        }
                        else
                        {
                            // Deduct reroll cost
                            PlayerWalletHelper.ChangeCoins(-currentRerollCost);
                            MelonLogger.Msg($"Spent {currentRerollCost} coins on reroll, {PlayerWalletHelper.GetCoins()} remaining");

                            // Increase reroll cost for next reroll
                            AwardSystem.currentRerollCost++;
                            MelonLogger.Msg($"Reroll cost increased to {AwardSystem.currentRerollCost}");
                        }

                        // Logic to reroll choices
                        GameManager_GiveEndGameRewards_Patch.Prefix(true);
                        EndGameWindowView_SetResultText_Patch.selectedChoiceIndex = 0;
                        EndGameWindowView_SetResultText_Patch.PopulateChoices(AwardSystem.choices, buttonDataList);

                        // Update reroll button text and state (we have a direct reference)
                        int newFreeRerolls = Reconsider.GetFreeRerollsAvailable();
                        int newRerollCost = newFreeRerolls > 0 ? 0 : AwardSystem.currentRerollCost;
                        bool canAffordNewReroll = newRerollCost == 0 || PlayerWalletHelper.GetCoins() >= newRerollCost;

                        rerollButtonData.MainText = newFreeRerolls > 0
                            ? L("ui.reroll_button_free", newFreeRerolls)
                            : L("ui.reroll_button", newRerollCost, PlayerWalletHelper.GetCoins());
                        rerollButtonData.IsDisabled = !canAffordNewReroll;
                        rerollButtonData.ChangeInteractionState(rerollButtonData.IsDisabled);
                        MelonLogger.Msg($"Updated reroll button: cost={newRerollCost}, free={newFreeRerolls}, disabled={!canAffordNewReroll}");
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error(ex);
                        MelonLogger.Error(ex.StackTrace);
                        throw ex;
                    }
                };
                // Add a "Skip" button
                ButtonData skipButtonData = new ButtonData
                {
                    MainText = L("ui.skip_button"),
                    onClickCallback = () =>
                    {
                        MelonLogger.Msg("Player chose to skip reinforcement selection.");

                        // Grant +3 coins
                        PlayerWalletHelper.ChangeCoins(3);
                        MelonLogger.Msg($"Gained 3 coins from skip, total coins: {PlayerWalletHelper.GetCoins()}");

                        // Reset reroll cost for next session
                        AwardSystem.currentRerollCost = 1;

                        // Logic to skip selection
                        ProceedClicked.Invoke(__instance, null);
                    }
                };
                MelonLogger.Msg("Adding Reroll and Skip buttons.");
                __result.ButtonData = [.. buttonDataList,
                rerollButtonData,
                skipButtonData,
            ];
                MelonLogger.Msg("Finished patching EndGameWindowButtons_BattleManagementDirectory.Initialize.");
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
                MelonLogger.Error(e.StackTrace);
            }
        }
    }

    [HarmonyPatch(typeof(EndGameWindowView), "SetResultText")]
    public static class EndGameWindowView_SetResultText_Patch
    {
        // Container references
        private static GameObject _chooserContainer;
        private static Image[] _choiceImages = new Image[3];
        public static Outline[] _choiceOutlines = new Outline[3];
        private static TextMeshProUGUI[] _choiceTitles = new TextMeshProUGUI[3];
        private static TextMeshProUGUI[] _choiceCosts = new TextMeshProUGUI[3];
        public static TextMeshProUGUI _descriptionText;
        public static bool isGameContinue = false;
        public static int selectedChoiceIndex = 0;

        [HarmonyPostfix]
        public static void Postfix(EndGameWindowView __instance, EndGameResultData endGameResultData)
        {
            try
            {
                isGameContinue = false;
                if (endGameResultData.IsGameOver) return;

                var missionCompletePanelField = AccessTools.Field(typeof(EndGameWindowView), "_missionCompletePanel");
                var missionCompletePanel = (RectTransform)missionCompletePanelField.GetValue(__instance);

                if (missionCompletePanel == null) return;

                // Reset reroll cost at the start of a new reward session
                AwardSystem.currentRerollCost = 1;
                Reconsider.ResetFreeRerolls();
                MelonLogger.Msg("Starting new reward session, reset reroll cost to 1");

                // Get reference text for styling
                var totalCoinsTextField = AccessTools.Field(typeof(EndGameWindowView), "_totalCoinsText");
                var referenceText = (TextMeshProUGUI)totalCoinsTextField.GetValue(__instance);
                selectedChoiceIndex = 0;

                // Create or update the chooser UI
                if (_chooserContainer == null || _chooserContainer.gameObject == null)
                {
                    CreateChooserUI(missionCompletePanel, referenceText);
                }


                PopulateChoices(AwardSystem.choices);
                _chooserContainer.SetActive(true);
                isGameContinue = true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error in EndGameWindowView_SetResultText_Patch: {ex}");
                MelonLogger.Error(ex.StackTrace);
            }
        }

        private static void CreateChooserUI(RectTransform parent, TextMeshProUGUI referenceText)
        {
            // Main container
            _chooserContainer = new GameObject("RoguelikeChooser");
            _chooserContainer.transform.SetParent(parent, false);

            var containerRect = _chooserContainer.AddComponent<RectTransform>();
            var containerLayout = _chooserContainer.AddComponent<VerticalLayoutGroup>();
            containerLayout.padding = new RectOffset(0, 0, 30, 0); // Add top padding (left, right, top, bottom)
            containerLayout.spacing = 15f;
            containerLayout.childAlignment = TextAnchor.MiddleCenter;
            containerLayout.childControlWidth = true;
            containerLayout.childControlHeight = true;
            containerLayout.childForceExpandWidth = false;
            containerLayout.childForceExpandHeight = false;

            var containerLayoutElement = _chooserContainer.AddComponent<LayoutElement>();
            containerLayoutElement.preferredWidth = 500f;
            containerLayoutElement.preferredHeight = 250f;

            // Header text
            var headerGO = new GameObject("Header");
            headerGO.transform.SetParent(_chooserContainer.transform, false);
            var headerText = headerGO.AddComponent<TextMeshProUGUI>();
            if (referenceText != null)
            {
                headerText.font = referenceText.font;
            }
            headerText.fontSize = 24f;
            headerText.color = Color.white;
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.text = L("ui.choose_reinforcement");

            var headerLayout = headerGO.AddComponent<LayoutElement>();
            headerLayout.preferredHeight = 30f;

            // Horizontal container for the 3 choices
            var choicesRow = new GameObject("ChoicesRow");
            choicesRow.transform.SetParent(_chooserContainer.transform, false);

            var choicesRowLayout = choicesRow.AddComponent<HorizontalLayoutGroup>();
            choicesRowLayout.spacing = 20f;
            choicesRowLayout.childAlignment = TextAnchor.MiddleCenter;
            choicesRowLayout.childControlWidth = true;
            choicesRowLayout.childControlHeight = true;
            choicesRowLayout.childForceExpandWidth = false;
            choicesRowLayout.childForceExpandHeight = false;

            var choicesRowLayoutElement = choicesRow.AddComponent<LayoutElement>();
            choicesRowLayoutElement.preferredHeight = 120f;
            choicesRowLayoutElement.flexibleWidth = 1f;

            // Create the 3 choice boxes
            for (int i = 0; i < 3; i++)
            {
                CreateChoiceBox(choicesRow.transform, i, referenceText);
            }

            // description box at the bottom
            var descriptionContainer = new GameObject("DescriptionContainer");
            descriptionContainer.transform.SetParent(_chooserContainer.transform, false);

            var descContainerLayout = descriptionContainer.AddComponent<LayoutElement>();
            descContainerLayout.preferredHeight = 360f;
            descContainerLayout.flexibleWidth = 1f;

            var descPadding = descriptionContainer.AddComponent<VerticalLayoutGroup>();
            descPadding.padding = new RectOffset(15, 15, 10, 15);
            descPadding.spacing = 10f;
            descPadding.childAlignment = TextAnchor.UpperCenter;
            descPadding.childControlWidth = true;
            descPadding.childControlHeight = false;  // Changed to false - let LayoutElement control height
            descPadding.childForceExpandWidth = true;
            descPadding.childForceExpandHeight = false;

            var descTextGO = new GameObject("DescriptionText");
            descTextGO.transform.SetParent(descriptionContainer.transform, false);

            // Add and configure RectTransform first
            var descTextRect = descTextGO.AddComponent<RectTransform>();
            descTextRect.sizeDelta = new Vector2(0, 40f);

            _descriptionText = descTextGO.AddComponent<TextMeshProUGUI>();
            if (referenceText != null)
            {
                _descriptionText.font = referenceText.font;
            }
            _descriptionText.fontSize = 16f;
            _descriptionText.color = new Color(0.9f, 0.9f, 0.9f);
            _descriptionText.alignment = TextAlignmentOptions.Top;  // Changed to Top
            _descriptionText.richText = true;
            _descriptionText.text = L("ui.hover_description");
            _descriptionText.textWrappingMode = TextWrappingModes.Normal;

            var descTextLayout = descTextGO.AddComponent<LayoutElement>();
            descTextLayout.minHeight = 40f;
            descTextLayout.preferredHeight = 80f;  // Give it more room
            descTextLayout.flexibleWidth = 1f;

            // Move to end of layout
            _chooserContainer.transform.SetAsLastSibling();
        }

        private static void CreateChoiceBox(Transform parent, int index, TextMeshProUGUI referenceText)
        {
            // Choice container
            var choiceGO = new GameObject($"Choice_{index}");
            choiceGO.transform.SetParent(parent, false);

            var choiceLayout = choiceGO.AddComponent<LayoutElement>();
            choiceLayout.preferredWidth = 120f;
            choiceLayout.preferredHeight = 120f;

            var choiceVertical = choiceGO.AddComponent<VerticalLayoutGroup>();
            choiceVertical.spacing = 5f;
            choiceVertical.padding = new RectOffset(5, 5, 5, 5);
            choiceVertical.childAlignment = TextAnchor.MiddleCenter;
            choiceVertical.childControlWidth = false;  // Changed to false
            choiceVertical.childControlHeight = false; // Changed to false
            choiceVertical.childForceExpandWidth = false;
            choiceVertical.childForceExpandHeight = false;

            // Background/border
            var choiceBg = choiceGO.AddComponent<Image>();
            choiceBg.color = Color.black;

            // Outline for border effect
            var outline = choiceGO.AddComponent<Outline>();
            outline.effectColor = Color.gray;
            outline.effectDistance = new Vector2(2f, 2f);
            _choiceOutlines[index] = outline;

            // Icon area
            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(choiceGO.transform, false);

            _choiceImages[index] = iconGO.AddComponent<Image>();
            _choiceImages[index].color = Color.black;
            _choiceImages[index].preserveAspect = true;

            // Set RectTransform size directly
            var iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(64f, 64f);

            var iconLayout = iconGO.AddComponent<LayoutElement>();
            iconLayout.minWidth = 64f;
            iconLayout.minHeight = 64f;
            iconLayout.preferredWidth = 64f;
            iconLayout.preferredHeight = 64f;

            // Title below icon
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(choiceGO.transform, false);

            _choiceTitles[index] = titleGO.AddComponent<TextMeshProUGUI>();
            if (referenceText != null)
            {
                _choiceTitles[index].font = referenceText.font;
            }
            _choiceTitles[index].fontSize = 14f;
            _choiceTitles[index].color = Color.white;
            _choiceTitles[index].alignment = TextAlignmentOptions.Center;
            _choiceTitles[index].textWrappingMode = TextWrappingModes.Normal;
            _choiceTitles[index].richText = true;

            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(110f, 35f);

            var titleLayout = titleGO.AddComponent<LayoutElement>();
            titleLayout.minHeight = 35f;
            titleLayout.preferredHeight = 35f;
            titleLayout.preferredWidth = 110f;

            // Cost text below title
            var costGO = new GameObject("Cost");
            costGO.transform.SetParent(choiceGO.transform, false);

            _choiceCosts[index] = costGO.AddComponent<TextMeshProUGUI>();
            if (referenceText != null)
            {
                _choiceCosts[index].font = referenceText.font;
            }
            _choiceCosts[index].fontSize = 12f;
            _choiceCosts[index].color = Color.yellow;
            _choiceCosts[index].alignment = TextAlignmentOptions.Center;
            _choiceCosts[index].text = "0 coins";

            var costRect = costGO.GetComponent<RectTransform>();
            costRect.sizeDelta = new Vector2(110f, 20f);

            var costLayout = costGO.AddComponent<LayoutElement>();
            costLayout.minHeight = 20f;
            costLayout.preferredHeight = 20f;
            costLayout.preferredWidth = 110f;
        }

        public static void PopulateChoices(List<Reinforcement> choices, List<ButtonData> buttonDataList = null)
        {
            MelonLogger.Msg($"Populating reinforcement choices UI with {choices.Count} choices.");

            for (int i = 0; i < 3 && i < choices.Count; i++)
            {
                MelonLogger.Msg($"Populating choice {i}");
                var preview = choices[i].GetNextLevelPreview();

                MelonLogger.Msg($" - Choice: {preview.Name}");
                // Set title
                _choiceTitles[i].text = preview.Name;

                // Set cost
                int cost = Reinforcement.RarityCosts[preview.Rarity];
                _choiceCosts[i].text = L("ui.cost_format", cost);
                _choiceCosts[i].color = PlayerWalletHelper.GetCoins() >= cost ? Color.yellow : Color.red;

                MelonLogger.Msg($" - Title set to: {_choiceTitles[i].text}");
                // Set icon if available, otherwise use colored placeholder
                if (preview.Company.Sprite != null)
                {
                    MelonLogger.Msg($" - Setting sprite for choice {i}");
                    _choiceImages[i].sprite = preview.Company.Sprite;
                    _choiceImages[i].color = Color.white;
                    _choiceImages[i].type = Image.Type.Simple;
                    _choiceImages[i].preserveAspect = true;
                }
                else
                {
                    MelonLogger.Msg($" - No sprite found for choice {i}, using placeholder color");
                    _choiceImages[i].sprite = null;
                    _choiceImages[i].color = preview.Company.BorderColor;
                }

                MelonLogger.Msg($" - Image color set to: {_choiceImages[i].color}");
                // Update border color via Outline component
                var outline = _choiceImages[i].transform.parent.GetComponent<Outline>();
                if (outline != null)
                {
                    MelonLogger.Msg($" - Setting outline color for choice {i}");
                    outline.effectColor = preview.Company.BorderColor;
                }

                // Update the button text and disabled state in the menu list
                if (buttonDataList != null && i < buttonDataList.Count)
                {
                    MelonLogger.Msg($" - Updating ButtonData MainText for choice {i}");
                    buttonDataList[i].MainText = preview.MenuItem;

                    // Update disabled state based on affordability
                    int choiceCost = Reinforcement.RarityCosts[preview.Rarity];
                    bool canAffordChoice = PlayerWalletHelper.GetCoins() >= choiceCost;
                    buttonDataList[i].IsDisabled = !canAffordChoice;

                    // Trigger UI refresh by invoking the OnInteractionStateChanged event
                    buttonDataList[i].ChangeInteractionState(buttonDataList[i].IsDisabled);
                }
            }

            // Reset selectedChoiceIndex if it's out of bounds
            if (selectedChoiceIndex >= choices.Count)
            {
                MelonLogger.Warning($"selectedChoiceIndex {selectedChoiceIndex} is out of bounds for {choices.Count} choices. Resetting to 0.");
                selectedChoiceIndex = 0;
            }

            if (choices.Count > 0)
            {
                _descriptionText.text = choices[selectedChoiceIndex].GetNextLevelPreview().FullString;
                MelonLogger.Msg(" - description text set: " + _descriptionText.text);
                _choiceOutlines[selectedChoiceIndex].effectColor = Color.yellow;
            }
            else
            {
                MelonLogger.Warning("No choices available to populate.");
            }
        }
    }
}
