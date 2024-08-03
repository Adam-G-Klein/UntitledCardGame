using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayableCard))]
public class PlayableCardEditor : Editor {

    CardType cardTypeSO;
    CompanionTypeSO companionTypeSO;

    public override void OnInspectorGUI() {
        PlayableCard playableCard = (PlayableCard) target;
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Effect Step Controls");
        EditorGUILayout.Space(5);

        cardTypeSO = (CardType) EditorGUILayout.ObjectField(
            "Card Type",
            cardTypeSO,
            typeof(CardType),
            false);

        companionTypeSO = (CompanionTypeSO) EditorGUILayout.ObjectField(
            "Companion Type From",
            companionTypeSO,
            typeof(CompanionTypeSO),
            false);

        

        if (GUILayout.Button("Create card")) {
            if(cardTypeSO == null) {
                Debug.LogError("No card type selected");
            }
            else {
                playableCard.card = new Card(cardTypeSO, companionTypeSO);
            }
        }
    }
}
