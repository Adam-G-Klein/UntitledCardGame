using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Linq;
using FMODUnity;

public class PackSlotView
{
    public PackSO packSO;
    public VisualElement root;
    private VisualTreeAsset packTemplate;
    private bool display;
    private bool isSelectedView;
    private IPackSlotViewDelegate packSlotViewDelegate;
    public VisualElement pack = null;

    public PackSlotView(IPackSlotViewDelegate packSlotViewDelegate, VisualElement root, VisualTreeAsset packTemplate, bool display, bool isSelectedView, PackSO packSO = null)
    {
        this.root = root;
        this.packSO = packSO;
        this.packTemplate = packTemplate;
        this.display = display;
        this.isSelectedView = isSelectedView;
        this.packSlotViewDelegate = packSlotViewDelegate;

        SetupPackSlotView();
    }

    private void SetupPackSlotView()
    {
        root.pickingMode = PickingMode.Position;
        root.focusable = true;
        root.AddToClassList("focusable");

        root.RegisterOnSelected(() => PackSlotClickEvent(root));

        root.RegisterCallback<PointerEnterEvent>(PointerEnter);
        root.RegisterCallback<PointerLeaveEvent>(PointerLeave);

        VisualElementFocusable containerFocusable = root.AsFocusable();
        containerFocusable.additionalFocusAction += () => PointerEnter(root.CreateFakePointerEnterEvent());
        containerFocusable.additionalUnfocusAction += () => PointerLeave(root.CreateFakePointerLeaveEvent());
        FocusManager.Instance.RegisterFocusableTarget(containerFocusable);

        if (!display || !packSO) return;
        pack = CreateVisualElementFromTemplate();
        pack.name = packSO.packName;

        pack.AddToClassList("pack");
        var packNameLabel = pack.Q<Label>("packTitle");
        var packDescriptionLabel = pack.Q<Label>("packDescription");
        var darkOverlay = pack.Q<VisualElement>("darkOverlay");
        var button = pack.Q<Button>("submitButton");
        button.text = isSelectedView ? "Unselect" : "Select";

        if (packNameLabel != null)
        {
            packNameLabel.text = packSO.packName;
        }
        if (packDescriptionLabel != null)
        {
            packDescriptionLabel.text = packSO.packDescription;
        }
        root.Clear();
        root.Add(pack);
    }

    private void PointerEnter(PointerEnterEvent evt)
    {
        var target = evt.target as VisualElement;
        if (target.Children().Count() == 0) return;
        VisualElement darkOverlay = target.Q<VisualElement>("darkOverlay");
        darkOverlay.AddToClassList("darkOverlayHover");
        VisualElement button = target.Q<Button>("submitButton");
        button.AddToClassList("buttonHover");
    }

    private void PointerLeave(PointerLeaveEvent evt)
    {
        var target = evt.target as VisualElement;
        if (target.Children().Count() == 0) return;
        VisualElement darkOverlay = target.Q<VisualElement>("darkOverlay");
        darkOverlay.RemoveFromClassList("darkOverlayHover");
        VisualElement button = target.Q<Button>("submitButton");
        button.RemoveFromClassList("buttonHover");
    }

    private void PackSlotClickEvent(VisualElement VE)
    {
        if (pack == null) return;
        packSlotViewDelegate.PackSlotOnClick(this);
    }

    private VisualElement CreateVisualElementFromTemplate()
    {
        return packTemplate.CloneTree();
    }

    public void HandleSelect()
    {
        pack.Q<Button>("submitButton").text = "Unselect";
    }

    public void HandleUnselect()
    {
        pack.Q<Button>("submitButton").text = "Select";
    }

    public void FakePointerLeaveEvent()
    {
        PointerLeave(root.CreateFakePointerLeaveEvent());
    }
}
