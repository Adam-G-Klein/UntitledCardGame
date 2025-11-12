using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLoadBossfight : MonoBehaviour
{
    public GameStateVariableSO gameStateVariableSO;
    public MapGeneratorSO mapGenerator;

    // Start is called before the first frame update
    void Start()
    {
        Map map = mapGenerator.generateMap();
        gameStateVariableSO.map.SetValue(map);
        gameStateVariableSO.currentEncounterIndex = 0;
        gameStateVariableSO.activeEncounter.SetValue(map.encounters[0]);
        gameStateVariableSO.SetLocation(Location.STARTING_TEAM);
        gameStateVariableSO.LoadNextLocation(true);
    }
}
