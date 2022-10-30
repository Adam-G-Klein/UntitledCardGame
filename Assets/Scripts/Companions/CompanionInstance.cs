using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//stubbing this here for now for testing purposes
public class DealCardEventInfo {
    public int scale;
    public string target;
    public DealCardEventInfo(int scale, string target){
        this.scale = scale;
        this.target = target;
    }
}
public class CompanionInstance : MonoBehaviour
{
    public Companion companion;
    [Space(10)]
    public SpriteRenderer spriteRenderer;
    [SerializeField]
    private CardsDealtEvent cardsDealtEvent;
    [SerializeField]
    private CompanionInstantiatedEvent companionInstantiatedEvent;


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


    public void companionDealEventHandler(DealCardEventInfo info){
        if(!info.target.Equals(companion.id)) return;
        List<CardInfo> cards = getCardsFromDeck(info.scale);
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
                List<CardInfo> cards = getCardsFromDeck(info.scale);
                cardsDealtEvent.Raise(new CardsDealtEventInfo(cards));
                break;
            case CardEffectName.Damage:
                companion.currentHealth -= info.scale;
                break;
            case CardEffectName.Buff:
                companion.currentAttackDamage += info.scale; 
                break;
        }
    }
    
}
