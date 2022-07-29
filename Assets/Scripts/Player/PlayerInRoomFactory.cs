using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInRoomFactory
{
    public PlayerInRoomFactory(){}
    public void generatePlayer(){
        LocationStore playerLoc = GameObject.FindGameObjectWithTag("PlayerStore").GetComponent<LocationStore>();
        PrefabStore prefabStore = GameObject.Find("PrefabStore").GetComponent<PrefabStore>();
        Object.Instantiate(
            prefabStore.getPrefabByName(PlayerData.roomPrefabName),
            playerLoc.getLoc(), 
            Quaternion.identity);
    }

}
