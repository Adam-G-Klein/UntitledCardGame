using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionInstance : MonoBehaviour, Entity
{
    public Companion companion;
    [Space(10)]
    public SpriteRenderer spriteRenderer;
    [SerializeField]
    private CardsDealtEvent cardsDealtEvent;
    [SerializeField]
    private CompanionInstantiatedEvent companionInstantiatedEvent;
    [SerializeField]
    private GameplayConstants constants;


    void Start()
    {
        this.spriteRenderer.sprite = companion.companionType.sprite;
        // Tried doing this in Awake, but it looks like the fields of companion
        // hadn't been initialized by then
        companionInstantiatedEvent.Raise(new CompanionInstantiatedEventInfo(companion));
    }

    void Awake() {
    }

    void Update()
    {
    }


    public void dealCards(int numCards){
        List<CardInfo> cards = getCardsFromDeck(numCards);
        cardsDealtEvent.Raise(new CardsDealtEventInfo(cards));
    }

    public List<CardInfo> getCardsFromDeck(int numCards){
        List<CardInfo> returnList = new List<CardInfo>();
        CardInfo card;
        List<CardInfo> deckCards = companion.deck.cards;
        for(int i = 0; i < numCards; i++){
            //Drawing with replacement for now
            card = deckCards[Random.Range(0,deckCards.Count)];
            returnList.Add(card);
        }
        return returnList;
    }

    public void cardEffectEventHandler(CardEffectEventInfo info){
        if(!info.targets.Contains(companion.id)) return;
        switch(info.effectName) {
            case CardEffectName.Draw:
                dealCards(info.scale);
                break;
            case CardEffectName.Damage:
                // TODO: heal effect
                companion.currentHealth -= info.scale;
                break;
            case CardEffectName.Buff:
                companion.currentAttackDamage += info.scale; 
                break;
        }
    }

    public void enemyEffectEventHandler(EnemyEffectEventInfo info){
        if(!info.targets.Contains(companion.id)) return;
        switch(info.effectName) {
            case EnemyEffectName.Damage:
                companion.currentHealth -= info.scale;
                break;
            case EnemyEffectName.Buff: 
                // TODO: weaken effect
                companion.currentAttackDamage += info.scale; 
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
    public int getHealth(){
        return companion.currentHealth;
    }

    public int getMaxHealth() {
        return companion.companionType.maxHealth;
    }
    
}
