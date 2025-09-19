using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PowerSO))]
public class PowerEditor : Editor
{
    EffectStepName stepName = EffectStepName.Default;
    int abilityIndex = 0;

    public override void OnInspectorGUI()
    {
        PowerSO powerSO = (PowerSO)target;
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Effect Step Controls");
        EditorGUILayout.Space(5);

        stepName = (EffectStepName)EditorGUILayout.EnumPopup(
            "New effect",
            stepName);
        abilityIndex = EditorGUILayout.IntField("Ability Index", abilityIndex);

        if (GUILayout.Button("Add Ability Effect"))
        {
            EffectStep newEffect = InstantiateFromClassname.Instantiate<EffectStep>(
                stepName.ToString(),
                new object[] { });

            if (newEffect == null)
            {
                Debug.LogError("Failed to instantiate effect step, " +
                "please check Scripts/Effects/EffectSteps/* to verify the className for the  " +
                " and verify that the arguments set in the editor correspond to " +
                " the arguments in the constructor");
            }
            else
            {
                if (powerSO.abilities == null)
                {
                    powerSO.abilities = new List<EntityAbility>();
                }

                if (abilityIndex > powerSO.abilities.Count)
                {
                    Debug.LogError("Ability index given is outside range");
                }

                if (powerSO.abilities[abilityIndex].effectSteps == null)
                {
                    powerSO.abilities[abilityIndex].effectSteps = new List<EffectStep>();
                }

                powerSO.abilities[abilityIndex].effectSteps.Add(newEffect);
            }
            save(powerSO);
        }

        if (GUILayout.Button("Add New Ability"))
        {
            if (powerSO.abilities == null)
            {
                powerSO.abilities = new List<EntityAbility>();
            }
            powerSO.abilities.Add(new EntityAbility());
            save(powerSO);
        }

        // if (GUILayout.Button("Add Tooltip"))
        // {
        //     powerSO.tooltip = new TooltipViewModel();
        //     save(powerSO);
        // }
    }
    private void save(PowerSO powerSO) {
        // These three calls cause the asset to actually be modified
        // on disc when we hit the button
        AssetDatabase.Refresh();
        EditorUtility.SetDirty(powerSO);
        AssetDatabase.SaveAssets();

    }
}

