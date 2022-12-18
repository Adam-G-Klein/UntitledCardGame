using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CardInfo))]
public class CardInfoEditor : Editor {

    string effectProcedureClassName = "SimpleEffect";
    public override void OnInspectorGUI() {
        CardInfo cardInfo = (CardInfo) target;
        DrawDefaultInspector();
        if(cardInfo.EffectProcedures == null) {
            Debug.Log("EffectProcedures was null, setting to empty list");
            cardInfo.EffectProcedures = new List<EffectProcedure>();
        }

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
                if(cardInfo.EffectProcedures == null) {
                    Debug.Log("EffectProcedures was null, setting to empty list");
                    cardInfo.EffectProcedures = new List<EffectProcedure>();
                }
                cardInfo.EffectProcedures.Add(newProcedure);
            }
            
            // These three calls cause the asset to actually be modified
            // on disc when we hit the button
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(cardInfo);
            AssetDatabase.SaveAssets();
        }
    }

}
