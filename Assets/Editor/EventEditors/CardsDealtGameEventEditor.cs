using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CardsDealtGameEvent))]
public class CardsDealtGameEventEditor : Editor
{
    CardInfo cardInfo = null;

    public override void OnInspectorGUI() {
        CardsDealtGameEvent cardsDealtEvent = (CardsDealtGameEvent) target;
        DrawDefaultInspector();

        cardInfo = EditorGUILayout.ObjectField(
            cardInfo,
            typeof(CardInfo),
            false) as CardInfo;
        
        if (GUILayout.Button("Test Raise Event")) {
            cardsDealtEvent.Raise(new CardsDealtEventInfo(
                new List<CardInfo>() { cardInfo }
            ));
        }
    }
}
