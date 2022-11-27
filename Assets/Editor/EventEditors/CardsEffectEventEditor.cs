using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CardEffectEvent))]
public class CardEffectEventEditor : Editor
{
    CardEffectName effectName;
    int scale = 1;
    [SerializeField]
    string targetId = "Unset Id";

    public override void OnInspectorGUI() {
        CardEffectEvent cardEffectEvent = (CardEffectEvent) target;
        DrawDefaultInspector();
        EditorGUILayout.LabelField("Effect Name");

        effectName  = (CardEffectName) EditorGUILayout.EnumPopup(
            effectName);
        
        EditorGUILayout.LabelField("Scale");
        scale = EditorGUILayout.IntField(
            scale);

        EditorGUILayout.LabelField("Target Id");
        targetId = EditorGUILayout.TextField(
            targetId);

        if (GUILayout.Button("Raise Card Effect Event")) {
            //TODO, figure out how to raise at end of frame
            cardEffectEvent.Raise(new CardEffectEventInfo(
                effectName,
                scale,
                new List<string>() { targetId }
            ));
        }
    }
}
