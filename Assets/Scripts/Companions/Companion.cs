using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Companion : Entity, ICombatStats, IDeckEntity 
{
    public CompanionTypeSO companionType;

    public Deck deck;
    public CombatStats combatStats;
    public int companionLevel = 1;
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
        foreach(CompanionAbility ability in companionType.abilities) {
            if (ability.companionAbilityTrigger == CompanionAbility.CompanionAbilityTrigger.OnCombine) {
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
        this.companionLevel += 1;
    }

    public CombatStats GetCombatStats()
    {
        return this.combatStats;
    }

    public void InvokeOnCombineAbilities() {
        if(onCombineAbilities != null && onCombineAbilities.Count > 0) {
            EffectDocument document = new EffectDocument();
            document.map.AddItem(EffectDocument.ORIGIN, this);
            document.originEntityType = EntityType.Companion;
            EffectManager.Instance.invokeEffectWorkflow(document, onCombineAbilities[0], null);
        }
        foreach(EffectWorkflow effectWorkflow in onCombineAbilities.GetRange(1, onCombineAbilities.Count - 1)) {
            EffectDocument document = new EffectDocument();
            document.map.AddItem(EffectDocument.ORIGIN, this);
            document.originEntityType = EntityType.Companion;
            EffectManager.Instance.QueueEffectWorkflow(effectWorkflow);
        }
    }
}
