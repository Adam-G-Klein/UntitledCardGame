using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CardsDealtEvent))]
public class CardsDealtGameEventEditor : Editor
{
    CardType cardInfo = null;
    string companionId = "unset ID";

    public override void OnInspectorGUI() {
        CardsDealtEvent cardsDealtEvent = (CardsDealtEvent) target;
        DrawDefaultInspector();


        EditorGUILayout.LabelField("The card ScriptableObject");
        cardInfo = EditorGUILayout.ObjectField(
            cardInfo,
            typeof(CardType),
            false) as CardType;

        EditorGUILayout.LabelField("The companion to attach this card's stats to");
        companionId = EditorGUILayout.TextField(companionId);
        
        if (GUILayout.Button("Test Raise Event")) {
            CompanionInstance companionInstance = CombatEntityManager.Instance
                .getCompanionInstanceById(companionId);
            // If you're getting a null pointer error here, it's probably because the id pasted
            // into the field above isn't corresponding to an id the CombatEntityManager was made 
            // aware of
            CombatEntityInEncounterStats stats = companionInstance.stats;
            InCombatDeck deckFrom = companionInstance.inCombatDeck;
            if(cardInfo != null) {
                cardsDealtEvent.Raise(new CardsDealtEventInfo(
                    new List<Card>() { new Card(cardInfo) },
                    companionInstance
                ));
            }
        }
    }
}
