using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class UpdateTutorialUIDocAction : TutorialAction
{
    [SerializeField]
    private string firstLineOfText;

    [SerializeField]
    private string secondLineOfText;

    [SerializeField]
    private string thirdLineOfText;

    [SerializeField]
    private string tutorialIndex;
    [SerializeField]
    private string tutorialStepsCount = "3";

    [SerializeField]
    private string buttonText;

    [SerializeField]
    private UIDocument uiDocument;

    [SerializeField]
    private Sprite tutorialImage;
    [SerializeField]
    private Sprite backgroundImage;
    [SerializeField]
    private String tutorialTitle;

    public UpdateTutorialUIDocAction()
    {
        tutorialActionType = "Update Tutorial UI Doc Action";
    }

    public override IEnumerator Invoke()
    {
        var root = uiDocument.rootVisualElement;
        UpdateTutorialText(root.Q<Label>("tutorialText1"), firstLineOfText);
        UpdateTutorialText(root.Q<Label>("tutorialText2"), secondLineOfText);
        UpdateTutorialText(root.Q<Label>("tutorialText3"), thirdLineOfText);
        Label tutorialIndexText = root.Q<Label>("tutorialIndexText");
        tutorialIndexText.text = $"(Page {tutorialIndex} of {tutorialStepsCount})";

        VisualElement visualElement = root.Q<VisualElement>("tutorialImage");
        visualElement.style.backgroundImage = new StyleBackground(tutorialImage);

        if (backgroundImage != null)
        {
            VisualElement backgroundImageVE = root.Q<VisualElement>("backgroundImage");
            backgroundImageVE.style.backgroundImage = new StyleBackground(backgroundImage);
        }

        if (buttonText != null)
        {
            Button tutorialButton = root.Q<Button>("next");
            tutorialButton.text = buttonText;
        }
        if (tutorialTitle != "")
        {
            Label label = root.Q<Label>("tutorialTitle");
            label.text = tutorialTitle;
        }

        uiDocument.GetComponent<UIDocumentScreenspace>().SetStateDirty();

        //text1.style.display = DisplayStyle.None;
        //text1.style.display = DisplayStyle.Flex;

        yield return null;
    }

    public void UpdateTutorialText(Label label, String text) {
        if (String.IsNullOrEmpty(text.Trim()))
        {
            label.style.display = DisplayStyle.None;
        }
        else
        {
            label.style.display = DisplayStyle.Flex;
            label.text = text;
        }
    }
}
