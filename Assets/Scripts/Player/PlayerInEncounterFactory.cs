using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInEncounterFactory 
{
    public PlayerInEncounterFactory() { }
    public void generatePlayer(Player player) {
        LocationStore playerLoc = GameObject.FindGameObjectWithTag("PlayerStore").GetComponent<LocationStore>();
        PrefabStore prefabStore = GameObject.Find("PrefabStore").GetComponent<PrefabStore>();
        GameObject playerGameObject = Object.Instantiate(
            prefabStore.getPrefabByName(PlayerData.encounterPrefabName),
            playerLoc.getLoc(), 
            Quaternion.identity);
        playerGameObject.GetComponent<PlayerData>().setPlayer(player);
    }

}
