using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class VictoryView : MonoBehaviour
{
    private UIDocument doc;
    private UIDocumentScreenspace docRenderer;

    private Material mat;
    private CanvasGroup canvasGroup;

    [SerializeField]
    private float fadeTime = .1f;


    void OnEnable()
    {
        doc = GetComponent<UIDocument>();
        docRenderer = GetComponent<UIDocumentScreenspace>();
        // Set initial alpha to 0
        mat = GetComponent<RawImage>().material;
        mat.SetFloat("_alpha", 0);

        // Ensure the CanvasGroup alpha is set to 0 and blocks raycasts
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0;
    }

    public void Setup(List<Companion> companions)
    {
        
    }

    public void Show() {
        // Ensure the initial alpha is set to 0 before starting the fade-in
        canvasGroup.blocksRaycasts = true;
        mat.SetFloat("_alpha", 0);
        canvasGroup.alpha = 0;
        LeanTween.value(gameObject, 0, 1, fadeTime)
            .setOnUpdate((float val) => {
                mat.SetFloat("_alpha", val);
                canvasGroup.alpha = val;
            });
    }


}
