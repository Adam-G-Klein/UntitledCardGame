using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialRoomLoader : MonoBehaviour
{
    private RoomManager roomManager;

    void Start(){
        roomManager = GameObject.FindGameObjectWithTag("Managers").GetComponent<RoomManager>();
    }

    public void loadStartRoom(){
        roomManager.loadRoom(new StartRoom());
    }

}
