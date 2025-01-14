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

    public UpdateTutorialUIDocAction() {
        tutorialActionType = "Update Tutorial UI Doc Action";
    }

    public override IEnumerator Invoke() {
        Debug.Log("updating tutorial doc");
        // update the text in the UI doc...can I even do that?
        var root = uiDocument.rootVisualElement;
        Label text1 = root.Q<Label>("tutorialText1");
        text1.text = firstLineOfText;
        //text1.MarkDirtyRepaint();
        Label text2 = root.Q<Label>("tutorialText2");
        text2.text = secondLineOfText;
        //text2.MarkDirtyRepaint();
        Label text3 = root.Q<Label>("tutorialText3");
        text3.text = thirdLineOfText; 
        //text3.MarkDirtyRepaint();
        //TODO: force the document to rerender or something like that I think
        Label tutorialIndexText = root.Q<Label>("tutorialIndexText");
        tutorialIndexText.text = $"(Page {tutorialIndex} of {tutorialStepsCount})"; 

        VisualElement visualElement = root.Q<VisualElement>("tutorialImage");
        visualElement.style.backgroundImage = new StyleBackground(tutorialImage); 

        if (buttonText != null) {
            Button tutorialButton = root.Q<Button>("next");
            tutorialButton.text = buttonText;
        }

        uiDocument.GetComponent<UIDocumentScreenspace>().SetStateDirty();

        //text1.style.display = DisplayStyle.None;
        //text1.style.display = DisplayStyle.Flex;

        Debug.Log($"Text was set to: {text1.text}");

        yield return null;
    }
}
