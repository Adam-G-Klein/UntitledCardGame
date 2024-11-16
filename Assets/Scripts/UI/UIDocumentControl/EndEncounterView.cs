using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class EndEncounterView : MonoBehaviour
{
    private UIDocument doc;
    private UIDocumentScreenspace docRenderer;

    private Material mat;

    [SerializeField]
    private float fadeTime = 0.5f;


    void OnEnable()
    {
        doc = GetComponent<UIDocument>();
        docRenderer = GetComponent<UIDocumentScreenspace>();
        mat = GetComponent<RawImage>().material;
        mat.SetFloat("_alpha", 0);
    }

    public void Setup(int baseGoldEarnedPerBattle, int interestEarned, int interestCap, float interestPercentage)
    {
        doc.rootVisualElement.Q<Label>("base-gold").text = "Base Gold Earned: " + baseGoldEarnedPerBattle.ToString();
        doc.rootVisualElement.Q<Label>("interest").text = "Interest: " + interestEarned.ToString();
        doc.rootVisualElement.Q<Label>("interest-help").text = "(You earn " +
            interestPercentage.ToString("P0") + 
            " of your current Gold as Interest, capped at " +
            interestCap.ToString() + " gold per combat)";
    }

    public void Show() {
        LeanTween.value(gameObject, 0, 1, fadeTime)
            .setOnUpdate((float val) => {
                mat.SetFloat("_alpha", val);
            });
    }


}
