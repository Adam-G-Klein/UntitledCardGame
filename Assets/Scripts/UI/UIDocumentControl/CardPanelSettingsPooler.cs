using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class ReusablePanel {
    public PanelSettings panelSettings;
    public bool inUse;

    public ReusablePanel(PanelSettings panelSettings, bool inUse) {
        this.panelSettings = panelSettings;
        this.inUse = inUse;
    }
}
public class CardPanelSettingsPooler : GenericSingleton<CardPanelSettingsPooler> {
    [SerializeField]
    public List<PanelSettings> panelSettingsPool = new List<PanelSettings>();

    private List<ReusablePanel> reusablePanels = new List<ReusablePanel>();

    void Start() {
        foreach(PanelSettings panelSettings in panelSettingsPool) {
            if(panelSettings.targetTexture == null) {
                Debug.LogError("Panel settings target texture is null! Fix your panel settings!");
                continue;
            }
            panelSettings.referenceResolution = UIDocumentCard.CARD_REFERENCE_RESOLUTION;
            panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            reusablePanels.Add(new ReusablePanel(panelSettings, false));
        }
    }

    public PanelSettings GetPanelSettings() {
        foreach(ReusablePanel reusablePanel in reusablePanels) {
            Debug.Log("Checking reusable panel: " + reusablePanel.panelSettings.name + " in use: " + reusablePanel.inUse);
            if (!reusablePanel.inUse) {
                reusablePanel.inUse = true;
                return reusablePanel.panelSettings;
            }
        }
        Debug.LogError("No reusable panels available! Add more to the list or fix your code :)");
        return null;
    }

    public void ReturnPanelSettings(PanelSettings panelSettings) {
        foreach(ReusablePanel reusablePanel in reusablePanels) {
            if (reusablePanel.panelSettings == panelSettings) {
                reusablePanel.inUse = false;
                return;
            }
        }
    }
}