using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDealtEventInfo {
    public List<CardInfo> cards;

    public CardDealtEventInfo(List<CardInfo> cards){
        this.cards = cards;
    }
}

[ExecuteInEditMode]
public class PlayerHand : MonoBehaviour
{
    public List<PlayableCard> cardsInHand;
    //Temporary method of placing cards until we 
    // wanna do math and have a hover-enlarge effect implemented
    private LocationStore cardLocStore; 

    [SerializeField]
    private GameObject cardPrefab;

    [Header("Values For PrototypeUI Card Placement")]
    [SerializeField]
    private int cardYCoord = -200;
    [SerializeField]
    private int startXCoord = -700;
    [SerializeField]
    private int cardSpacing = 20;

    void Awake() {
        cardLocStore = GameObject.FindGameObjectWithTag("CardLocationStore").GetComponent<LocationStore>();

    }

    //TODO: only do this when cardsInHand changes
    // Cards currently delete themselves from the list and destroy themselves
    // will change when we have an event bus
    void Update() {
        displayCards();

    }

    public void cardDealtEventHandler(CardDealtEventInfo info){
        PlayableCard newCard;
        foreach(CardInfo cardInfo in info.cards) {
            newCard = PrefabInstantiator.instantiateCard(cardPrefab, transform, cardInfo);
            cardsInHand.Add(newCard);
        }
        displayCards();
    }

    private void displayCards(){
        float xLoc = startXCoord;
        for(int i = 0; i < cardsInHand.Count; i++) {
            cardsInHand[i].transform.localPosition = new Vector2(xLoc, cardYCoord);
            xLoc += cardSpacing;
        }
    }
}