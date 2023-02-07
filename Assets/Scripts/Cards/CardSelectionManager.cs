using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Can make a card selection manager that extends this class
// could pass a reference to it in with EffectProcedureContext
// can override requestTarget to first ...
// what if we use UIState?
// Set the UIState to card selecting... and then every click on 
// a card will raise an effect Target Supplied Event
public class CardSelectionManager: TargetProvider {
    public GameObject cardViewUIPrefab;
    public override void requestTarget(List<EntityType> validTargets, TargetRequester requester, List<TargettableEntity> disallowedTargets = null){
        this.targettingCoroutine = getTargetCoroutine(validTargets, requester, disallowedTargets);
        StartCoroutine(targettingCoroutine);
    }

    public void requestCardTargetFromList(List<Card> cards, TargetRequester requester) {
        this.targettingCoroutine = getCardTargetCoroutine(cards, requester);
        StartCoroutine(targettingCoroutine);
    }

    private IEnumerator getCardTargetCoroutine(List<Card> cards, TargetRequester requester) {
        requestedTarget = null;
        displayCardGroup(cards);
        yield return null;
    }

    private void displayCardGroup(List<Card> cards) {
        GameObject cardSelectionUI = Instantiate(cardViewUIPrefab);
        CardViewUI cardViewUI = cardSelectionUI.GetComponent<CardViewUI>();
        cardViewUI.Setup(cards);
    }


}