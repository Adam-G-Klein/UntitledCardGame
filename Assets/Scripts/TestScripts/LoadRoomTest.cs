using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadRoomTest : MonoBehaviour
{
    public RoomManager roomManager;
    public bool loadDefaultRoom = false;
    public bool loadTestRoom = false;

    // Update is called once per frame
    void Update()
    {
        if (loadDefaultRoom)
        {
            roomManager.loadRoom(new DefaultRoom());
            loadDefaultRoom = false;
        }
        else if (loadTestRoom)
        {
            roomManager.loadRoom(new TestRoom());
            loadTestRoom = false;
        }
    }
}
