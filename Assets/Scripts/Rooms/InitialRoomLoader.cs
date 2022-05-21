using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialRoomLoader : MonoBehaviour
{
    private Manager manager;

    void Start(){
        //grab the roomManager, doesn't matter which one
        manager = GameObject.FindGameObjectWithTag("Managers").GetComponent<RoomManager>();
    }

    public void loadStartRoom(){
        manager.loadScene(new MapGenerationLoadScene());
    }

}
