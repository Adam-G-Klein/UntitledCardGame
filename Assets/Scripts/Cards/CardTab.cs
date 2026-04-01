using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CardTab : MonoBehaviour
{
    [SerializeField] private UIDocument uiDoc;
    [SerializeField] private RawImage rawImage;
    [SerializeField] private PanelSettings panelSettingsTemplate;
    [SerializeField] private RenderTexture renderTextureBase;

    private CompanionInstance companionInstance;

    void Awake() {
        uiDoc.panelSettings = Instantiate(panelSettingsTemplate);
        uiDoc.panelSettings.targetTexture = new RenderTexture(renderTextureBase.descriptor);
        rawImage.texture = uiDoc.panelSettings.targetTexture;
    }

    public void Init(CompanionInstance companionInstance) {
        this.companionInstance = companionInstance;
    }

    public void Show() {
        rawImage.enabled = true;
    }

    public void Hide() {
        rawImage.enabled = false;
    }
}
