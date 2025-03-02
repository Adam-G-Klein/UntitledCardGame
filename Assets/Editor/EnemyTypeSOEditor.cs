using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyTypeSO))]
public class EnemyTypeSOEditor : Editor {

    EffectStepName stepName = EffectStepName.Default;
    int index = 0;
    PatternToEdit patternToEdit;

    public override void OnInspectorGUI() {
        EnemyTypeSO enemyType = (EnemyTypeSO) target;
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Effect Step Controls");
        EditorGUILayout.Space(5);

        stepName = (EffectStepName) EditorGUILayout.EnumPopup(
            "New effect",
            stepName);
        index = EditorGUILayout.IntField("Action Index", index);
        patternToEdit = (PatternToEdit) EditorGUILayout.EnumPopup("Which Pattern to Edit?", patternToEdit);
        if (GUILayout.Button("Add Enemy Pattern Effect")) {
            EffectStep newEffect = InstantiateFromClassname.Instantiate<EffectStep>(
                stepName.ToString(),
                new object[] {});

            if(newEffect == null) {
                Debug.LogError("Failed to instantiate effect step, " +
                "please check Scripts/Effects/EffectSteps/* to verify the className for the  " +
                " and verify that the arguments set in the editor correspond to " +
                " the arguments in the constructor");
                return;
            }
            switch (patternToEdit) {
            case PatternToEdit.DefaultPattern:
                enemyType.enemyPattern.behaviors[index].effectSteps.Add(newEffect);
                break;
            case PatternToEdit.BelowHalfHPPattern:
                enemyType.belowHalfHPEnemyPattern.behaviors[index].effectSteps.Add(newEffect);
                break;
            case PatternToEdit.AdaptWhenAlonePattern:
                enemyType.adaptWhenAloneEnemyPattern.behaviors[index].effectSteps.Add(newEffect);
                break;
            }
        }
        if(GUILayout.Button("Add Tooltip")) {
            enemyType.tooltip = new TooltipViewModel();
        }

        if (GUILayout.Button("Add Enemy Ability Effect")) {
            EffectStep newEffect = InstantiateFromClassname.Instantiate<EffectStep>(
                stepName.ToString(),
                new object[] {});

            if(newEffect == null) {
                Debug.LogError("Failed to instantiate effect step, " +
                "please check Scripts/Effects/EffectSteps/* to verify the className for the  " +
                " and verify that the arguments set in the editor correspond to " +
                " the arguments in the constructor");
            }
            else {
                if (enemyType.abilities == null) {
                    enemyType.abilities = new List<EntityAbility>();
                }

                if (index > enemyType.abilities.Count) {
                    Debug.LogError("Ability index given is outside range");
                }

                if (enemyType.abilities[index].effectSteps == null) {
                    enemyType.abilities[index].effectSteps = new List<EffectStep>();
                }

                enemyType.abilities[index].effectSteps.Add(newEffect);
            }
        }
        if (GUILayout.Button("Add New Ability")) {
            if (enemyType.abilities == null) {
                enemyType.abilities = new List<EntityAbility>();
            }
            enemyType.abilities.Add(new EntityAbility());
        }
    }

    public enum PatternToEdit {
        DefaultPattern,
        BelowHalfHPPattern,
        AdaptWhenAlonePattern,
    }
}
