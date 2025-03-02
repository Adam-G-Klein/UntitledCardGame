using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShopManager))]
[CanEditMultipleObjects]
public class ShopManagerEditor : Editor {
    public EncounterVariableSO encounterToSet;
    public CompanionTypeSO companionToBuy;

    public override void OnInspectorGUI() {
        ShopManager shopManager = (ShopManager) serializedObject.targetObject;
        DrawDefaultInspector();
        
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Test Utilities");
        EditorGUILayout.Space(5);

        companionToBuy = EditorGUILayout.ObjectField(
            companionToBuy,
            typeof(CompanionTypeSO),
            false) as CompanionTypeSO;

        // if(GUILayout.Button("Buy Companion")) {
        //     if(companionToBuy != null) {
        //         ShopManager.Instance.ProcessCompanionBuyRequest(
        //             new CompanionBuyRequest(
        //                 new Companion(companionToBuy),
        //                 0,
        //                 null));
        //     } else {
        //         Debug.LogWarning("Must set companion to buy first!");
        //     }
        // }
        
        encounterToSet = EditorGUILayout.ObjectField(
            encounterToSet,
            typeof(EncounterVariableSO),
            false) as EncounterVariableSO;

        if (GUILayout.Button("Set Active Encounter")) {
            if (encounterToSet != null)
                shopManager.gameState.activeEncounter.SetValue(encounterToSet);
            else
                Debug.LogWarning("Must set encounter to set first!");
            
        }
        
    }
}
