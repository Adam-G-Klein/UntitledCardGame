using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CardInHandSelectionView
{
    private UIDocument rootUiDoc;
    private VisualElement rootElement;
    private Label label;
    private VisualElement mainArea;
    public IconButton confirmButton;
    public IconButton cancelButton;
    private VisualElement cardCastLocationVisualElement;

    private int fadeTween = -1;
    private const float FadeTime = 0.3f;
    private Action onConfirmHandler = null;
    private Action onCancelHandler = null;
    private static List<Targetable.TargetType> PLAYABLE_CARD_TARGET = new List<Targetable.TargetType>() { Targetable.TargetType.Card };

    public CardInHandSelectionView(UIDocument rootUiDoc, VisualElement rootElement)
    {
        this.rootUiDoc = rootUiDoc;
        this.rootElement = rootElement;
        this.label = rootElement.Q<Label>("card-in-harnd-selection-label");
        this.mainArea = rootElement.Q<VisualElement>("card-in-hand-selection-area");
        this.confirmButton = rootElement.Q<IconButton>("card-in-hand-selection-button");
        this.cancelButton = rootElement.Q<IconButton>("card-in-hand-cancel-button");
        this.cardCastLocationVisualElement = rootElement.Q<VisualElement>("card-in-hand-selection-card-cast-location");

        this.confirmButton.RegisterOnSelected(() => onConfirmHandler?.Invoke());
        this.cancelButton.RegisterOnSelected(() => onCancelHandler?.Invoke());
        FocusManager.Instance.RegisterFocusableTarget(this.confirmButton.AsFocusable());
        FocusManager.Instance.DisableFocusableTarget(this.confirmButton.AsFocusable());
        FocusManager.Instance.RegisterFocusableTarget(this.cancelButton.AsFocusable());
        FocusManager.Instance.DisableFocusableTarget(this.cancelButton.AsFocusable());
    }

    public void EnableSelection(string text, Action<GeometryChangedEvent> geoChanged, bool canCancel)
    {
        FocusManager.Instance.StashFocusablesNotOfTargetType(PLAYABLE_CARD_TARGET, this.GetType().Name);
        FocusManager.Instance.EnableFocusableTarget(this.confirmButton.AsFocusable());
        if (canCancel) {
            this.cancelButton.style.display = DisplayStyle.Flex;
            FocusManager.Instance.EnableFocusableTarget(this.cancelButton.AsFocusable());
            this.cancelButton.pickingMode = PickingMode.Position;
        }
        this.rootElement.style.display = DisplayStyle.Flex;
        // VFX: the selection's dark backdrop fades in (and out, in DisableSelection) rather than snapping the whole
        // board dark in one frame, which read as a flash when a draw card asks you to pick a card
        Fade(this.rootElement.resolvedStyle.opacity < 1f ? this.rootElement.resolvedStyle.opacity : 0f, 1f, null);
        this.label.text = text;
        this.confirmButton.pickingMode = PickingMode.Position;

        EventCallback<GeometryChangedEvent> geoChangedEvent = null;
        geoChangedEvent = (evt) =>
        {
            geoChanged(evt);
            this.rootElement.UnregisterCallback(geoChangedEvent);
        };
        this.rootElement.RegisterCallback(geoChangedEvent);
    }

    public void DisableSelection()
    {
        FocusManager.Instance.UnstashFocusables(this.GetType().Name);
        FocusManager.Instance.DisableFocusableTarget(this.confirmButton.AsFocusable());
        FocusManager.Instance.DisableFocusableTarget(this.cancelButton.AsFocusable());
        this.cancelButton.style.display = DisplayStyle.None;
        this.confirmButton.pickingMode = PickingMode.Ignore;
        // VFX: fades out, then hides (a new selection before then cancels the fade and fades back in)
        Fade(this.rootElement.resolvedStyle.opacity, 0f, () => this.rootElement.style.display = DisplayStyle.None);
    }

    private void Fade(float from, float to, Action onDone)
    {
        if (fadeTween >= 0) LeanTween.cancel(fadeTween);
        this.rootElement.style.opacity = from;
        LTDescr tween = LeanTween.value(from, to, FadeTime).setEaseInOutSine().setIgnoreTimeScale(true)
            .setOnUpdate(a => this.rootElement.style.opacity = a);
        if (onDone != null) tween.setOnComplete(onDone);
        fadeTween = tween.uniqueId;
    }

    public void UpdateLabelText(string text)
    {
        this.label.text = text;
    }

    public void SetConfirmedHandler(Action action)
    {
        onConfirmHandler = action;
    }

    public void RemoveConfirmedHandler(Action action)
    {
        onConfirmHandler = null;
    }

    public void EnableCancelHandler(Action action)
    {
        onCancelHandler = action;
    }

    public void RemoveCancelHandler()
    {
        onCancelHandler = null;
    }

    public VisualElement GetCardCastLocationElement()
    {
        return this.cardCastLocationVisualElement;
    }

    public Vector3 GetSplineStartpoint()
    {
        Vector3 uiPoint = new Vector3(mainArea.worldBound.xMin, mainArea.worldBound.center.y, 0);
        return UIDocumentGameObjectPlacer.GetWorldPositionFromUIDocumentPosition(uiPoint);
    }

    public Vector3 GetSplineEndpoint()
    {
        Vector3 uiPoint = new Vector3(mainArea.worldBound.xMax, mainArea.worldBound.center.y, 0);
        return UIDocumentGameObjectPlacer.GetWorldPositionFromUIDocumentPosition(uiPoint);
    }
}