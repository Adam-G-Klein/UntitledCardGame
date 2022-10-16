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
    //TODO remove this reference, 
    //publish events to bus instead of calling directly
    private PlayerHand hand;

    void Start()
    {
        this.spriteRenderer.sprite = companion.companionType.sprite;
        //TODO remove this reference, 
        //publish events to bus instead of calling directly
        hand = GameObject.Find("PlayerHand").GetComponent<PlayerHand>();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.D)){
            companionDealEventHandler(new DealCardEventInfo(Random.Range(0,3), companion.id));
        }
    }


    public void companionDealEventHandler(DealCardEventInfo info){
        if(!info.target.Equals(companion.id)) return;
        List<CardInfo> returnList = new List<CardInfo>();
        CardInfo card;
        List<CardInfo> deckCards = companion.deck.cards;
        for(int i = 0; i < info.scale; i++){
            //Drawing with replacement for now
            card = deckCards[Random.Range(0,deckCards.Count)];
            returnList.Add(card);
        }
        //TODO remove this reference, 
        //publish events to bus instead of calling directly
        hand.cardDealtEventHandler(new CardsDealtEventInfo(returnList));




    }
}
