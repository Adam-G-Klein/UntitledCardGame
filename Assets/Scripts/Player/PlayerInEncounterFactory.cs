using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInEncounterFactory 
{
    public PlayerInEncounterFactory(){}
    public void generatePlayer(){
        LocationStore playerLoc = GameObject.FindGameObjectWithTag("PlayerStore").GetComponent<LocationStore>();
        PrefabStore prefabStore = GameObject.Find("PrefabStore").GetComponent<PrefabStore>();
        Object.Instantiate(
            prefabStore.getPrefabByName(global.PlayerData.encounterPrefabName),
            playerLoc.getLoc(), 
            Quaternion.identity);
    }

}
