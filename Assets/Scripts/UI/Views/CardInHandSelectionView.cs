using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CardInHandSelectionView {
    private UIDocument rootUiDoc;
    private VisualElement rootElement;
    private Label label;
    private VisualElement mainArea;
    private IconButton confirmButton;
    private VisualElement cardCastLocationVisualElement;

    private Action onConfirmHandler = null;
    private static List<Targetable.TargetType> PLAYABLE_CARD_TARGET = new List<Targetable.TargetType>() { Targetable.TargetType.Card };

    public CardInHandSelectionView(UIDocument rootUiDoc, VisualElement rootElement) {
        this.rootUiDoc = rootUiDoc;
        this.rootElement = rootElement;
        this.label = rootElement.Q<Label>("card-in-harnd-selection-label");
        this.mainArea = rootElement.Q<VisualElement>("card-in-hand-selection-area");
        this.confirmButton = rootElement.Q<IconButton>("card-in-hand-selection-button");
        this.cardCastLocationVisualElement = rootElement.Q<VisualElement>("card-in-hand-selection-card-cast-location");

        this.confirmButton.RegisterOnSelected(() => onConfirmHandler?.Invoke());
        FocusManager.Instance.RegisterFocusableTarget(this.confirmButton.AsFocusable());
        FocusManager.Instance.DisableFocusableTarget(this.confirmButton.AsFocusable());
        ControlsManager.Instance.RegisterIconChanger(this.confirmButton);
    }

    public void EnableSelection(string text, Action<GeometryChangedEvent> geoChanged) {
        FocusManager.Instance.StashFocusablesNotOfTargetType(PLAYABLE_CARD_TARGET, this.GetType().Name);
        FocusManager.Instance.EnableFocusableTarget(this.confirmButton.AsFocusable());
        this.rootElement.style.display = DisplayStyle.Flex;
        this.label.text = text;
        this.confirmButton.pickingMode = PickingMode.Position;

        EventCallback<GeometryChangedEvent> geoChangedEvent = null;
        geoChangedEvent = (evt) => {
            geoChanged(evt);
            this.rootElement.UnregisterCallback(geoChangedEvent);
        };
        this.rootElement.RegisterCallback(geoChangedEvent);
    }

    public void DisableSelection() {
        FocusManager.Instance.UnstashFocusables(this.GetType().Name);
        FocusManager.Instance.DisableFocusableTarget(this.confirmButton.AsFocusable());
        this.rootElement.style.display = DisplayStyle.None;
    }

    public void UpdateLabelText(string text) {
        this.label.text = text;
    }

    public void SetConfirmedHandler(Action action) {
        onConfirmHandler = action;
    }

    public void RemoveConfirmedHandler(Action action) {
        onConfirmHandler = null;
    }

    public VisualElement GetCardCastLocationElement() {
        return this.cardCastLocationVisualElement;
    }
}