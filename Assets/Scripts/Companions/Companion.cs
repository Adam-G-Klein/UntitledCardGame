using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Companion : Entity, ICombatStats, IDeckEntity, IUIEntity
{
    public CompanionTypeSO companionType;

    public Deck deck;
    public CombatStats combatStats;
    [Header("This is here because CompanionInstance doesn't currently exist in the shop")]
    public List<EffectWorkflow> onCombineAbilities = new List<EffectWorkflow>();

    public Companion(CompanionTypeSO companionType)
    {
        this.companionType = companionType;
        this.combatStats = new CombatStats(
                companionType.maxHealth);
        this.deck = new Deck(companionType);
        this.id = Id.newGuid();
        this.entityType = EntityType.CompanionInstance;
        setupOnCombineAbilities(companionType);
    }

    private void setupOnCombineAbilities(CompanionTypeSO companionType) {
        foreach(EntityAbility ability in companionType.abilitiesV2) {
            if (ability.abilityTrigger == EntityAbility.EntityAbilityTrigger.OnCombine) {
                onCombineAbilities.Add(ability.effectWorkflow);
            }
        }
    }

    public Sprite getSprite() {
        return this.companionType.sprite;
    }

    public Deck getDeck() {
        return this.deck;
    }

    public int getDealtPerTurn() {
        return this.deck.cardsDealtPerTurn;
    }

    public void setDealtPerTurn(int dealtPerTurn) {
        this.deck.cardsDealtPerTurn = dealtPerTurn;
    }

    public void Upgrade(List<Companion> companionsCombined) {

        // if this upgrade initiated from combining companions go ahead and replace the current deck with a superset of all the other companions
        if (companionsCombined != default && (companionsCombined.Count > 0)) {
            deck.cards.Clear();
            foreach (var companion in companionsCombined) {
                deck.AddCards(companion.deck);
            }
        }

        //reset health to the new max
        this.combatStats.currentHealth = this.combatStats.maxHealth;
        // temp
        this.combatStats.maxHealth += 40;
        this.deck.cardsDealtPerTurn += 1;
    }

    public CombatStats GetCombatStats()
    {
        return this.combatStats;
    }

    public void InvokeOnCombineAbilities(GameStateVariableSO gameState) {
        if (onCombineAbilities == null || onCombineAbilities.Count < 1) {
            return;
        }
        if (onCombineAbilities.Count > 1) {
            Debug.Log("Companion " + this.companionType.name + " should only have 1 on combine ability, instead it has multiple.");
        }
        EffectDocument document = new();
        document.map.AddItem(EffectDocument.ORIGIN, this);
        document.originEntityType = EntityType.Companion;
        // Add the game state to the map so that the manual on combine workflows can
        // edit game state.
        document.map.AddItem("gameState", gameState);
        EffectManager.Instance.invokeEffectWorkflow(document, onCombineAbilities[0], null);
    }

    public string GetName()
    {
        return this.companionType.companionName;
    }

    public int GetCurrentHealth()
    {
        return this.combatStats.currentHealth;
    }

    public string GetDescription()
    {
        return this.companionType.keepsakeDescription;
    }

    public CombatInstance GetCombatInstance()
    {
        return null;
    }

    public EnemyInstance GetEnemyInstance()
    {
        return null;
    }

    public DeckInstance GetDeckInstance()
    {
        return null;
    }
}
