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

    public static string DETAILS_CONTAINER_SUFFIX = "-details";
    public static string DETAILS_HEADER_SUFFIX = "-details-title";
    public static string DETAILS_DESCRIPTION_SUFFIX = "-details-desc";

    [Header("Needs its own reference because the singleton isn't alive in time")]
    public GameplayConstantsSO gameplayConstants;
    public static string STATUS_EFFECTS_CONTAINER_SUFFIX = "-status-effects";
    public static string STATUS_EFFECTS_TAB_CLASSNAME = "status-effect";

    private void OnEnable()
    {
        docRenderer = gameObject.GetComponent<UIDocumentScreenspace>();
        root = GetComponent<UIDocument>().rootVisualElement;
        if(!gameState.activeEncounter.GetValue().GetType().Equals(typeof(EnemyEncounter))) {
            Debug.LogError("Active encounter is not an EnemyEncounter, Go to ScriptableObjects/Variables/GameState.so and hit Set Active Encounter to set the encounter to an enemy encounter");
            return;
        }
        var enemies = ((EnemyEncounter)gameState.activeEncounter.GetValue()).enemyList;
        companions = gameState.companions.activeCompanions;
        setupEntities(root.Q<VisualElement>("enemyContainer"), enemies.Cast<Entity>().ToList(), true);
        setupEntities(root.Q<VisualElement>("companionContainer"), companions.Cast<Entity>().ToList(), false);
        setupCards();
        EnemyEncounterViewModel.Instance.SetListener(this);
    }

    public void UpdateView() {
        // get all the shit you need from EnemyEncounterViewModel.Instance

    }

    private void setupEntities(VisualElement container, List<Entity> entities, bool isEnemy) {
        // DAE use enh for loops and keep track of index like this cause they hate array syntax
        var index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        foreach (var entity in entities) {
            container.Add(setupEntity(entity, index, isEnemy));
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
        container.AddToClassList(portraitContainer.name + STATUS_EFFECTS_CONTAINER_SUFFIX);

        var detailsContainer = new VisualElement();
        // TODO: figure out how to avoid querying from root. All the elements we want to query need to have 
        // unique names from root for now. 
        detailsContainer.AddToClassList(portraitContainer.name + DETAILS_CONTAINER_SUFFIX);

        var titleContainer = new VisualElement();
        titleContainer.AddToClassList("pillar-name");
        var titleLabel = new Label();
        titleLabel.AddToClassList(portraitContainer.name + DETAILS_HEADER_SUFFIX);
        titleContainer.Add(titleLabel);
        detailsContainer.Add(titleContainer);
        var descContainer = new VisualElement();
        descContainer.AddToClassList("pillar-text");
        var descLabel = new Label();
        descLabel.AddToClassList(portraitContainer.name + DETAILS_DESCRIPTION_SUFFIX);
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
        int maxHandSize = gameplayConstants.MAX_HAND_SIZE;
        for (int i = 0; i < maxHandSize; i++) {
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
        root.Q<Label>("manaCounter").text = mana.ToString();
        docRenderer.SetStateDirty();
    }
}
