using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CardEffectEvent))]
public class CardEffectEventEditor : Editor
{
    SimpleEffectName effectName;
    int scale = 1;
    [SerializeField]
    string targetId = "Unset Id";
    bool needsTargets = true;

    public override void OnInspectorGUI() {
        CardEffectEvent cardEffectEvent = (CardEffectEvent) target;
        DrawDefaultInspector();
        EditorGUILayout.LabelField("Effect Name");

        effectName  = (SimpleEffectName) EditorGUILayout.EnumPopup(
            effectName);
        
        EditorGUILayout.LabelField("Scale");
        scale = EditorGUILayout.IntField(
            scale);

        EditorGUILayout.LabelField("Target Id");
        targetId = EditorGUILayout.TextField(
            targetId);

        EditorGUILayout.LabelField("Needs Targets");
        needsTargets = EditorGUILayout.Toggle(
            needsTargets);

        if (GUILayout.Button("Raise Card Effect Event")) {
            //TODO, figure out how to raise at end of frame
            Debug.LogWarning("Not raising event, because it's not working");

            /*

            cardEffectEvent.Raise(new CardEffectEventInfo(
                effectName,
                scale,
                needsTargets ? new List<TargettableEntity> {targetId} : new List<string>()
            ));
            */
        }
    }
}
