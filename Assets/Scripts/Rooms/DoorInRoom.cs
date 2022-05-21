using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DoorInRoom : MonoBehaviour
{
    // will be set by the room factory
    private Room connectedRoom;
    private RoomManager roomManager;
    // Start is called before the first frame update
    void Start()
    {
        roomManager = GameObject.FindGameObjectWithTag("Managers").GetComponent<RoomManager>();
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("door collision, going to " + connectedRoom.getSceneString());
        roomManager.loadScene(connectedRoom);
    }

    public void setRoom(Room room){
        this.connectedRoom = room;
    }



    
}
