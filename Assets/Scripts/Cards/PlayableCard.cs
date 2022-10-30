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
    private EnemyManager enemyManager;
    private CompanionManager companionManager;

    // Start is called before the first frame update
    void Start()
    {
        cardInfo = GetComponent<CardDisplay>().cardInfo;
        //TODO remove this reference, only here for testing purposes
        hand = GameObject.Find("PlayerHand").GetComponent<PlayerHand>();
        GameObject enemyManagerGO = GameObject.Find("EnemyManager");
        GameObject companionManagerGO = GameObject.Find("CompanionManager");
        // My attempt at null safing. We should def talk about how we want to 
        // do this generally because it'll happen a lot with the modular scenes
        if(enemyManagerGO) enemyManager = enemyManagerGO.GetComponent<EnemyManager>();
        if(companionManagerGO) companionManager = companionManagerGO.GetComponent<CompanionManager>();
    }

    public void OnPointerClick(PointerEventData eventData) 
    {
        CardCastArguments args = new CardCastArguments(getCastTargets());
        cardInfo.Cast(args);
        //TODO do this as a part of the PlayerHand script handling the cast event
        //Can also consider removing based on cardinfo.id
        hand.cardsInHand.Remove(this);
        //Card information should be stored in the associated scriptable object,
        // meaning that destroying the onscreen prefab should be a part of the casting process
        Destroy(gameObject); 
    }


    private List<string> getCastTargets(){
        // Basically a stubbed function for now.
        // I think actual targeting is going to involve a state
        // machine in the event bus run by the UI, saying that the next click
        // is to target something for the given card, checking that each click is a valid target
        // before posting the cardWithTarget event, waiting for confirmation of target, and then
        // actually getting to this part where we have a card we're casting with its targets
        if(cardInfo.EffectsList[0].effectName == CardEffectName.Damage
            && enemyManager){
            return new List<string> { enemyManager.getRandomEnemyId() };
        }
        if(cardInfo.EffectsList[0].effectName == CardEffectName.Draw
            || cardInfo.EffectsList[0].effectName == CardEffectName.Buff
            && companionManager){
            return new List<string> { companionManager.getRandomCompanionId() };
        }
        else{
            return new List<string>();
        }

    }

    // Keeping these here for reference as they will almost certainly
    // be needed for UI effects in the future
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

    

    // Used when instantiating the card after Start has run
    // See PrefabInstantiator
    public void setCardInfo(CardInfo card){
        this.cardInfo = card;
    }

}
