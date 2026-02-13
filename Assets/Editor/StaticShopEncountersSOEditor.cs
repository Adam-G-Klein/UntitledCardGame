using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StaticShopEncountersSO))]
public class StaticShopEncountersSOEditor : Editor {

    public override void OnInspectorGUI() {
        StaticShopEncountersSO staticShopEncounters = (StaticShopEncountersSO) target;
        DrawDefaultInspector();

        if (GUILayout.Button("Add Shop Encounter")) {
            StaticShopEncounter newEncounter = new StaticShopEncounter();
            staticShopEncounters.shopEncounters.Add(newEncounter);
        }
    }
}
