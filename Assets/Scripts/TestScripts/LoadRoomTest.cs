using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadRoomTest : MonoBehaviour
{
    public RoomManager roomManager;
    public bool loadRoom = false;

    // Update is called once per frame
    void Update()
    {
        if (loadRoom)
        {
            roomManager.loadRoom(new DefaultRoom());
            loadRoom = false;
        }
    }
}
