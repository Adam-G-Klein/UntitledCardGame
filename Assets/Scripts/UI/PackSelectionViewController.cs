using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.VisualScripting;

[System.Serializable]
public class UnlockablePack
{
    public PackSO packSO;
    public AchievementSO achievementSO;
}

public class PackSelectionViewController : MonoBehaviour, IPackSlotViewDelegate
{
    [SerializeField]
    private UIDocument packSelectionUIDocument;
    [SerializeField]
    private VisualTreeAsset packTemplate;
    [SerializeField]
    private GameStateVariableSO gameState;
    public List<PackSO> startingPackSOs;
    public List<UnlockablePack> unlockablePackSOs;
    private List<PackSO> selectedPackSOs;
    private Dictionary<string, PackSlotView> packToSlotMap = new();
    private Dictionary<string, PackSlotView> VEToSlotMap = new();
    private Label packsSelectedLabel;
    private Button selectPacksButton;
    private Toggle toggleEnableAscension;
    private Button decreaseAscensionButton;
    private Button increaseAscensionButton;
    private Label ascensionNumberLabel;
    private Label ascensionDescriptionLabel;
    private int ascensionIndex = 0;
    private VisualElement ascensionContainer;
    private bool ascensionsEnabled = false;

    void Start()
    {
        packsSelectedLabel = packSelectionUIDocument.rootVisualElement.Q<Label>("selectedPacksLabel");
        selectPacksButton = packSelectionUIDocument.rootVisualElement.Q<Button>("selectPacksButton");
        selectPacksButton.RegisterOnSelected(HandlePacksSelected);
        selectPacksButton.pickingMode = PickingMode.Position;
        FocusManager.Instance.RegisterFocusableTarget(selectPacksButton.AsFocusable());
        selectedPackSOs = gameState.previouslySelectedPackSOs;
        toggleEnableAscension = packSelectionUIDocument.rootVisualElement.Q<Toggle>("EnableAscensionToggle");
        decreaseAscensionButton = packSelectionUIDocument.rootVisualElement.Q<Button>("DecreaseAscensionButton");
        increaseAscensionButton = packSelectionUIDocument.rootVisualElement.Q<Button>("IncreaseAscensionButton");
        ascensionNumberLabel = packSelectionUIDocument.rootVisualElement.Q<Label>("AscensionNumberLabel");
        ascensionDescriptionLabel = packSelectionUIDocument.rootVisualElement.Q<Label>("AscensionDescriptionLabel");
        ascensionContainer = packSelectionUIDocument.rootVisualElement.Q<VisualElement>("AscensionSelectionUIDocument");

        StartCoroutine(InitializeAscensionUI());

        UpdateUIState();
        for (var i = 0; i < 5; i++)
        {
            SetupPackSlot(i, selectedPackSOs, "packSlot", true);
        }
        for (var i = 0; i < 5; i++)
        {
            SetupPackSlot(i, startingPackSOs, "startingPackOption", false);
        }
        for (var i = 0; i < 5; i++)
        {
            SetupUnlockablePackSlot(i, unlockablePackSOs, "unlockablePackOption", false);
        }
    }

    private IEnumerator InitializeAscensionUI()
    {
        yield return new WaitUntil(() => ProgressManager.Instance != null && ProgressManager.Instance.ascensionInfo != null);
        if (ProgressManager.Instance.ascensionInfo.playersMaxAscensionUnlocked == -1)
        {
            ascensionContainer.style.display = DisplayStyle.None;
            toggleEnableAscension.value = false;
        }
        else
        {
            // setup the ascension UI functionality
            FocusManager.Instance.RegisterFocusableTarget(toggleEnableAscension.AsFocusable());
            toggleEnableAscension.RegisterValueChangedCallback(toggledAscensionEnabled);
            toggleEnableAscension.pickingMode = PickingMode.Position;
            FocusManager.Instance.RegisterFocusableTarget(decreaseAscensionButton.AsFocusable());
            decreaseAscensionButton.RegisterOnSelected(UpdateSelectedAscension(-1));
            decreaseAscensionButton.pickingMode = PickingMode.Position;
            FocusManager.Instance.RegisterFocusableTarget(increaseAscensionButton.AsFocusable());
            increaseAscensionButton.RegisterOnSelected(UpdateSelectedAscension(1));
            increaseAscensionButton.pickingMode = PickingMode.Position;
        }
    }

    private Action UpdateSelectedAscension(int change)
    {
        return () =>
        {
            if (ascensionIndex == 0 && change < 0) return; // can't go below 0
            if (ascensionIndex >= ProgressManager.Instance.ascensionInfo.playersMaxAscensionUnlocked && change > 0) return; // can't go above max ascension unlocked
            ascensionIndex += change;

            AscensionSO ascensionSO = ProgressManager.Instance.ascensionInfo.ascensionSOList[ascensionIndex];
            ascensionNumberLabel.text = $"Ascension {ascensionIndex + 1}";
            ascensionDescriptionLabel.text = ascensionSO.description;
        };
    }

    private void toggledAscensionEnabled(ChangeEvent<bool> evt)
    {
        ascensionsEnabled = evt.newValue;
    }

    private void SetupPackSlot(int i, List<PackSO> packSOs, string packName, bool isSelected)
    {
        var packSO = i >= packSOs.Count() ? null : packSOs[i];
        var packSlot = packSelectionUIDocument.rootVisualElement.Q<VisualElement>($"{packName}{i + 1}");

        PackSlotView packSlotView = new(this, packSlot, packTemplate, !(packSO != null && selectedPackSOs.Contains(packSOs[i])) || isSelected, isSelected, packSO);
        if (!isSelected && packSO != null) packToSlotMap.Add(packSO.packName, packSlotView);
        VEToSlotMap.Add(packSlot.name, packSlotView);
    }

    private void SetupUnlockablePackSlot(int i, List<UnlockablePack> unlockablePacks, string packName, bool isSelected)
    {
        UnlockablePack unlockablePack = i >= unlockablePacks.Count() ? null : unlockablePacks[i];
        if (unlockablePack == null) return;
        var packSO = unlockablePack.packSO;
        var packSlot = packSelectionUIDocument.rootVisualElement.Q<VisualElement>($"{packName}{i + 1}");

        bool isPackLocked = !unlockablePack.achievementSO.isCompleted;
        PackSlotView packSlotView = new(this, packSlot, packTemplate, !(packSO != null && selectedPackSOs.Contains(packSO)) || isSelected, isSelected, packSO, isPackLocked, unlockablePack.achievementSO);
        if (packSO != null) packToSlotMap.Add(packSO.packName, packSlotView);
        VEToSlotMap.Add(packSlot.name, packSlotView);
    }

    public void PackSlotOnClick(PackSlotView packSlotView)
    {
        VisualElement childToMove = packSlotView.pack;
        var packSO = packSlotView.packSO;
        if (selectedPackSOs.Contains(packSO))
        {
            packSlotView.HandleUnselect(); // the pack should know how to update the style of contents
            selectedPackSOs.Remove(packSO);
            PackSlotView targetPackSlot = packToSlotMap[packSO.packName];
            targetPackSlot.root.Add(childToMove);

            targetPackSlot.packSO = packSlotView.packSO;
            targetPackSlot.pack = packSlotView.pack;
            targetPackSlot.FakePointerLeaveEvent();
            packSlotView.pack = null;
            packSlotView.packSO = null;
        }
        else
        {
            // the pack should know how to update the style of contents
            List<VisualElement> selectPackSlots = packSelectionUIDocument.rootVisualElement.Q<VisualElement>("selectedOptionContainer").Children().ToList();
            for (var i = 0; i < selectPackSlots.Count; i++)
            {
                if (selectPackSlots[i].Children().Count() == 0)
                {
                    selectedPackSOs.Add(packSO);
                    packSlotView.HandleSelect();

                    PackSlotView targetPackSlot = VEToSlotMap[selectPackSlots[i].name];
                    targetPackSlot.pack = packSlotView.pack;
                    targetPackSlot.packSO = packSlotView.packSO;
                    targetPackSlot.root.Add(childToMove);
                    targetPackSlot.FakePointerLeaveEvent();
                    packSlotView.pack = null;
                    packSlotView.packSO = null;
                    break;
                }
            }
        }
        UpdateUIState();
    }

    private void UpdateUIState()
    {
        packsSelectedLabel.text = $"{selectedPackSOs.Count}/5 Selected";
        selectPacksButton.SetEnabled(selectedPackSOs.Count() == 5);
    }

    private void HandlePacksSelected()
    {
        if (ascensionsEnabled)
        {
            gameState.ascensionLevel = ascensionIndex;
        }
        else
        {
            gameState.ascensionLevel = -1;
        }
        Debug.Log("Ascensions enabled: " + ascensionsEnabled);
        selectPacksButton.SetEnabled(false);
        gameState.previouslySelectedPackSOs = selectedPackSOs;
        List<CompanionTypeSO> commonCompanions = new();
        List<CompanionTypeSO> uncommonCompanions = new();
        List<CompanionTypeSO> rareCompanions = new();
        foreach (PackSO packSO in selectedPackSOs)
        {
            commonCompanions.AddRange(packSO.companionPoolSO.commonCompanions);
            uncommonCompanions.AddRange(packSO.companionPoolSO.uncommonCompanions);
            rareCompanions.AddRange(packSO.companionPoolSO.rareCompanions);
        }
        gameState.baseShopData.companionPool.commonCompanions = commonCompanions;
        gameState.baseShopData.companionPool.uncommonCompanions = uncommonCompanions;
        gameState.baseShopData.companionPool.rareCompanions = rareCompanions;
        gameState.baseShopData.activePacks = selectedPackSOs;
        gameState.LoadNextLocation();
        // progress to the next scene probably
    }
}