using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CardType))]
public class CardTypeEditor : Editor {

    string effectProcedureClassName = "CombatEffectProcedure";
    public override void OnInspectorGUI() {
        CardType cardType = (CardType) target;
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Effect Procedure Controls");
        EditorGUILayout.Space(5);

        effectProcedureClassName = EditorGUILayout.TextField(
            "New procedure classname",
            effectProcedureClassName);

        if (GUILayout.Button("Add Effect Procedure")) {
            EffectProcedure newProcedure = InstantiateFromClassname.Instantiate<EffectProcedure>(
                effectProcedureClassName, 
                new object[] {});

            if(newProcedure == null) {
                Debug.LogError("Failed to instantiate effect procedure, " +
                "please check Scripts/Cards/CardEffectProcedures/* to verify the className for the  " +
                " and verify that the arguments set in the editor correspond to " +
                " the arguments in the constructor");
            }
            else {
                cardType.EffectProcedures.Add(newProcedure);
            }
            
            // These three calls cause the asset to actually be modified
            // on disc when we hit the button
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(cardType);
            AssetDatabase.SaveAssets();
        }
    }

}
