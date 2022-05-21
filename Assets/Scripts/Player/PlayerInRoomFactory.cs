using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInRoomFactory : MonoBehaviour
{
    public void generatePlayer(){
        LocationStore playerLoc = GameObject.FindGameObjectWithTag("PlayerStore").GetComponent<LocationStore>();
        PrefabStore prefabStore = GameObject.Find("PrefabStore").GetComponent<PrefabStore>();
        Object.Instantiate(
            prefabStore.getPrefabByName(global.PlayerData.roomPrefabName),
            playerLoc.getLoc(1), 
            Quaternion.identity);
    }

}
