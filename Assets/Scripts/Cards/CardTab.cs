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

    private VisualElement background;
    private VisualElement icon;

    private RenderTexture rt;
    private PanelSettings ps;
    private CompanionInstance companionInstance;
    private Vector3 baseTabPosition;
    private Vector3 loweredTabPosition;

    void Awake() {
        baseTabPosition = uiDoc.transform.localPosition;
        loweredTabPosition = new Vector3(baseTabPosition.x, baseTabPosition.y - 4f, baseTabPosition.z);
        uiDoc.transform.localPosition = loweredTabPosition;
        ps = Instantiate(panelSettingsTemplate);
        uiDoc.panelSettings = ps;
        rt = new RenderTexture(renderTextureBase.descriptor);
        uiDoc.panelSettings.targetTexture = rt;
        rawImage.texture = rt;

        background = uiDoc.rootVisualElement.Q<VisualElement>("background");
        icon = uiDoc.rootVisualElement.Q<VisualElement>("icon");
    }

    void OnDestroy() {
        if (rt != null) {
            rt.Release();
            Destroy(rt);
            rt = null;
        }

        if (ps != null) {
            Destroy(ps);
            ps = null;
        }
    }

    public void Init(CompanionInstance companionInstance) {
        this.companionInstance = companionInstance;
        background.style.backgroundImage = new StyleBackground(companionInstance.companion.companionType.pack.cardTab);
        icon.style.backgroundImage = new StyleBackground(companionInstance.companion.companionType.icon);
    }

    public void Show() {
        rawImage.enabled = true;
        LeanTween.moveLocal(uiDoc.gameObject, baseTabPosition, 0.35f)
            .setEase(LeanTweenType.easeInOutQuad);
    }

    public void Hide() {
        LeanTween.moveLocal(uiDoc.gameObject, loweredTabPosition, 0.35f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() => rawImage.enabled = false);
    }
}
