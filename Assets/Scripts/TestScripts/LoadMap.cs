using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMap : MonoBehaviour
{
    public MapVariableSO mapReference;
    public EncounterReference encounterReference;

    public void loadMap() {
        if (mapReference.GetValue() == null) {
            Debug.LogError("Map Reference Value is null!");
            return;
        }
        Map map = mapReference.GetValue();
        if (map.encounters.Count == 0) {
            Debug.LogError("Map has no encounters!");
            return;
        }
        encounterReference.Value = map.encounters[0].Value;
        string sceneName = encounterReference.Value.getEncounterType() == EncounterType.Enemy ? "PlaceholderEnemyEncounter" : "PlaceholderShopEncounter";
        SceneManager.LoadScene(sceneName);
    }
}
