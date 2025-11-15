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
        this.rootElement.style.display = DisplayStyle.None;
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