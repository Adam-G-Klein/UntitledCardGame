using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CardsDealtEvent))]
public class CardsDealtGameEventEditor : Editor
{
    CardInfo cardInfo = null;

    public override void OnInspectorGUI() {
        CardsDealtEvent cardsDealtEvent = (CardsDealtEvent) target;
        DrawDefaultInspector();

        cardInfo = EditorGUILayout.ObjectField(
            cardInfo,
            typeof(CardInfo),
            false) as CardInfo;
        
        if (GUILayout.Button("Test Raise Event")) {
            if(cardInfo != null) {
                cardsDealtEvent.Raise(new CardsDealtEventInfo(
                    new List<CardInfo>() { cardInfo }
                ));
            }
        }
    }
}
