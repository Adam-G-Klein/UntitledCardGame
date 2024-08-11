using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System;

public class CombatEncounterView : MonoBehaviour
{
    public GameStateVariableSO gameState;
    private VisualElement root;

    private List<Companion> companions;
    UIDocumentScreenspace docRenderer;


    private void OnEnable()
    {
        docRenderer = gameObject.GetComponent<UIDocumentScreenspace>();
        root = GetComponent<UIDocument>().rootVisualElement;
        var enemies = ((EnemyEncounter)gameState.activeEncounter.GetValue()).enemyList;
        companions = gameState.companions.activeCompanions;
        setupEntities(root.Q<VisualElement>("enemyContainer"), enemies.Cast<Entity>().ToList(), true);
        setupEntities(root.Q<VisualElement>("companionContainer"), companions.Cast<Entity>().ToList(), false);
        setupCards();

    }

    private void setupEntities(VisualElement container, List<Entity> entities, bool isEnemy) {
        // DAE use enh for loops and keep track of index like this cause they hate array syntax
        var index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        foreach (var entity in entities) {
            container.Add(setupEntity(entity, index, isEnemy));
            print("HI" + index);
            index++;
        }
    }

    private VisualElement setupEntity(Entity entity, int index, bool isEnemy) {
        var pillar = new VisualElement();
        pillar.AddToClassList("entity-pillar");

        var topTriangle = new VisualElement();
        topTriangle.AddToClassList("top-triangle");
        pillar.Add(topTriangle);

        var container = new VisualElement();
        container.AddToClassList("container");
        container.Add(setupCardContainer(entity, index, isEnemy));
        pillar.Add(container);
        
        return pillar;
    }

    private VisualElement setupCardContainer(Entity entity, int index, bool isEnemy) {
        var container = new VisualElement();
        container.name = entity.id;
        container.AddToClassList("pillar-container");

        setupTabs(container, entity);

        var portraitContainer = new VisualElement();
        var baseString = isEnemy ? UIDocumentGameObjectPlacer.ENEMY_UIDOC_ELEMENT_PREFIX : UIDocumentGameObjectPlacer.COMPANION_UIDOC_ELEMENT_PREFIX;
        portraitContainer.name = baseString + index.ToString();
        portraitContainer.AddToClassList("portrait-container");
        container.Add(portraitContainer);

        var detailsContainer = new VisualElement();
        detailsContainer.AddToClassList("pillar-details");

        var titleContainer = new VisualElement();
        titleContainer.AddToClassList("pillar-name");
        var titleLabel = new Label();
        titleLabel.AddToClassList("pillar-name");
        titleContainer.Add(titleLabel);
        detailsContainer.Add(titleContainer);
        var descContainer = new VisualElement();
        descContainer.AddToClassList("pillar-text");
        var descLabel = new Label();
        descLabel.AddToClassList("pillar-desc-label");
        descContainer.Add(descLabel);

        detailsContainer.Add(descContainer);
        if (entity.entityType == EntityType.CompanionInstance || entity.entityType == EntityType.Companion)
        {
            var companion = (Companion)entity;
            titleLabel.text = companion.companionType.companionName;
            descLabel.text = companion.companionType.keepsakeDescription;

        }
        else //to lazy to write and erorr case :)
        {
            var enemy = (Enemy)entity;
            titleLabel.text = "Spooky scary skelington";
            descLabel.text = "I DO THE DAMAGEEE! (CombatEncounterView)";//Fill out
        }

        container.Add(detailsContainer);

        return container;
    }

    private void setupTabs(VisualElement container, Entity entity) {
        var armor = new VisualElement();
        armor.AddToClassList("armorTab");
        var armorLabel = new Label();
        armorLabel.AddToClassList("pillar-tab-text");
        armorLabel.name = "armorText";

        armor.Add(armorLabel);
        container.Add(armor);

        var health = new VisualElement();
        health.AddToClassList("healthTab");
        var healthLabel = new Label();
        healthLabel.AddToClassList("pillar-tab-text");
        healthLabel.name = "healthText";
        health.Add(healthLabel);
        container.Add(health);

        var shield = new VisualElement();
        shield.AddToClassList("shield");
        var shieldLabel = new Label();
        shieldLabel.AddToClassList("pillar-tab-text");
        shieldLabel.name = "shieldText";
        shield.Add(shieldLabel);
        container.Add(shield);
        //TODO fill out relevant text;
        if (entity.entityType == EntityType.CompanionInstance || entity.entityType == EntityType.Companion)
        {
            var companion = (Companion)entity;
            armorLabel.text = "99";
            healthLabel.text = companion.combatStats.getCurrentHealth().ToString();
            shieldLabel.text = "99";

        }
        else
        {
            var enemy = (Enemy)entity;
            armorLabel.text = "91";
            healthLabel.text = enemy.combatStats.getCurrentHealth().ToString();
            shieldLabel.text = "92";
        }

    }


    private void setupCards()
    {
        var container = root.Q<VisualElement>("cardContainer");
        int index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        foreach (Companion companion in companions) {
            VisualElement cardContainer = new VisualElement();
            cardContainer.AddToClassList("companion-card-placer");
            container.Add(cardContainer);
            var card = new VisualElement();
            card.name = UIDocumentGameObjectPlacer.CARD_UIDOC_ELEMENT_PREFIX + index;
            card.AddToClassList("cardPlace");
            cardContainer.Add(card);
            index++;
        }
    }

    public void updateMana(int mana) {
        print("here" + mana);
        root.Q<Label>("manaCounter").text = mana.ToString();
        docRenderer.SetStateDirty();
    }
}
