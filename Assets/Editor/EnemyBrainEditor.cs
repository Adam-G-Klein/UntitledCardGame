using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyBrain))]
public class EnemyBrainEditor : Editor {

    string enemyBehaviorClassName = "DefaultEnemyBehavior";
    public override void OnInspectorGUI() {
        EnemyBrain enemyBrain = (EnemyBrain) target;
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Enemy Behavior Controls");
        EditorGUILayout.Space(5);

        enemyBehaviorClassName = EditorGUILayout.TextField(
            "New procedure classname",
            enemyBehaviorClassName);

        if (GUILayout.Button("Add Enemy Behavior")) {
            EnemyBehavior newBehavior = InstantiateFromClassname.Instantiate<EnemyBehavior>(
                enemyBehaviorClassName, 
                new object[] {});

            if(newBehavior == null) {
                Debug.LogError("Failed to instantiate enemy behavior, " +
                "please check Scripts/Enemies/EnemyBrains/* to verify the className ");
            }

            if(enemyBrain.behaviors == null) {
                enemyBrain.behaviors = new List<EnemyBehavior>();
            }

            enemyBrain.behaviors.Add(newBehavior);
            
            // These three calls cause the asset to actually be modified
            // on disc when we hit the button
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(enemyBrain);
            AssetDatabase.SaveAssets();
        }
    }

}
