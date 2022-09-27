using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; 

/*
Cards should be dealt to a hand canvas that's in the main UI
EventManager should ask for a card from a specific companion,
    which will then be dealt to the hand (will stub with keys for now)
Can handle card placement with location repository
Assuming  we're drawing with replacement for now, implementing w/o replacement
    should just involve atomically removing the card from the deck
Each companion needs a current deck ScriptableObject in addition to its
    StartingDeck
CompanionCreation causes the starting deck to be populated into the current deck

*/
[RequireComponent(typeof(CardDisplay))]
public class PlayableCard : MonoBehaviour
    , IPointerClickHandler 
    , IDragHandler
    , IPointerEnterHandler
    , IPointerExitHandler
{
    private CardInfo cardInfo;
    //TODO remove this reference, only here for testing purposes
    private PlayerHand hand; 

    // Start is called before the first frame update
    void Start()
    {
        cardInfo = GetComponent<CardDisplay>().cardInfo;
        //TODO remove this reference, only here for testing purposes
        hand = GameObject.Find("PlayerHand").GetComponent<PlayerHand>();
    }

    public void OnPointerClick(PointerEventData eventData) 
    {
        CardCastArguments args = new CardCastArguments(Random.Range(0,10));
        cardInfo.Cast(args);
        //TODO do this as a part of the PlayerHand script handling the cast event
        //Can also consider removing based on cardinfo.id
        hand.cardsInHand.Remove(this);
        //Card information should be stored in the associated scriptable object,
        // meaning that destroying the onscreen prefab should be a part of the casting process
        Destroy(gameObject); 
    }
    public void OnDrag(PointerEventData eventData)
    {
        print("I'm being dragged!");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // print("pointer enter!");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // print("pointer exit!");
    }

    /*
    private void Update()
    {
        // left click to Cast this card
        if (Input.GetMouseButtonDown(0))
        {
            CardCastArguments args = new CardCastArguments(Random.Range(0,10));
            cardInfo.Cast(args);
        }
    }
    */

    // Used when instantiating the card after Start has run
    // See PrefabInstantiator
    public void setCardInfo(CardInfo card){
        this.cardInfo = card;
    }

}
