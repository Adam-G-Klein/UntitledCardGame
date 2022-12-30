using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CardsDealtEvent))]
public class CardsDealtGameEventEditor : Editor
{
    CardInfo cardInfo = null;
    string companionId = "unset ID";

    public override void OnInspectorGUI() {
        CardsDealtEvent cardsDealtEvent = (CardsDealtEvent) target;
        DrawDefaultInspector();


        EditorGUILayout.LabelField("The card ScriptableObject");
        cardInfo = EditorGUILayout.ObjectField(
            cardInfo,
            typeof(CardInfo),
            false) as CardInfo;

        EditorGUILayout.LabelField("The companion to attach this card's stats to");
        companionId = EditorGUILayout.TextField(companionId);
        
        if (GUILayout.Button("Test Raise Event")) {
            CompanionManager companionManager = GameObject.Find("CompanionManager").GetComponent<CompanionManager>();
            CompanionInstance companionInstance = companionManager.getCompanionInstanceById(companionId);
            // If you're getting a null pointer error here, it's probably because the id pasted
            // into the field above isn't corresponding to an id the companionManager was made 
            // aware of through a companionInstantiated event
            CombatEntityInEncounterStats stats = companionInstance.stats;
            InCombatDeck deckFrom = companionInstance.inCombatDeck;
            if(cardInfo != null) {
                cardsDealtEvent.Raise(new CardsDealtEventInfo(
                    new List<CardInfo>() { cardInfo },
                    deckFrom,
                    stats
                ));
            }
        }
    }
}
