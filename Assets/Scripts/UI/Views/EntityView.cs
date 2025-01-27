using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EntityView {
    public VisualElement entityContainer;

    public static string STATUS_EFFECTS_CONTAINER_SUFFIX = "-status-effects";

    private float SCREEN_WIDTH_PERCENT = 0.11f;
    private float RATIO = 1.4f;

    public EntityView(IUIEntity entity, int index, bool isEnemy) {
        entityContainer = setupEntity(entity, index, isEnemy);
    }

    public void SetupEntityImage(Sprite sprite) {
        VisualElement portraitContainer = entityContainer.Q(className: "portrait-container");
        portraitContainer.style.backgroundImage = new StyleBackground(sprite);
    }

    public void HideDescription() {
        entityContainer.Q("description-container").RemoveFromHierarchy();
        VisualElement detailsContainer = entityContainer.Q("details-container");
        detailsContainer.AddToClassList("pillar-details-no-description");
        detailsContainer.RemoveFromClassList("pillar-details");
        VisualElement titleContainer = entityContainer.Q("title-container");
        titleContainer.RemoveFromClassList("pillar-name");
        titleContainer.AddToClassList("pillar-name-no-description");
    }

    private VisualElement setupEntity(IUIEntity entity, int index, bool isEnemy) {
        var pillar = new VisualElement();
        pillar.AddToClassList("entity-pillar");

        var topTriangle = new VisualElement();
        topTriangle.AddToClassList("top-triangle");
        pillar.Add(topTriangle);

        var pillarContainer = new VisualElement();
        pillarContainer.AddToClassList("pillar-container");
        pillarContainer.Add(setupCardColumn(entity, index, isEnemy));

        var leftColumn = new VisualElement();
        leftColumn.AddToClassList("pillar-left-column");
        leftColumn.Add(setupHealthAndBlockTabs(entity));
        pillarContainer.Add(leftColumn);

        var rightColumn = new VisualElement();
        rightColumn.AddToClassList("pillar-right-column");
        pillarContainer.Add(rightColumn);

        pillar.Add(pillarContainer);

        Tuple<int, int> entityWidthHeight = GetWidthAndHeight();
        pillar.style.width = entityWidthHeight.Item1;
        pillar.style.height = entityWidthHeight.Item2;
        
        return pillar;
    }

    private VisualElement setupCardColumn(IUIEntity entity, int index, bool isEnemy) {
        var column = new VisualElement();
        column.name = entity.GetName();
        column.AddToClassList("pillar-card-column");

        VisualElement detailsContainer = setupCardColumnPortraitAndTitle(column, entity, index, isEnemy);
        VisualElement descriptionContainer = setupCardColumnDescription(entity);
        
        detailsContainer.Add(descriptionContainer);

        column.Add(detailsContainer);

        return column;
    }

    // returns the details container, which holds everything below the portrait
    private VisualElement setupCardColumnPortraitAndTitle(VisualElement column, IUIEntity entity, int index, bool isEnemy) {
        var portraitContainer = new VisualElement();
        var baseString = isEnemy ? UIDocumentGameObjectPlacer.ENEMY_UIDOC_ELEMENT_PREFIX : UIDocumentGameObjectPlacer.COMPANION_UIDOC_ELEMENT_PREFIX;
        string portraitContainerName = baseString + index.ToString();
        portraitContainer.name = portraitContainerName;
        portraitContainer.AddToClassList("portrait-container");
        column.Add(portraitContainer);
        column.AddToClassList(portraitContainer.name + STATUS_EFFECTS_CONTAINER_SUFFIX);

        var detailsContainer = new VisualElement();
        detailsContainer.name = "details-container";
        detailsContainer.AddToClassList("pillar-details");

        var titleContainer = new VisualElement();
        titleContainer.AddToClassList("pillar-name");
        titleContainer.name = "title-container";
        var titleLabel = new Label();
        titleLabel.AddToClassList("pillar-name");
        titleContainer.Add(titleLabel);
        titleLabel.text = entity.GetName(); 
        detailsContainer.Add(titleContainer);
        return detailsContainer;
    }

    // returns the description container, which holds the enemy intent, the companion description, and the 
    // deck drawers on hover for companions
    private VisualElement setupCardColumnDescription(IUIEntity entity) {
        var descContainer = new VisualElement();
        descContainer.name = "description-container";
        descContainer.AddToClassList("pillar-text");

        var descLabel = new Label();

        descLabel.AddToClassList("pillar-desc-label");
        descLabel.text = entity.GetDescription();
        descContainer.Add(descLabel);

        return descContainer;
    }

    private VisualElement setupHealthAndBlockTabs(IUIEntity entityInstance) {
        var tabContainer = new VisualElement();
        tabContainer.AddToClassList("pillar-tab-container");
        CombatStats stats = entityInstance.GetCombatStats();

        var healthTab = new VisualElement();
        healthTab.AddToClassList("health-tab");
        var healthLabel = new Label();
        healthLabel.AddToClassList("pillar-tab-text");
        healthLabel.text = stats.getCurrentHealth().ToString();
        healthTab.Add(healthLabel);
        tabContainer.Add(healthTab);

        CombatInstance combatInstance = entityInstance.GetCombatInstance();

        if(combatInstance && combatInstance.GetStatus(StatusEffectType.Defended) > 0) {
            var blockContainer = new VisualElement();
            blockContainer.AddToClassList("block-tab");
            var blockLabel = new Label();
            blockLabel.AddToClassList("pillar-tab-text");
            blockLabel.text = combatInstance.GetStatus(StatusEffectType.Defended).ToString();
            blockContainer.Add(blockLabel);
            tabContainer.Add(blockContainer);
        }

        return tabContainer;
    }

    private Tuple<int, int> GetWidthAndHeight() {
        int width = (int)(Screen.width * SCREEN_WIDTH_PERCENT);
        int height = (int)(width * RATIO);

        // This drove me insane btw
        #if UNITY_EDITOR
        UnityEditor.PlayModeWindow.GetRenderingResolution(out uint windowWidth, out uint windowHeight);
        width = (int)(windowWidth * SCREEN_WIDTH_PERCENT);
        height = (int)(width * RATIO);
        #endif

        return new Tuple<int, int>(width, height);
    }
}