using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EntityView {
    public VisualElement entityContainer;

    public static string STATUS_EFFECTS_CONTAINER_SUFFIX = "-status-effects";

    public EntityView(IUIEntity entity, int index, bool isEnemy) {
        entityContainer = setupEntity(entity, index, isEnemy);
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
        detailsContainer.AddToClassList("pillar-details");

        var titleContainer = new VisualElement();
        titleContainer.AddToClassList("pillar-name");
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
}