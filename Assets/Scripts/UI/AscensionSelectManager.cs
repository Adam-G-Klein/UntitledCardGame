using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AscensionSelectManager : MonoBehaviour
{
    [SerializeField]
    private GameStateVariableSO gameState;
    [SerializeField]
    private UIDocument ascensionSelectUIDocument;
    [SerializeField]
    private float disabledArrowOpacity = 0.3f;
    [Header("Intro dialogue - every line but the last waits for a click, the last stays on screen")]
    [SerializeField]
    private Sprite introSpeakerSprite;
    [SerializeField]
    private List<string> introDialogueLines = new List<string> { "What stake would you like to play at today?" };

    private Button startButton;
    private Button leftArrow;
    private Button rightArrow;
    private Label stakeNumberLabel;
    private Label stakeDescriptionLabel;

    private List<AscensionSO> ascensions;
    private int selectedIndex = 0;
    private int maxSelectableIndex = 0;

    void Start()
    {
        gameState.currentLocation = Location.ASCENSION_SELECT;
        StartCoroutine(SetupWhenReady());
    }

    private IEnumerator SetupWhenReady()
    {
        yield return new WaitUntil(() =>
            UIDocumentUtils.ElementIsReady(ascensionSelectUIDocument.rootVisualElement)
            && ProgressManager.Instance != null
            && ProgressManager.Instance.ascensionInfo != null);
        Setup();
    }

    private void Setup()
    {
        VisualElement root = ascensionSelectUIDocument.rootVisualElement;
        startButton = root.Q<Button>("startButton");
        leftArrow = root.Q<Button>("leftArrow");
        rightArrow = root.Q<Button>("rightArrow");
        stakeNumberLabel = root.Q<Label>("stakeNumber");
        stakeDescriptionLabel = root.Q<Label>("stakeDescription");

        AscensionInfo ascensionInfo = ProgressManager.Instance.ascensionInfo;
        ascensions = ascensionInfo.ascensionSOList;
        maxSelectableIndex = Mathf.Clamp(ascensionInfo.playersMaxAscensionUnlocked, 0, ascensions.Count - 1);
        selectedIndex = Mathf.Clamp(gameState.ascensionLevel, 0, maxSelectableIndex);

        leftArrow.RegisterOnSelected(() => ChangeSelectedAscension(-1));
        rightArrow.RegisterOnSelected(() => ChangeSelectedAscension(1));
        startButton.RegisterOnSelected(StartButtonHandler);

        FocusManager.Instance.RegisterFocusables(ascensionSelectUIDocument);

        UpdateDisplay();
        StartCoroutine(IntroDialogue());
    }

    private IEnumerator IntroDialogue()
    {
        for (int i = 0; i < introDialogueLines.Count; i++)
        {
            bool isLastLine = i == introDialogueLines.Count - 1;
            yield return DialogueView.Instance.SpeakLineCoroutine(introSpeakerSprite, introDialogueLines[i], !isLastLine);
        }
    }

    private void ChangeSelectedAscension(int change)
    {
        int newIndex = Mathf.Clamp(selectedIndex + change, 0, maxSelectableIndex);
        if (newIndex == selectedIndex) return;

        selectedIndex = newIndex;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        stakeNumberLabel.text = $"Stake {selectedIndex}";
        stakeDescriptionLabel.text = ascensions[selectedIndex].description;
        leftArrow.style.opacity = selectedIndex > 0 ? 1f : disabledArrowOpacity;
        rightArrow.style.opacity = selectedIndex < maxSelectableIndex ? 1f : disabledArrowOpacity;
    }

    private void StartButtonHandler()
    {
        startButton.SetEnabled(false);
        gameState.ascensionLevel = selectedIndex;
        gameState.LoadNextLocation();
    }
}
