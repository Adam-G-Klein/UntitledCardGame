using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CompanionView
{
    private static float CONTAINER_ASPECT_RATIO = 1.25f;
    private static float SCREEN_WIDTH_PERCENT = 0.20f;

    public VisualElement container;
    private IUIEntity entity;
    private IEntityViewDelegate viewDelegate;
    private VisualTreeAsset template;

    private VisualElement parentContainer;
    private VisualElement statusContainer;
    private VisualElement solidBackground;
    private VisualElement imageElement;
    private Label primaryNameLabel;
    private Label secondaryNameLabel;
    private Label blockLabel;
    private Label healthLabel;

    public CompanionView(
            IUIEntity entity,
            VisualTreeAsset template,
            IEntityViewDelegate viewDelegate = null,
            bool isCompanionManagementView = false) {
        this.entity = entity;
        this.viewDelegate = viewDelegate;
        this.template = template;
        
        this.container = SetupCompanionView();
    }

    private VisualElement SetupCompanionView() {
        VisualElement companionRoot = this.template.CloneTree();

        this.parentContainer = companionRoot.Q<VisualElement>("companion-view-parent-container");
        this.statusContainer = companionRoot.Q<VisualElement>("companion-view-status-container");
        this.solidBackground = companionRoot.Q<VisualElement>("companion-view-solid-background");
        this.imageElement = companionRoot.Q<VisualElement>("companion-view-companion-image");
        this.primaryNameLabel = companionRoot.Q<Label>("companion-view-primary-name-label");
        this.secondaryNameLabel = companionRoot.Q<Label>("companion-view-secondary-name-label");
        this.blockLabel = companionRoot.Q<Label>("companion-view-block-value-label");
        this.healthLabel = companionRoot.Q<Label>("companion-view-health-value-label");

        Sprite sprite = null;
        if (entity is Companion companion) {
            sprite = companion.companionType.sprite;
        } else if (entity is CompanionInstance companionInstance) {
            sprite = companionInstance.companion.companionType.sprite;
        }
        this.imageElement.style.backgroundImage = new StyleBackground(sprite);

        this.primaryNameLabel.text = entity.GetName();
        this.secondaryNameLabel.text = ""; // TODO: Do this lmao

        // Moving past the random VisualElement parent CloneTree() creates
        VisualElement container = companionRoot.Children().First();

        UpdateWidthAndHeight(container);

        return container;
    }

    public void UpdateWidthAndHeight(VisualElement root) {
        Tuple<int, int> entityWidthHeight = GetWidthAndHeight();
        root.style.width = entityWidthHeight.Item1;
        root.style.height = entityWidthHeight.Item2;
    }

    private Tuple<int, int> GetWidthAndHeight() {
        int width = (int)(Screen.width * SCREEN_WIDTH_PERCENT);
        int height = (int)(width / CONTAINER_ASPECT_RATIO);

        // This drove me insane btw
        #if UNITY_EDITOR
        UnityEditor.PlayModeWindow.GetRenderingResolution(out uint windowWidth, out uint windowHeight);
        width = (int)(windowWidth * SCREEN_WIDTH_PERCENT);
        height = (int)(width / CONTAINER_ASPECT_RATIO);
        #endif

        return new Tuple<int, int>(width, height);
    }
}
