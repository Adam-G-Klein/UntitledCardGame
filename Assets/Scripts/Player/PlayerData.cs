using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour 
{
    // using the same prefab for now
    public static string encounterPrefabName = "PlayerWithHealthBar";
    public static string roomPrefabName = "Player";

    private Player player;

    public void setPlayer(Player player) {
        this.player = player;
    }

    public Player getPlayer() {
        return player;
    }

    public Entity getEntity() {
        return player;
    }
}