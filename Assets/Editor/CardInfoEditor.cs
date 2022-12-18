using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CardInfo))]
public class CardInfoEditor : Editor {

    SimpleEffectName simpleEffectName = SimpleEffectName.Unset;
    string effectProcedureClassName = "";
    public override void OnInspectorGUI() {
        CardInfo cardInfo = (CardInfo) target;
        DrawDefaultInspector();
        if(cardInfo.EffectProcedures == null)
            cardInfo.EffectProcedures = new List<EffectProcedure>();
        EditorGUILayout.LabelField("EffectProcedureCount: " + cardInfo.EffectProcedures.Count);

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Effect Procedure Controls");
        EditorGUILayout.Space(5);

        effectProcedureClassName = EditorGUILayout.TextField(
            "Enter new procedure class name",
            effectProcedureClassName);

        EditorGUILayout.LabelField("Effect Procedure Constructor Arguments");
        EditorGUILayout.LabelField("Important: Do not include arguments not in the constructor");

        object[] instantiationArgs = getInstantiationArgs();
        

        if (GUILayout.Button("Add Effect Procedure")) {
            if (cardInfo.EffectProcedures == null)
                cardInfo.EffectProcedures = new List<EffectProcedure>();
            
            EffectProcedure newProcedure = InstantiateFromClassname.Instantiate<EffectProcedure>(
                effectProcedureClassName, 
                instantiationArgs);

            if(newProcedure == null) {
                cardInfo.EffectProcedureNames.Add("Error instantiating effect procedure, see logs");
            }
            else {
                cardInfo.EffectProcedureNames.Add(effectProcedureClassName);
                if(cardInfo.EffectProcedures == null)
                    cardInfo.EffectProcedures = new List<EffectProcedure>();
                cardInfo.EffectProcedures.Add(newProcedure);
            }
            
            // These three calls cause the asset to actually be modified
            // on disc when we hit the button
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(cardInfo);
            AssetDatabase.SaveAssets();
        }
    }

    private object[] getInstantiationArgs() {

        simpleEffectName = (SimpleEffectName) EditorGUILayout.EnumPopup(
            "Simple Effect Name", 
            simpleEffectName);

        List<EntityType> validTargets = new List<EntityType>() {
            // TODO selectors
            EntityType.Enemy,
            EntityType.Companion
        };

        int baseScale = EditorGUILayout.IntField("Base Scale", 0);

        List<object> argsList = new List<object>();

        if(simpleEffectName != SimpleEffectName.Unset) {
            argsList.Add(simpleEffectName);
            argsList.Add(baseScale);
            argsList.Add(validTargets);
        }

        return argsList.ToArray();
    }
}
