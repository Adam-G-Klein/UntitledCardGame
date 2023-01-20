using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionInstance : CombatEntityInstance
{
    public Companion companion;
    [Space(10)]
    [SerializeField]
    private CardsDealtEvent cardsDealtEvent;
    [SerializeField]
    private GameplayConstants constants;

    public InCombatDeck inCombatDeck;

    protected override void Start()
    {
        base.Start();
        this.baseStats = companion;
        this.id = companion.id; // Crucial step to make sure we don't end up with different IDs for this entity
        stats = new CombatEntityInEncounterStats(companion);
        inCombatDeck = new InCombatDeck(companion.deck);
        // Tried doing this in Awake, but it looks like the fields of companion
        // hadn't been initialized by then
    }

    void Awake() {
    }

    void Update()
    {
    }


    public void dealCards(int numCards){
        List<Card> cards = inCombatDeck.dealCardsFromDeck(numCards);
        StartCoroutine(cardsDealtEvent.RaiseAtEndOfFrameCoroutine(new CardsDealtEventInfo(cards, inCombatDeck, stats)));
    }

    public override bool isTargetableByChildImpl(EffectTargetRequestEventInfo eventInfo)
    {
        return inCombatDeck.drawPile.Count > 0 
            || inCombatDeck.discardPile.Count > 0;
    }

    public void cardEffectEventHandler(CardEffectEventInfo info){
        if(!info.targets.Contains(this)) return;
        switch(info.effectName) {
            case SimpleEffectName.Draw:
                dealCards(info.scale);
                break;
            case SimpleEffectName.Damage:
                // TODO: heal effect
                stats.currentHealth -= info.scale;
                break;
            case SimpleEffectName.Buff:
                base.applyStatusEffect(StatusEffect.Strength, info.scale);
                break;
            case SimpleEffectName.Weaken:
                base.applyStatusEffect(StatusEffect.Weakness, info.scale);
                break;
            case SimpleEffectName.Discard:
                Debug.LogWarning("Oh god a companion is getting discarded what happened");
                break;
        }
    }

    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info){
        switch(info.newPhase){
            case TurnPhase.START_PLAYER_TURN:
                // Don't see too much reason to implement this differently yet,
                // but maybe turn start draws will be different at some point
                dealCards(constants.START_TURN_DRAW_PER_COMPANION);
                break;
        }
    }
    
    public override CombatEntityInEncounterStats getCombatEntityInEncounterStats() {
        return stats;
    }
}

